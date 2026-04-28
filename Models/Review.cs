using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workify_Full.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public int Rating  { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        [Required]
        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        [Required]
        public string ReviewerId { get; set; } = string.Empty;
        [ForeignKey("ReviewerId")]
        public virtual ApplicationUser? Reviewer { get; set; }

        [Required]
        public string RevieweeId { get; set; } = string.Empty;
        [ForeignKey("RevieweeId")]
        public virtual ApplicationUser? Reviewee { get; set; }
    }
}
