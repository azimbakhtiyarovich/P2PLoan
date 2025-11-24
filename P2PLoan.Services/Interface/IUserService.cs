
using P2PLoan.Core.DTO.Auth;
using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;
public interface IUserService
{
    Task<User> RegisterAsync(RegisterDto registerDto);
    Task<User> LoginAsync(LoginDto loginDto);
}
