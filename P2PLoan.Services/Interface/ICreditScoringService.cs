using P2PLoan.Core.DTO.CreditScore;

namespace P2PLoan.Services.Interface;

public interface ICreditScoringService
{
    /// <summary>
    /// Borrower uchun kredit ballini hisoblaydi va BorrowerProfile ga saqlaydi.
    /// </summary>
    Task<CreditScoreResultDto> CalculateAndSaveAsync(Guid borrowerProfileId);

    /// <summary>
    /// Oxirgi hisoblangan kredit ballini qaytaradi (yangilashmaydi).
    /// </summary>
    Task<CreditScoreResultDto?> GetLatestScoreAsync(Guid borrowerProfileId);

    /// <summary>
    /// Borrower berilgan miqdordagi qarzga layoqatli ekanini tekshiradi.
    /// Layoqatsiz bo'lsa CreditCheckFailedException otadi.
    /// </summary>
    Task EnsureEligibleAsync(Guid borrowerProfileId, decimal loanAmount);
}
