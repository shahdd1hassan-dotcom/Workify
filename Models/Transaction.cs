using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using Workify_Full.Models.Enum;
namespace Workify_Full.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; } 

        public string? Description { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key -> Wallet
        public int WalletId { get; set; }
        [ForeignKey("WalletId")]
        public Wallet? Wallet { get; set; }

        // Optional Foreign Key -> Milestone
        public int? MilestoneId { get; set; }
        [ForeignKey("MilestoneId")]
        public Milestone? Milestone { get; set; }

    }
}
