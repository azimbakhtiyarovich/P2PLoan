using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;

public interface IJwtService
{
    /// <summary>Access token generatsiya qiladi.</summary>
    string GenerateToken(User user, IEnumerable<string> roles);

    /// <summary>Token ichidagi UserId ni qaytaradi. Yaroqsiz bo'lsa null.</summary>
    Guid? ValidateToken(string token);
}
