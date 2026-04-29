using System.ComponentModel.DataAnnotations;

namespace Workify_Full.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Display(Name = "Skills")]
        [StringLength(500, ErrorMessage = "Skills cannot exceed 500 characters.")]
        public string? Skills { get; set; }

        [Range(0, 100000, ErrorMessage = "Hourly rate must be between 0 and 100,000.")]
        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }

        [StringLength(100)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL.")]
        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }
    }
}
