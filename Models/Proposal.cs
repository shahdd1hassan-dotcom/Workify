using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workify_Full.Models.Enum;

namespace Workify_Full.Models
{
    public class Proposal
    {
        [Key]
        public int ProposalId { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public decimal BidAmount  { get; set; }
        public int DeliveryDays { get; set; }
        public ProposalStatus Status  { get; set; } = ProposalStatus.Pending;
        public DateTime SubmittedAt  { get; set; } = DateTime.UtcNow;

        [Required]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public virtual Job? Job { get; set; }

        [Required]
        public string FreelancerId { get; set; } = string.Empty;
        [ForeignKey("FreelancerId")]
        public virtual ApplicationUser? Freelancer { get; set; }

        public virtual Contract? Contract { get; set; }
    }
}







