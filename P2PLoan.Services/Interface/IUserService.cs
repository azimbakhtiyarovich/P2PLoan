using P2PLoan.Core.DTO.Auth;
using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<User> LoginAsync(string phone, string rawPassword);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByPhoneAsync(string phone);
    Task<bool> VerifyPhoneAsync(Guid userId);

    /// <summary>Yangi xom parolni hash qilib saqlaydi.</summary>
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignRoleAsync(Guid userId, short roleId);
    Task<bool> RemoveRoleAsync(Guid userId, short roleId);
    Task<bool> DeleteUserAsync(Guid userId);
}
