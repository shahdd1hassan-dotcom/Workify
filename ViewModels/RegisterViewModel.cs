using System.ComponentModel.DataAnnotations;
using Workify_Full.Models.Enum;

namespace Workify_Full.ViewModels
{
    public class RegisterViewModel
    {
        [Required, Display(Name = "Full Name")]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Client;
    }
}
