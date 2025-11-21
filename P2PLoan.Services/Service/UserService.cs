using P2PLoan.Core.DTO_s.Auth;
using P2PLoan.Core.Entities;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;
public class UserService : IUserService
{
    public Task<User> LoginAsync(LoginDto loginDto)
    {
        throw new NotImplementedException();
    }

    public Task<User> RegisterAsync(RegisterDto registerDto)
    {
        throw new NotImplementedException();
    }
}
