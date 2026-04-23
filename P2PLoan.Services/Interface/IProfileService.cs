using P2PLoan.Core.DTO.Profile;

namespace P2PLoan.Services.Interface;

public interface IProfileService
{
    Task<BorrowerProfileDto?> GetBorrowerProfileAsync(Guid userId);
    Task UpsertBorrowerProfileAsync(Guid userId, UpdateBorrowerProfileDto dto);
    Task<Guid?> GetBorrowerProfileIdAsync(Guid userId);

    Task<LenderProfileDto?> GetLenderProfileAsync(Guid userId);
    Task UpsertLenderProfileAsync(Guid userId, LenderProfileDto dto);
}
