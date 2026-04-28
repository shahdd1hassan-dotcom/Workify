# Workify — ASP.NET MVC Backend Implementation Guide

> **Tagline:** Work. Without Limits.
> **Stack:** ASP.NET Core MVC · Entity Framework Core · SQL Server · ASP.NET Core Identity
> **Philosophy:** No JWT tokens, no repository pattern, no over-engineering.

---

## 1. Overview & Goal

This guide covers building a clean, efficient ASP.NET Core MVC backend for the **Workify freelance marketplace**. The static frontend already exists — this backend powers all core flows:

- User registration and login (via ASP.NET Core Identity)
- Job posting, browsing, and filtering
- Freelancer proposals and client decisions
- Contracts with milestone-based payment tracking
- Escrow wallet and transaction management
- Real-time messaging between parties
- Dispute filing and resolution
- Admin moderation panel

This is an **MVC Web App project** — use Visual Studio's graphical interface. No manual terminal commands required.

---

## 2. Project Setup

### 2.1 Create the Project

1. **File → New → Project**
2. Select **ASP.NET Core Web App (Model-View-Controller)** → Next
3. Project name: `Workify.Web` | Solution name: `Workify`
4. Framework: **.NET 8.0 (LTS)**
5. Authentication type: **Individual Accounts** (wires ASP.NET Core Identity automatically)
6. Leave all other defaults → **Create**

### 2.2 NuGet Packages

Install via **Tools → NuGet Package Manager → Manage NuGet Packages for Solution**:

| Package | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server provider for EF Core |
| `Microsoft.EntityFrameworkCore.Tools` | Enables migrations from Package Manager Console |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity + EF Core integration |
| `Microsoft.AspNetCore.Identity.UI` | Razor UI pages for login/register scaffolding |

### 2.3 Connection String

In `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkifyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

> `(localdb)\mssqllocaldb` is the default LocalDB instance shipped with Visual Studio. No separate SQL Server installation needed for development.

---

## 3. Folder Structure

```
Workify.Web/
├── Controllers/
│   ├── AccountController.cs
│   ├── JobsController.cs
│   ├── ProposalsController.cs
│   ├── ContractsController.cs
│   ├── MilestonesController.cs
│   ├── MessagesController.cs
│   ├── WalletController.cs
│   ├── DisputesController.cs
│   └── AdminController.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Job.cs
│   ├── Proposal.cs
│   ├── Contract.cs
│   ├── Milestone.cs
│   ├── Message.cs
│   ├── Conversation.cs
│   ├── Transaction.cs
│   ├── Wallet.cs
│   ├── Dispute.cs
│   ├── DisputeMessage.cs
│   ├── Review.cs
│   └── Enums/
│       ├── UserRole.cs
│       ├── JobStatus.cs
│       ├── ProposalStatus.cs
│       ├── ContractStatus.cs
│       ├── MilestoneStatus.cs
│       ├── TransactionType.cs
│       └── DisputeStatus.cs
├── Data/
│   └── ApplicationDbContext.cs
├── ViewModels/
│   ├── JobCreateViewModel.cs
│   ├── ProposalSubmitViewModel.cs
│   └── ... (one per form)
└── wwwroot/           ← static frontend files go here
```

---

## 4. Entities & Data Models

All models live in `Models/`. Every entity uses EF Core data annotations or Fluent API for relationships.

### 4.1 ApplicationUser

Extends `IdentityUser` (which provides `Id`, `Email`, `PasswordHash`, etc.).

| Field | Type | Notes |
|---|---|---|
| `Id` | `string` (GUID) | Inherited — primary key |
| `FullName` | `string` | Display name across the platform |
| `Role` | `UserRole` (enum) | `Freelancer` or `Client` |
| `Bio` | `string?` | Profile bio |
| `AvatarUrl` | `string?` | Path to profile picture |
| `Skills` | `string?` | Comma-separated or JSON array (freelancer) |
| `HourlyRate` | `decimal?` | Freelancer rate |
| `Country` | `string?` | User location |
| `IsVerified` | `bool` | Admin-verified badge |
| `IsBanned` | `bool` | Admin ban flag |
| `CreatedAt` | `DateTime` | Account creation timestamp |
| `Wallet` | `Wallet` (nav) | One-to-one: user owns one wallet |
| `PostedJobs` | `ICollection<Job>` | Jobs posted by this client |
| `Proposals` | `ICollection<Proposal>` | Proposals submitted by this freelancer |
| `ClientContracts` | `ICollection<Contract>` | Contracts where user is the client |
| `FreelancerContracts` | `ICollection<Contract>` | Contracts where user is the freelancer |

```csharp
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

    public Wallet? Wallet { get; set; }
    public ICollection<Job> PostedJobs { get; set; } = new List<Job>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<Contract> ClientContracts { get; set; } = new List<Contract>();
    public ICollection<Contract> FreelancerContracts { get; set; } = new List<Contract>();
}
```

---

### 4.2 Job

Represents a job posting made by a client.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Title` | `string` | Job title |
| `Description` | `string` | Full job description |
| `Category` | `string` | e.g., Web Dev, Design, Writing |
| `BudgetMin` | `decimal` | Minimum budget |
| `BudgetMax` | `decimal` | Maximum budget |
| `BudgetType` | `string` | `"Fixed"` or `"Hourly"` |
| `ExperienceLevel` | `string` | Entry / Intermediate / Expert |
| `Status` | `JobStatus` (enum) | Open, Closed, Cancelled |
| `CreatedAt` | `DateTime` | When posted |
| `ExpiresAt` | `DateTime?` | Optional expiry date |
| `ClientId` | `string` (FK) | → `ApplicationUser.Id` |
| `Client` | `ApplicationUser` (nav) | Navigation property |
| `Proposals` | `ICollection<Proposal>` | All proposals submitted for this job |
| `Contract` | `Contract?` (nav) | Resulting contract if hired |

