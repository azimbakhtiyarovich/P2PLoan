using P2PLoan.Core.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2PLoan.Core.Entities;

public class UserProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    // === Asosiy ma'lumotlar ===
    [MaxLength(200)] public string? FullName { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    public string? Address { get; set; }
    [MaxLength(100)] public string? Country { get; set; }

    // === KYC ===
    [MaxLength(50)] public string? PassportNumber { get; set; }
    public DateTime? PassportIssuedDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.NotSubmitted;

    // === Moliyaviy ma'lumotlar ===
    [Column(TypeName = "decimal(18,2)")] public decimal MonthlyIncome { get; set; } = 0m;
    [Column(TypeName = "decimal(18,2)")] public decimal? ExistingDebt { get; set; }

    // === Credit Scoring ===
    public int? CreditScore { get; set; }
    public CreditRating CreditRating { get; set; } = CreditRating.CCC;
    public DateTimeOffset? LastScoredAt { get; set; }

    // === Investor sozlamalari ===
    [Column(TypeName = "decimal(18,2)")] public decimal? PreferredMinAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? PreferredMaxAmount { get; set; }

    // === Navigatsiya ===
    public User? User { get; set; }
}
