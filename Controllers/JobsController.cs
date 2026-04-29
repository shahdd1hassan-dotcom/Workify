using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workify_Full.Data;
using Workify_Full.Models;
using Workify_Full.Models.Enum;
using Workify_Full.ViewModels;

namespace Workify_Full.Controllers
{
    [Authorize]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─────────────────────────────────────────────
        //  INDEX — paginated + filtered list of open jobs
        // ─────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(
            string? searchQuery,
            string? category,
            string? experienceLevel,
            string? budgetType,
            decimal? budgetMax,
            int page = 1)
        {
            const int pageSize = 9;

            // Start with all open jobs, include client for display
            IQueryable<Job> query = _db.Jobs
                .Include(j => j.Client)
                .Include(j => j.Proposals)
                .Where(j => j.Status == JobStatus.Open);

            // ── Filter chains ──
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string q = searchQuery.Trim().ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(q) ||
                    j.Description.ToLower().Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(j => j.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                query = query.Where(j => j.ExperienceLevel == experienceLevel);
            }

            if (!string.IsNullOrWhiteSpace(budgetType))
            {
                query = query.Where(j => j.BudgetType == budgetType);
            }

            if (budgetMax.HasValue && budgetMax.Value > 0)
            {
                query = query.Where(j => j.BudgetMax <= budgetMax.Value);
            }

            // ── Ordering ──
            query = query.OrderByDescending(j => j.CreatedAt);

            // ── Pagination ──
            int totalCount = query.Count();
            var jobs = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ── Determine current user role ──
            bool isClient = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                if (user != null)
                    isClient = user.Role == UserRole.Client;
            }

            var vm = new JobIndexViewModel
            {
                Jobs = jobs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                Category = category,
                ExperienceLevel = experienceLevel,
                BudgetType = budgetType,
                BudgetMax = budgetMax,
                IsClient = isClient
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────
        //  DETAILS — single job + client info + proposals
        // ─────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var job = _db.Jobs
                .Include(j => j.Client)
                .Include(j => j.Proposals)
                .FirstOrDefault(j => j.JobId == id);

            if (job == null)
                return NotFound();

            // Client info
            var client = job.Client;
            string initials = "??";
            if (client != null && !string.IsNullOrEmpty(client.FullName))
            {
                var parts = client.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : parts[0][..Math.Min(2, parts[0].Length)].ToUpper();
            }

            int clientTotalJobs = client != null
                ? _db.Jobs.Count(j => j.ClientId == client.Id)
                : 0;

            // Current user context
            bool isOwner = false;
            bool isClient = false;
            bool hasAlreadyApplied = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var currentUser = _userManager.GetUserAsync(User).Result;
                isOwner = job.ClientId == userId;
                isClient = currentUser?.Role == UserRole.Client;
                hasAlreadyApplied = job.Proposals.Any(p => p.FreelancerId == userId);
            }

            bool hasAcceptedProposals = job.Proposals
                .Any(p => p.Status == ProposalStatus.Hired);

            var vm = new JobDetailsViewModel
            {
                Job = job,
                ClientName = client?.FullName ?? "Unknown",
                ClientInitials = initials,
                ClientMemberSince = client?.CreatedAt ?? DateTime.UtcNow,
                ClientTotalJobs = clientTotalJobs,
                ProposalCount = job.Proposals.Count,
                IsOwner = isOwner,
                IsClient = isClient,
                HasAlreadyApplied = hasAlreadyApplied,
                HasAcceptedProposals = hasAcceptedProposals
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────
        //  CREATE (GET) — show the post-job form
        // ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null || user.Role != UserRole.Client)
                return RedirectToAction("Index");

            return View(new JobCreateViewModel());
        }

        // ─────────────────────────────────────────────
        //  CREATE (POST) — validate and save new job
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JobCreateViewModel model)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null || user.Role != UserRole.Client)
                return Forbid();

            // Custom validation: BudgetMax >= BudgetMin
            if (model.BudgetMax < model.BudgetMin)
            {
                ModelState.AddModelError("BudgetMax", "Maximum budget must be ≥ minimum budget.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var job = new Job
            {
                Title = model.Title,
                Description = model.Description,
                Category = model.Category,
                BudgetMin = model.BudgetMin,
                BudgetMax = model.BudgetMax,
                BudgetType = model.BudgetType,
                ExperienceLevel = model.ExperienceLevel,
                Status = JobStatus.Open,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = model.ExpiresAt,
                ClientId = user.Id
            };

            _db.Jobs.Add(job);
            _db.SaveChanges();

            TempData["Success"] = "Job posted successfully!";
            return RedirectToAction("Details", new { id = job.JobId });
        }

        // ─────────────────────────────────────────────
        //  CLOSE — client sets job status to Closed
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Close(int id)
        {
            var userId = _userManager.GetUserId(User);
            var job = _db.Jobs.FirstOrDefault(j => j.JobId == id);

            if (job == null)
                return NotFound();

            if (job.ClientId != userId)
                return Forbid();

            job.Status = JobStatus.Closed;
            _db.SaveChanges();

            TempData["Success"] = "Job closed successfully.";
            return RedirectToAction("Details", new { id });
        }

        // ─────────────────────────────────────────────
        //  DELETE — client deletes job (only if no hired proposals)
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var job = _db.Jobs
                .Include(j => j.Proposals)
                .FirstOrDefault(j => j.JobId == id);

            if (job == null)
                return NotFound();

            if (job.ClientId != userId)
                return Forbid();

            // Block deletion if any proposal has been accepted/hired
            bool hasHired = job.Proposals.Any(p => p.Status == ProposalStatus.Hired);
            if (hasHired)
            {
                TempData["Error"] = "Cannot delete a job that has accepted proposals.";
                return RedirectToAction("Details", new { id });
            }

            _db.Jobs.Remove(job);
            _db.SaveChanges();

            TempData["Success"] = "Job deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
