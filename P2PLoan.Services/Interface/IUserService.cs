
using P2PLoan.Core.DTO.Auth;
using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;
public interface IUserService
{
    Task<User> RegisterAsync(User user);
    Task<User> LoginAsync(User user);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByPhoneAsync(string phone);
    Task<bool> VerifyPhoneAsync(Guid userId);
    Task<bool> UpdatePasswordAsync(Guid userId, string newHash);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignRoleAsync(Guid userId, short roleId);
    Task<bool> RemoveRoleAsync(Guid userId, short roleId);
    Task<bool> DeleteUserAsync(Guid userId);
}