---

### 4.3 Proposal

A freelancer's bid on a job.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `CoverLetter` | `string` | Freelancer's pitch |
| `BidAmount` | `decimal` | Proposed price |
| `DeliveryDays` | `int` | Estimated delivery in days |
| `Status` | `ProposalStatus` (enum) | Pending, Shortlisted, Hired, Rejected, Withdrawn |
| `SubmittedAt` | `DateTime` | Submission timestamp |
| `JobId` | `int` (FK) | → `Job.Id` |
| `Job` | `Job` (nav) | Navigation property |
| `FreelancerId` | `string` (FK) | → `ApplicationUser.Id` |
| `Freelancer` | `ApplicationUser` (nav) | Navigation property |
| `Contract` | `Contract?` (nav) | Contract created from this proposal |

---

### 4.4 Contract

Created when a client hires a freelancer. Central entity connecting job, proposal, parties, milestones, and disputes.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Terms` | `string` | Agreed contract terms text |
| `TotalAmount` | `decimal` | Total contract value |
| `Status` | `ContractStatus` (enum) | Active, Completed, Cancelled, Disputed |
| `StartDate` | `DateTime` | Contract start date |
| `EndDate` | `DateTime?` | Completed or cancelled date |
| `JobId` | `int` (FK) | → `Job.Id` |
| `Job` | `Job` (nav) | Navigation property |
| `ProposalId` | `int` (FK) | → `Proposal.Id` |
| `Proposal` | `Proposal` (nav) | The winning proposal |
| `ClientId` | `string` (FK) | → `ApplicationUser.Id` |
| `Client` | `ApplicationUser` (nav) | Client party |
| `FreelancerId` | `string` (FK) | → `ApplicationUser.Id` |
| `Freelancer` | `ApplicationUser` (nav) | Freelancer party |
| `Milestones` | `ICollection<Milestone>` | Payment milestones |
| `Messages` | `ICollection<Message>` | Linked conversation thread |
| `Dispute` | `Dispute?` (nav) | Active dispute if any |

> **Important:** Contract has two FK columns pointing to `ApplicationUser`. EF Core cannot resolve this automatically — explicit Fluent API configuration is required in `OnModelCreating` (see Section 7).

---

### 4.5 Milestone

Contracts are broken into milestones. Each milestone has an escrow-funded amount released when the client approves the deliverable.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Title` | `string` | e.g., "Phase 1: Design" |
| `Description` | `string?` | What is expected to be delivered |
| `Amount` | `decimal` | Payment amount for this milestone |
| `DueDate` | `DateTime?` | Expected completion date |
| `Status` | `MilestoneStatus` (enum) | Funded, InProgress, Submitted, InReview, Released, Disputed |
| `SubmittedAt` | `DateTime?` | When freelancer marked complete |
| `ReleasedAt` | `DateTime?` | When client released payment |
| `DeliverableNotes` | `string?` | Freelancer notes on submission |
| `ContractId` | `int` (FK) | → `Contract.Id` |
| `Contract` | `Contract` (nav) | Navigation property |
| `Transactions` | `ICollection<Transaction>` | Fund + release transactions |

