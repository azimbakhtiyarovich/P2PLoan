using P2PLoan.Core.Enum;

namespace P2PLoan.Core.DTO.Profile;

public class UserProfileDto
{
    public Guid UserId { get; set; }

    // Asosiy
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }

    // KYC
    public string? PassportNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public KycStatus KycStatus { get; set; }

    // Moliyaviy
    public decimal MonthlyIncome { get; set; }
    public decimal? ExistingDebt { get; set; }

    // Credit Score
    public int? CreditScore { get; set; }
    public CreditRating CreditRating { get; set; }
    public DateTimeOffset? LastScoredAt { get; set; }

    // Investor sozlamalari
    public decimal? PreferredMinAmount { get; set; }
    public decimal? PreferredMaxAmount { get; set; }
}
