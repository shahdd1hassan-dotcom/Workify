using System.ComponentModel.DataAnnotations;

namespace Workify_Full.ViewModels
{
    public class JobCreateViewModel
    {
        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(120, ErrorMessage = "Title cannot exceed 120 characters.")]
        [Display(Name = "Job Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        [Display(Name = "Project Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Minimum budget is required.")]
        [Range(1, 1000000, ErrorMessage = "Budget must be between $1 and $1,000,000.")]
        [Display(Name = "Minimum Budget")]
        public decimal BudgetMin { get; set; }

        [Required(ErrorMessage = "Maximum budget is required.")]
        [Range(1, 1000000, ErrorMessage = "Budget must be between $1 and $1,000,000.")]
        [Display(Name = "Maximum Budget")]
        public decimal BudgetMax { get; set; }

        [Required(ErrorMessage = "Budget type is required.")]
        [Display(Name = "Payment Type")]
        public string BudgetType { get; set; } = "Fixed";

        [Required(ErrorMessage = "Experience level is required.")]
        [Display(Name = "Experience Level")]
        public string ExperienceLevel { get; set; } = "Entry";

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiresAt { get; set; }
    }
}
