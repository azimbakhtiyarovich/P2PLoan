using P2PLoan.Core.DTO.Profile;

namespace P2PLoan.Services.Interface;

public interface IProfileService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}
