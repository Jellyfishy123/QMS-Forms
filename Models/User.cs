using System;
using System.ComponentModel.DataAnnotations;

namespace QMSForms.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool MustChangePassword { get; set; } = true;

        // 🔐 Store encrypted password
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // First login detection
        public bool IsFirstLogin { get; set; } = true;

        // Password reset support
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}
