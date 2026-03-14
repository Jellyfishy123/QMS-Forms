using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSForms.Data;
using QMSForms.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BCrypt.Net;

namespace QMSForms.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private const string DefaultPassword = "ktc2026";

        public AccountController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ================= LOGIN =================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // First time user
            if (user == null)
            {
                if (password != DefaultPassword)
                {
                    ViewBag.Error = "New users must use the default password";
                    return View();
                }

                user = new User
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                    MustChangePassword = true,
                    IsFirstLogin = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid login attempt";
                return View();
            }

            HttpContext.Session.SetString("UserEmail", user.Email);

            if (user.MustChangePassword)
                return RedirectToAction("ChangePassword");

            return RedirectToAction("Index", "Home");
        }

        // ================= CHANGE PASSWORD =================
        public async Task<IActionResult> ChangePassword()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("Login");

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.MustChangePassword = false;
            user.IsFirstLogin = false;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Requests");
        }

        // ================= FORGOT PASSWORD =================
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Always show same message (security best practice)
            ViewBag.Message = "If this email exists, a reset link has been sent.";

            if (user == null)
                return View();

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Account",
                new { token = user.ResetToken }, Request.Scheme);

            await SendResetEmail(user.Email, resetLink);

            return View();
        }

        // ================= RESET PASSWORD =================
        public IActionResult ResetPassword(string token)
        {
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.ResetToken == model.Token &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid or expired token");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            user.MustChangePassword = false;
            user.IsFirstLogin = false;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Password reset successful. Please login.";
            return RedirectToAction("Login");
        }

        // ================= EMAIL SENDER =================
        private async Task SendResetEmail(string toEmail, string resetLink)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = int.Parse(_config["EmailSettings:Port"]);
            var senderName = _config["EmailSettings:SenderName"];
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var username = _config["EmailSettings:Username"];
            var password = _config["EmailSettings:Password"];

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "QMS Password Reset",
                Body = $"Click the link below to reset your password:\n\n{resetLink}\n\nThis link expires in 15 minutes.",
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);
            await client.SendMailAsync(mail);
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
