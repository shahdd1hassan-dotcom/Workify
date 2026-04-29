using Workify_Full.Models;

namespace Workify_Full.ViewModels
{
    public class JobDetailsViewModel
    {
        public Job Job { get; set; } = null!;

        // ── Client info (denormalised for the view) ──
        public string ClientName { get; set; } = string.Empty;
        public string ClientInitials { get; set; } = string.Empty;
        public DateTime ClientMemberSince { get; set; }
        public int ClientTotalJobs { get; set; }

        // ── Proposal stats ──
        public int ProposalCount { get; set; }

        // ── Contextual flags ──
        public bool IsOwner { get; set; }
        public bool IsClient { get; set; }
        public bool HasAlreadyApplied { get; set; }
        public bool HasAcceptedProposals { get; set; }
    }
}