---

### 4.6 Wallet

Each user has one wallet. Tracks available and escrow-held funds.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `AvailableBalance` | `decimal` | Funds the user can withdraw or spend |
| `EscrowBalance` | `decimal` | Funds locked in active milestones |
| `Currency` | `string` | e.g., `"USD"` (default) |
| `UserId` | `string` (FK) | → `ApplicationUser.Id` (unique) |
| `User` | `ApplicationUser` (nav) | Navigation property |
| `Transactions` | `ICollection<Transaction>` | Full transaction history |

---

### 4.7 Transaction

An **immutable** record of every money movement in the system. **Never update or delete a transaction row.**

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Amount` | `decimal` | Transaction amount |
| `Type` | `TransactionType` (enum) | Deposit, EscrowLock, EscrowRelease, PlatformFee, Withdrawal, Refund |
| `Description` | `string` | Human-readable description |
| `CreatedAt` | `DateTime` | Timestamp |
| `WalletId` | `int` (FK) | → `Wallet.Id` |
| `Wallet` | `Wallet` (nav) | Navigation property |
| `MilestoneId` | `int?` (FK) | Optional link to related milestone |
| `Milestone` | `Milestone?` (nav) | Navigation property |

---

### 4.8 Conversation

A named thread between a client and freelancer, typically associated with a contract. Holds many `Message` records.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `CreatedAt` | `DateTime` | When the conversation started |
| `ContractId` | `int?` (FK) | Optional link to a contract |
| `Contract` | `Contract?` (nav) | Navigation property |
| `ParticipantAId` | `string` (FK) | First participant → `ApplicationUser.Id` |
| `ParticipantA` | `ApplicationUser` (nav) | Navigation property |
| `ParticipantBId` | `string` (FK) | Second participant → `ApplicationUser.Id` |
| `ParticipantB` | `ApplicationUser` (nav) | Navigation property |
| `Messages` | `ICollection<Message>` | All messages in the thread |

> **Note:** Like `Contract`, `Conversation` has two FK columns pointing to `ApplicationUser` — requires explicit Fluent API configuration.

---

### 4.9 Message

Individual message inside a `Conversation`.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Body` | `string` | Message content |
| `SentAt` | `DateTime` | Timestamp |
| `IsRead` | `bool` | Has the recipient read it? |
| `AttachmentUrl` | `string?` | Optional file attachment path |
| `ConversationId` | `int` (FK) | → `Conversation.Id` |
| `Conversation` | `Conversation` (nav) | Navigation property |
| `SenderId` | `string` (FK) | → `ApplicationUser.Id` |
| `Sender` | `ApplicationUser` (nav) | Navigation property |

---

### 4.10 Dispute

