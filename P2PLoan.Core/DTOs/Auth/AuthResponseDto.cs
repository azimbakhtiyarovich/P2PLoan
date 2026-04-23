
namespace P2PLoan.Core.DTO.Auth;
public record AuthResponseDto(Guid UserId, string AccessToken, 
    DateTime ExpiresAt, IEnumerable<string> Roles, string ActiveRole);
