using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;

namespace P2PLoan.DataAccess;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // === Identity ===
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    // === Profiles ===
    public DbSet<BorrowerProfile> BorrowerProfiles { get; set; }
    public DbSet<LenderProfile> LenderProfiles { get; set; }

    // === KYC ===
    public DbSet<KycDocument> KycDocuments { get; set; }

    // === Loans ===
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanOffer> LoanOffers { get; set; }
    public DbSet<Repayment> Repayments { get; set; }

    // === Investments ===
    public DbSet<Investment> Investments { get; set; }

    // === Payments & Wallet ===
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    // === Notifications & Audit ===
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Role PK — ValueGeneratedNever so Id=0 is valid for seeding ───────
        modelBuilder.Entity<Role>()
            .Property(r => r.Id)
            .ValueGeneratedNever();

        // ── UserRole composite PK ──────────────────────────────────────────
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── BorrowerProfile ────────────────────────────────────────────────
        modelBuilder.Entity<BorrowerProfile>()
            .HasOne(bp => bp.User)
            .WithOne()
            .HasForeignKey<BorrowerProfile>(bp => bp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BorrowerProfile>()
            .HasIndex(bp => bp.UserId)
            .IsUnique();

        // ── LenderProfile ──────────────────────────────────────────────────
        modelBuilder.Entity<LenderProfile>()
            .HasOne(lp => lp.User)
            .WithOne()
            .HasForeignKey<LenderProfile>(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LenderProfile>()
            .HasIndex(lp => lp.UserId)
            .IsUnique();

        // ── UserProfile ────────────────────────────────────────────────────
        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── KycDocument ────────────────────────────────────────────────────
        modelBuilder.Entity<KycDocument>()
            .HasOne(k => k.User)
            .WithMany(u => u.KycDocuments)
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Loan ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Borrower)
            .WithMany(bp => bp.Loans)
            .HasForeignKey(l => l.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.Status);

        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.BorrowerId);

        // ── LoanOffer ──────────────────────────────────────────────────────
        modelBuilder.Entity<LoanOffer>()
            .HasOne(o => o.Loan)
            .WithMany(l => l.Offers)
            .HasForeignKey(o => o.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Repayment ──────────────────────────────────────────────────────
        modelBuilder.Entity<Repayment>()
            .HasOne(r => r.Loan)
            .WithMany(l => l.Repayments)
            .HasForeignKey(r => r.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Repayment>()
            .HasIndex(r => new { r.LoanId, r.DueDate });

        // ── Investment ─────────────────────────────────────────────────────
        modelBuilder.Entity<Investment>()
            .HasOne(i => i.Loan)
            .WithMany(l => l.Investments)
            .HasForeignKey(i => i.LoanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Investment>()
            .HasOne(i => i.Lender)
            .WithMany(lp => lp.Investments)
            .HasForeignKey(i => i.LenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Investment>()
            .HasIndex(i => i.LenderId);

        // ── Wallet ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Wallet>()
            .HasOne(w => w.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Wallet>()
            .HasIndex(w => w.UserId)
            .IsUnique();

        // Optimistic concurrency
        modelBuilder.Entity<Wallet>()
            .Property(w => w.RowVersion)
            .IsRowVersion();

        // ── Transaction ────────────────────────────────────────────────────
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.WalletId, t.CreatedAt });

        // ── Payment ────────────────────────────────────────────────────────
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.ExternalId)
            .IsUnique()
            .HasFilter("[ExternalId] IS NOT NULL"); // NULL dagi duplicate ruxsat

        // ── Notification ───────────────────────────────────────────────────
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.Read });

        // ── AuditLog ───────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityType, a.EntityId });

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.CreatedAt);

        // ── User soft delete global query filter ───────────────────────────
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);

        // ── Role seeding ───────────────────────────────────────────────────
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 0, Name = "Borrower" },
            new Role { Id = 1, Name = "Lender"   },
            new Role { Id = 2, Name = "Admin"     }
        );
    }
}