Filed by either party when there is a disagreement about a contract or milestone. An admin reviews and resolves it.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Reason` | `string` | Why the dispute was filed |
| `Status` | `DisputeStatus` (enum) | Open, UnderReview, ResolvedForClient, ResolvedForFreelancer, Closed |
| `FiledAt` | `DateTime` | When filed |
| `ResolvedAt` | `DateTime?` | When resolved by admin |
| `AdminNotes` | `string?` | Admin resolution notes |
| `ContractId` | `int` (FK) | → `Contract.Id` |
| `Contract` | `Contract` (nav) | Navigation property |
| `FiledById` | `string` (FK) | → `ApplicationUser.Id` |
| `FiledBy` | `ApplicationUser` (nav) | Who filed the dispute |
| `MilestoneId` | `int?` (FK) | Optional — the specific milestone in dispute |
| `Milestone` | `Milestone?` (nav) | Navigation property |
| `DisputeMessages` | `ICollection<DisputeMessage>` | Thread between admin and parties |

---

### 4.11 DisputeMessage

Messages within a dispute thread (between admin and the two parties).

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Body` | `string` | Message content |
| `SentAt` | `DateTime` | Timestamp |
| `DisputeId` | `int` (FK) | → `Dispute.Id` |
| `Dispute` | `Dispute` (nav) | Navigation property |
| `SenderId` | `string` (FK) | → `ApplicationUser.Id` |
| `Sender` | `ApplicationUser` (nav) | Navigation property |

---

### 4.12 Review

After a contract completes, both parties can leave a review linked to the contract and directed at one party.

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Rating` | `int` | 1–5 star rating |
| `Comment` | `string?` | Optional written review |
| `CreatedAt` | `DateTime` | Timestamp |
| `ContractId` | `int` (FK) | → `Contract.Id` |
| `Contract` | `Contract` (nav) | Navigation property |
| `ReviewerId` | `string` (FK) | Who wrote the review → `ApplicationUser.Id` |
| `Reviewer` | `ApplicationUser` (nav) | Navigation property |
| `RevieweeId` | `string` (FK) | Who received the review → `ApplicationUser.Id` |
| `Reviewee` | `ApplicationUser` (nav) | Navigation property |

> **Note:** `Review` also has two FK columns pointing to `ApplicationUser` — requires explicit Fluent API configuration.

---

## 5. Enums

All enums live in `Models/Enums/`. EF Core maps each enum to an `int` column automatically.

```csharp
// UserRole.cs
public enum UserRole { Freelancer, Client }

// JobStatus.cs
public enum JobStatus { Open, Closed, Cancelled }

// ProposalStatus.cs
public enum ProposalStatus { Pending, Shortlisted, Hired, Rejected, Withdrawn }

// ContractStatus.cs
public enum ContractStatus { Active, Completed, Cancelled, Disputed }

// MilestoneStatus.cs
public enum MilestoneStatus { Funded, InProgress, Submitted, InReview, Released, Disputed }

// TransactionType.cs
public enum TransactionType { Deposit, EscrowLock, EscrowRelease, PlatformFee, Withdrawal, Refund }

