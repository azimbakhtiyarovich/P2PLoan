using P2PLoan.Core.Enum;

namespace P2PLoan.Core.DTO.Profile;

public class BorrowerProfileDto
{
    public Guid UserId { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? BirthDate { get; set; }

    // Moliyaviy
    public decimal MonthlyIncome { get; set; }
    public decimal? ExistingDebt { get; set; }

    // KYC
    public KycStatus KycStatus { get; set; }

    // Credit Score
    public int? CreditScore { get; set; }
    public CreditRating CreditRating { get; set; }
    public DateTimeOffset? LastScoredAt { get; set; }
}
