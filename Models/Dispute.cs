using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workify_Full.Models.Enum;
using System.Collections.Generic;

namespace Workify_Full.Models
{
    public class Dispute
    {
        [Key]
        public int DisputeId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DisputeStatus Status { get; set; } = DisputeStatus.Open;
        public DateTime FiledAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string? AdminNotes { get; set; }

        [Required]
        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public Contract? Contract { get; set; }

        [Required]
        public string FiledById { get; set; } = string.Empty;
        [ForeignKey("FiledById")]
        public ApplicationUser? FiledBy { get; set; }

        public int? MilestoneId { get; set; }
        [ForeignKey("MilestoneId")]
        public Milestone? Milestone { get; set; }

        public ICollection<DisputeMessage> DisputeMessages { get; set; } = new List<DisputeMessage>();
    }
}
