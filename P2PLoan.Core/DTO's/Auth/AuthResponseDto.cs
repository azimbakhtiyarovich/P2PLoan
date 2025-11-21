
namespace P2PLoan.Core.DTO_s.Auth;
public record AuthResponseDto(Guid UserId, string AccessToken, 
    DateTime ExpiresAt, IEnumerable<string> Roles, string ActiveRole);
