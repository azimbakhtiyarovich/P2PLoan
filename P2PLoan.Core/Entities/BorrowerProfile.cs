using P2PLoan.Core.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2PLoan.Core.Entities;

public class BorrowerProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    // === KYC ===
    [MaxLength(50)] public string? PassportNumber { get; set; }
    public DateTime? PassportIssuedDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.NotSubmitted;

    // === Moliyaviy ma'lumotlar ===
    /// <summary>Oylik daromad (UZS). string emas, decimal - tuzilgan ma'lumot.</summary>
    [Column(TypeName = "decimal(18,2)")] public decimal MonthlyIncome { get; set; } = 0m;

    /// <summary>Mavjud qarzlar jami summasi (UZS).</summary>
    [Column(TypeName = "decimal(18,2)")] public decimal? ExistingDebt { get; set; }

    // === Credit Scoring ===
    /// <summary>Kredit ball (300-850). Null = hali hisoblanmagan.</summary>
    public int? CreditScore { get; set; }

    /// <summary>Kredit reytingi (CCC...AAA).</summary>
    public CreditRating CreditRating { get; set; } = CreditRating.CCC;

    /// <summary>Kredit ball oxirgi yangilangan sana.</summary>
    public DateTimeOffset? LastScoredAt { get; set; }

    // === Navigatsiya ===
    public User? User { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
