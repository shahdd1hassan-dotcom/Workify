using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Workify_Full.Models.Enum;

namespace Workify_Full.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Skills { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Country { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // One-to-one
        public Wallet? Wallet { get; set; }

        // Navigation collections
        public ICollection<Job> PostedJobs { get; set; } = new List<Job>();
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<Contract> ClientContracts { get; set; } = new List<Contract>();
        public ICollection<Contract> FreelancerContracts { get; set; } = new List<Contract>();
    }
}
