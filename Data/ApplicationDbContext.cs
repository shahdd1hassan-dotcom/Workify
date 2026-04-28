using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Workify_Full.Models;

namespace Workify_Full.Data
{
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
            // Contract has TWO FK columns pointing to ApplicationUser 
            // EF Core can't guess which is which, so we tell it explicitly: 
            builder.Entity<Contract>().HasOne(c => c.Client)
            .WithMany(u => u.ClientContracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Contract>().HasOne(c => c.Freelancer)
            .WithMany(u => u.FreelancerContracts)
            .HasForeignKey(c => c.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);
            // Job -> Client (one-to-many). Prevent cascade delete to avoid multiple cascade paths.
            builder.Entity<Job>().HasOne(j => j.Client)
                .WithMany(u => u.PostedJobs)
                .HasForeignKey(j => j.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Proposal -> Freelancer (one-to-many). Prevent cascade delete to avoid multiple cascade paths.
            builder.Entity<Proposal>().HasOne(p => p.Freelancer)
                .WithMany(u => u.Proposals)
                .HasForeignKey(p => p.FreelancerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wallet -> User (one-to-one). Prevent cascade delete.
            builder.Entity<Wallet>().HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message -> Sender (user). Prevent cascade delete.
            builder.Entity<Message>().HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dispute -> FiledBy (user). Prevent cascade delete.
            builder.Entity<Dispute>().HasOne(d => d.FiledBy)
                .WithMany()
                .HasForeignKey(d => d.FiledById)
                .OnDelete(DeleteBehavior.Restrict);

            // DisputeMessage -> Sender (user). Prevent cascade delete.
            builder.Entity<DisputeMessage>().HasOne(dm => dm.Sender)
                .WithMany()
                .HasForeignKey(dm => dm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction -> Wallet. Prevent cascade delete from wallet to transactions.
            builder.Entity<Transaction>().HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
            // Conversation also has two FK columns pointing to ApplicationUser 
            builder.Entity<Conversation>().HasOne(c => c.ParticipantA)
            .WithMany()
            .HasForeignKey(c => c.ParticipantAId)
            .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Conversation>().HasOne(c => c.ParticipantB)
            .WithMany()
            .HasForeignKey(c => c.ParticipantBId)
            .OnDelete(DeleteBehavior.Restrict);
            // Review has two FK columns pointing to ApplicationUser 
            builder.Entity<Review>().HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Review>().HasOne(r => r.Reviewee)
            .WithMany()
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
            // Decimal precision for money columns 
            builder.Entity<Wallet>()
            .Property(w => w.AvailableBalance).HasPrecision(18, 2);
            builder.Entity<Wallet>()
            .Property(w => w.EscrowBalance).HasPrecision(18, 2);
            builder.Entity<Transaction>()
            .Property(t => t.Amount).HasPrecision(18, 2);
            builder.Entity<Milestone>()
            .Property(m => m.Amount).HasPrecision(18, 2);
            builder.Entity<Job>()
            .Property(j => j.BudgetMin).HasPrecision(18, 2);
            builder.Entity<Job>()
            .Property(j => j.BudgetMax).HasPrecision(18, 2);
            builder.Entity<Proposal>()
            .Property(p => p.BidAmount).HasPrecision(18, 2);
            builder.Entity<Contract>()
            .Property(c => c.TotalAmount).HasPrecision(18, 2);
        }
    }
}
