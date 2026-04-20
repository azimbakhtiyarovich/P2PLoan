using P2PLoan.Core.Enum;

namespace P2PLoan.Core.DTO.CreditScore;

public class CreditScoreResultDto
{
    public Guid BorrowerId { get; set; }

    /// <summary>Kredit ball: 300-850</summary>
    public int Score { get; set; }

    /// <summary>Reyting: CCC, B, BB, BBB, A, AA, AAA</summary>
    public CreditRating Rating { get; set; }

    /// <summary>Kredit berishga layoqatli yoki yo'q</summary>
    public bool IsEligible { get; set; }

    /// <summary>Minimal talab qilingan ball</summary>
    public int MinRequiredScore { get; set; }

    // Faktorlar bo'yicha breakdown
    public decimal KycFactor { get; set; }
    public decimal IncomeFactor { get; set; }
    public decimal DebtToIncomeFactor { get; set; }
    public decimal RepaymentHistoryFactor { get; set; }

    public DateTimeOffset CalculatedAt { get; set; }
}
