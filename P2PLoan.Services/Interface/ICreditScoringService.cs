using P2PLoan.Core.DTO.CreditScore;

namespace P2PLoan.Services.Interface;

public interface ICreditScoringService
{
    /// <summary>
    /// User uchun kredit ballini hisoblaydi va UserProfile ga saqlaydi.
    /// </summary>
    Task<CreditScoreResultDto> CalculateAndSaveAsync(Guid userId);

    /// <summary>
    /// Oxirgi hisoblangan kredit ballini qaytaradi (yangilashmaydi).
    /// </summary>
    Task<CreditScoreResultDto?> GetLatestScoreAsync(Guid userId);

    /// <summary>
    /// User berilgan miqdordagi qarzga layoqatli ekanini tekshiradi.
    /// Layoqatsiz bo'lsa CreditCheckFailedException otadi.
    /// </summary>
    Task EnsureEligibleAsync(Guid userId, decimal loanAmount);
}
