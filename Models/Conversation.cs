using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Workify_Full.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public Contract? Contract { get; set; }

        [Required]
        public string ParticipantAId { get; set; } = string.Empty;
        [ForeignKey("ParticipantAId")]
        public ApplicationUser? ParticipantA { get; set; }

        [Required]
        public string ParticipantBId { get; set; } = string.Empty;
        [ForeignKey("ParticipantBId")]
        public ApplicationUser? ParticipantB { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