// DisputeStatus.cs
public enum DisputeStatus { Open, UnderReview, ResolvedForClient, ResolvedForFreelancer, Closed }
```

---

## 6. Entity Relationships

| Relationship | Type |
|---|---|
| `ApplicationUser` → `Wallet` | One-to-One |
| `ApplicationUser` → `Job` (as Client) | One-to-Many |
| `ApplicationUser` → `Proposal` (as Freelancer) | One-to-Many |
| `ApplicationUser` → `Contract` (as Client & Freelancer) | One-to-Many ×2 (two FK columns) |
| `Job` → `Proposal` | One-to-Many |
| `Job` → `Contract` | One-to-One |
| `Proposal` → `Contract` | One-to-One |
| `Contract` → `Milestone` | One-to-Many |
| `Contract` → `Dispute` | One-to-One |
| `Contract` → `Review` | One-to-Many (up to 2 — one per party) |
| `Wallet` → `Transaction` | One-to-Many |
| `Milestone` → `Transaction` | One-to-Many |
| `Conversation` → `Message` | One-to-Many |
| `ApplicationUser` → `Conversation` | Many-to-Many via two FK columns (ParticipantA, ParticipantB) |
| `Dispute` → `DisputeMessage` | One-to-Many |

---

## 7. ApplicationDbContext

File: `Data/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<DisputeMessage> DisputeMessages { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Contract → two FKs to ApplicationUser
        builder.Entity<Contract>().HasOne(c => c.Client)
            .WithMany(u => u.ClientContracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contract>().HasOne(c => c.Freelancer)
            .WithMany(u => u.FreelancerContracts)
            .HasForeignKey(c => c.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Conversation → two FKs to ApplicationUser
        builder.Entity<Conversation>().HasOne(c => c.ParticipantA)
            .WithMany()
            .HasForeignKey(c => c.ParticipantAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Conversation>().HasOne(c => c.ParticipantB)
            .WithMany()
            .HasForeignKey(c => c.ParticipantBId)
            .OnDelete(DeleteBehavior.Restrict);

        // Review → two FKs to ApplicationUser
        builder.Entity<Review>().HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>().HasOne(r => r.Reviewee)
            .WithMany()
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision for all money columns
        builder.Entity<Wallet>().Property(w => w.AvailableBalance).HasPrecision(18, 2);
        builder.Entity<Wallet>().Property(w => w.EscrowBalance).HasPrecision(18, 2);
        builder.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
        builder.Entity<Milestone>().Property(m => m.Amount).HasPrecision(18, 2);
        builder.Entity<Job>().Property(j => j.BudgetMin).HasPrecision(18, 2);
        builder.Entity<Job>().Property(j => j.BudgetMax).HasPrecision(18, 2);
        builder.Entity<Proposal>().Property(p => p.BidAmount).HasPrecision(18, 2);
        builder.Entity<Contract>().Property(c => c.TotalAmount).HasPrecision(18, 2);
    }
}
```

> **Critical:** The `OnModelCreating` overrides are mandatory. Without them, EF Core throws an error about ambiguous FK relationships on `Contract`, `Conversation`, and `Review`.

---

## 8. Running Migrations

Open **Tools → NuGet Package Manager → Package Manager Console** and run:

```
Add-Migration InitialCreate
Update-Database
```

This creates `WorkifyDb` in LocalDB with all tables. Verify in **SQL Server Object Explorer**.

**Rule:** Every time you add, remove, or change a model property — run `Add-Migration <DescriptiveName>` then `Update-Database`.

---

## 9. Controllers

All controllers inject `ApplicationDbContext` via constructor. Use EF Core LINQ queries directly — no service layer.

### 9.1 AccountController

| Action | Method | Description |
|---|---|---|
| `Register` | POST | Collect role (Client/Freelancer), create `ApplicationUser` via `UserManager`, create a `Wallet` for the new user |
| `Login` | POST | Delegate to `SignInManager` |
| `Logout` | POST | Delegate to `SignInManager` |
| `Profile` | GET | Load user + reviews + wallet summary |
| `EditProfile` | POST | Update bio, skills, hourly rate, avatar |

### 9.2 JobsController

| Action | Method | Description |
|---|---|---|
| `Index` | GET | Paginated list of open jobs with filters (category, budget, experience) using `IQueryable<Job>` + `.Where()` chains |
| `Details` | GET | Load job + client info + proposal count |
| `Create` | GET/POST | Client only. Validate and save new job, set `Status = Open` |
| `Close` | POST | Client only. Set `Status = Closed` |
| `Delete` | POST | Client only, only if no accepted proposals |

### 9.3 ProposalsController

| Action | Method | Description |
|---|---|---|
| `Submit` | POST | Freelancer only, one proposal per job. Validate bid > 0 and job is Open |
| `MyProposals` | GET | Freelancer sees their submitted proposals and statuses |
| `ReviewProposals` | GET | Client sees all proposals for their job |
| `Shortlist` | POST | Client marks proposal as Shortlisted |
| `Hire` | POST | Client hires: set `Proposal.Status = Hired`, close all other proposals, set `Job.Status = Closed`, create a `Contract` |
| `Withdraw` | POST | Freelancer withdraws a pending proposal |

### 9.4 ContractsController

| Action | Method | Description |
|---|---|---|
| `Details` | GET | Load contract with milestones, both parties, dispute status |
| `ClientContracts` | GET | List of all client's contracts |
| `FreelancerContracts` | GET | List of all freelancer's contracts |
| `AddMilestone` | POST | Client adds a milestone to an active contract |
| `FundMilestone` | POST | Client funds milestone: debit `AvailableBalance`, credit `EscrowBalance`, create `EscrowLock` Transaction, set `Milestone.Status = InProgress` |
| `Complete` | POST | Mark whole contract as Completed after all milestones released |

### 9.5 MilestonesController

| Action | Method | Description |
|---|---|---|
| `Submit` | POST | Freelancer marks milestone as `InReview` |
| `Approve` | POST | Client approves: move amount from `EscrowBalance` to freelancer's `AvailableBalance`, create `EscrowRelease` + `PlatformFee` transactions, set `Milestone.Status = Released` |
| `Reject` | POST | Client rejects submission, sets back to `InProgress` with a note |

### 9.6 MessagesController

| Action | Method | Description |
|---|---|---|
| `Inbox` | GET | List all Conversations for logged-in user |
| `Thread` | GET | Load all messages in a conversation, mark unread as read |
| `Send` | POST | Create new Message in a conversation |
| `StartConversation` | POST | Create a Conversation if one doesn't already exist between two users |

### 9.7 WalletController

| Action | Method | Description |
|---|---|---|
| `Overview` | GET | Show available balance, escrow balance, and transaction history |
| `Deposit` | POST | Increase `AvailableBalance`, create `Deposit` Transaction |
| `Withdraw` | POST | Decrease `AvailableBalance` if sufficient funds, create `Withdrawal` Transaction |

> In production, `Deposit` hooks into a payment gateway (e.g., Stripe). For now, update the balance directly.

### 9.8 DisputesController

| Action | Method | Description |
|---|---|---|
| `File` | POST | Either party can file. Create `Dispute` with `Status = Open`, set `Contract.Status = Disputed`, optionally set `Milestone.Status = Disputed` |
| `Details` | GET | Load dispute with messages and contract context |
| `SendMessage` | POST | Add a `DisputeMessage` to the thread |
| `Resolve` | POST | **Admin only.** Set resolution, update contract/milestone/wallet, set `Dispute.Status` to resolved |

### 9.9 AdminController

Decorate with `[Authorize(Roles = "Admin")]`. Requires `AddRoles<IdentityRole>()` in `Program.cs`.

| Action | Method | Description |
|---|---|---|
| `Dashboard` | GET | Counts: open jobs, active contracts, open disputes, total users |
| `Users` | GET | Paginated user list with search |
| `BanUser / UnbanUser` | POST | Toggle `IsBanned` |
| `VerifyUser` | POST | Toggle `IsVerified` |
| `Jobs` | GET | All job postings with moderation controls |
| `RemoveJob` | POST | Admin removes a job post |
| `Disputes` | GET | All open disputes queue for admin review |

---

## 10. Program.cs Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // set true when adding email confirmation
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>()   // required for Admin role
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // required for Identity UI

app.Run();
```

---

## 11. Authorization

Cookie-based sessions — no JWT needed for MVC.

| Scenario | How to Implement |
|---|---|
| Only logged-in users | `[Authorize]` on controller or action |
| Only clients | Check `User.FindFirst("Role")?.Value == "Client"` inside the action |
| Only freelancers | Check `User.FindFirst("Role")?.Value == "Freelancer"` inside the action |
| Only the owner of a record | Load the record, compare `record.ClientId == User's Id`, return `Forbid()` if mismatch |
| Admin only | `[Authorize(Roles = "Admin")]` — requires `AddRoles<IdentityRole>()` in `Program.cs` |

> Store the user's `Role` as a **Claim** during login (via `UserClaimsPrincipalFactory` or manually in `AccountController`) for cheap role checks without a database round-trip on every request.

---

## 12. ViewModels

**Never pass EF Core model objects directly to views.** Use one ViewModel per form or page. All ViewModels live in `ViewModels/`.

```csharp
// JobCreateViewModel.cs — example
public class JobCreateViewModel
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    [Required] public decimal BudgetMin { get; set; }
    [Required] public decimal BudgetMax { get; set; }
    [Required] public string BudgetType { get; set; } = "Fixed";
    [Required] public string ExperienceLevel { get; set; } = "Entry";
    public DateTime? ExpiresAt { get; set; }
}
```

Create a similar ViewModel for every form: `ProposalSubmitViewModel`, `MilestoneCreateViewModel`, `DisputeFileViewModel`, `ReviewCreateViewModel`, etc.

The static HTML frontend POSTs to MVC controller actions via regular HTML forms or `fetch()`. The controller reads the ViewModel, validates it, and returns JSON or redirects.

---

## 13. Connecting the Static Frontend

### Option A — Serve from wwwroot (Recommended)

- Copy `Workify_Front/` contents into `wwwroot/`
- ASP.NET Core's static file middleware serves them automatically
- JavaScript `fetch()` calls hit controller endpoints (e.g., `fetch('/api/jobs')`)
- Add `[Route("api/[controller]")]` to controllers called from JS

### Option B — Keep Frontend Separate (CORS required)

- Frontend runs on a different port (e.g., a local dev server)
- Backend runs on `localhost:5000`
- Add `builder.Services.AddCors(...)` in `Program.cs` and configure allowed origins

> **Recommendation:** Option A is simpler and avoids CORS complexity — use it for the initial build.

---

## 14. Core Business Flow: Escrow & Payments

This is the most important flow to implement correctly. Follow this exact sequence:

| Step | Action | What Happens |
|---|---|---|
| 1 | Client deposits funds | `POST /Wallet/Deposit` → `AvailableBalance` increases, `Transaction (Deposit)` created |
| 2 | Milestone created | `POST /Contracts/AddMilestone` → Milestone saved with `Status = Funded` (pending client funding) |
| 3 | Client funds milestone | `POST /Milestones/Fund` → `AvailableBalance -= amount`, `EscrowBalance += amount`, `Transaction (EscrowLock)` created, `Milestone.Status = InProgress` |
| 4 | Freelancer submits work | `POST /Milestones/Submit` → `Milestone.Status = InReview`, `SubmittedAt = now` |
| 5a | Client approves | `POST /Milestones/Approve` → `EscrowBalance -= amount`, `freelancer.Wallet.AvailableBalance += (amount - fee)`, `Transaction (EscrowRelease)` + `Transaction (PlatformFee)` created, `Milestone.Status = Released` |
| 5b | Client rejects | `POST /Milestones/Reject` → `Milestone.Status = InProgress`, freelancer notified |
| 6 | Dispute filed | `POST /Disputes/File` → `Milestone.Status = Disputed`, `Contract.Status = Disputed`, admin reviews and resolves |
| 7 | Freelancer withdraws | `POST /Wallet/Withdraw` → `AvailableBalance -= amount`, `Transaction (Withdrawal)` created |

> **Rule:** Always update Wallet balances and create Transactions **in the same database operation** using a single `SaveChangesAsync()` call. This keeps the transaction log consistent with balances.

---

## 15. Recommended Build Order

Build and test in this order to avoid dependency issues:

1. Set up project, install packages, configure connection string
2. Define all Enums
3. Create `ApplicationUser` (extend `IdentityUser`)
4. Create remaining models (Job, Proposal, Contract, Milestone, Wallet, Transaction, Conversation, Message, Dispute, DisputeMessage, Review)
5. Configure `ApplicationDbContext` with all DbSets and `OnModelCreating` overrides
6. Run `Add-Migration InitialCreate` and `Update-Database` — verify tables in SQL Server Object Explorer
7. Build `AccountController` — register and login working end-to-end
8. Build `JobsController` — browse and post jobs working
9. Build `ProposalsController` — submit and hire flow
10. Build `ContractsController` + `MilestonesController` — full escrow lifecycle
11. Build `WalletController` — deposit, balance display, withdraw
12. Build `MessagesController` — conversations and messaging
13. Build `DisputesController` — filing and resolution
14. Build `AdminController` — user management and dispute queue
15. Connect static frontend via `wwwroot` and test end-to-end

---

## Final Notes

This backend is **intentionally straightforward**:
- No service layer between controllers and `DbContext`
- No JWT tokens
- No AutoMapper
- No extra abstraction libraries

EF Core and ASP.NET Core Identity handle the heavy lifting.

**Natural next additions when ready to expand:**
- Email confirmation via SMTP / SendGrid
- Real payment gateway integration (Stripe) for deposits
- SignalR for real-time messaging

None of these are required to ship a fully functional Workify backend.
