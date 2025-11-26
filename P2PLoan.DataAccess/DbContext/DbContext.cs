using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.Entities;

namespace P2PLoan.DataAccess;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<BorrowerProfile> BorrowerProfiles { get; set; }
    public DbSet<LenderProfile> LenderProfiles { get; set; }
    public DbSet<LoanOffer> LoanOffers { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Repayment> Repayments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<Investment>()
            .HasOne(i => i.Loan)
            .WithMany(l => l.Investments)
            .HasForeignKey(i => i.LoanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Investment>()
            .HasOne(i => i.Lender)
            .WithMany()
            .HasForeignKey(i => i.LenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
