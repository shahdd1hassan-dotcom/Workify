using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workify_Full.Models.Enum;
using System.Collections.Generic;

namespace Workify_Full.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public string BudgetType { get; set; } = "Fixed";
        public string ExperienceLevel { get; set; } = "Entry";
        public JobStatus Status { get; set; } = JobStatus.Open;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        // Client FK
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public Contract? Contract { get; set; }
    }
}
