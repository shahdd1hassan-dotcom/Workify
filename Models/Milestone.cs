using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using Workify_Full.Models.Enum;

namespace Workify_Full.Models
{
    public class Milestone
{
        [Key]
        public required int MilestoneId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Funded;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? DeliverableNotes { get; set; }

    // Foreign Key
    [Required]
    public int ContractId { get; set; }
    [ForeignKey("ContractId")]
    public Contract? Contract { get; set; }

        // Navigation Properties

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

}