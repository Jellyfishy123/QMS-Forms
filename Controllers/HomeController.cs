using Microsoft.AspNetCore.Mvc;
using QMSForms.Data;
using QMSForms.Models;
using System.Linq;

namespace QMSForms.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // Runs before any action
        // ==========================
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // Check if user is logged in via session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                // Redirect to login page if not logged in
                context.Result = RedirectToAction("Login", "Account");
            }

            base.OnActionExecuting(context);
        }

        // ==========================
        // GET: /Home/Index
        // ==========================
        public IActionResult Index(string status, string type, string project)
        {
            // Get all requests first
            var allRequests = _context.Requests.AsQueryable();

            // For filter dropdowns (distinct values)
            ViewBag.AllStatuses = Enum.GetValues(typeof(RequestStatus)).Cast<RequestStatus>().ToList();
            ViewBag.AllTypes = _context.Requests.Select(r => r.Type).Distinct().ToList();
            ViewBag.AllProjects = _context.Requests.Select(r => r.ProjectCode).Distinct().ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                allRequests = allRequests.Where(r => r.Status.ToString() == status);

            if (!string.IsNullOrEmpty(type))
                allRequests = allRequests.Where(r => r.Type == type);

            if (!string.IsNullOrEmpty(project))
                allRequests = allRequests.Where(r => r.ProjectCode == project);

            // Return the filtered requests, ordered by latest first
            return View(allRequests.OrderByDescending(r => r.RequestDate).ToList());
        }
    }
}