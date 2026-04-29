using Workify_Full.Models;

namespace Workify_Full.ViewModels
{
    public class JobIndexViewModel
    {
        // ── Paged results ──
        public List<Job> Jobs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // ── Current filter state (round-trip to the view) ──
        public string? SearchQuery { get; set; }
        public string? Category { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? BudgetType { get; set; }
        public decimal? BudgetMax { get; set; }

        // ── Role flag so the view can show/hide client-only UI ──
        public bool IsClient { get; set; }
    }
}
