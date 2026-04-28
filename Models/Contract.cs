using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workify_Full.Models.Enum;
using System.Collections.Generic;

namespace Workify_Full.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }
        public string Terms { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Active;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        [Required]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public Job? Job { get; set; }

        public int? ProposalId { get; set; }
        [ForeignKey("ProposalId")]
        public Proposal? Proposal { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;
        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }

        [Required]
        public string FreelancerId { get; set; } = string.Empty;
        [ForeignKey("FreelancerId")]
        public ApplicationUser? Freelancer { get; set; }

        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public Dispute? Dispute { get; set; }
    }
}    