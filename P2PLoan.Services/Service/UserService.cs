
using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.Entities;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;
public class UserService : IUserService
{
    private readonly DataAccess.ApplicationDbContext _context;
    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> AssignRoleAsync(Guid userId, short roleId)
    {
        var exists = await _context.UserRoles
             .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (exists) return false;

        _context.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByPhoneAsync(string phone)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
               .Where(ur => ur.UserId == userId)
               .Select(ur => ur.Role!)
               .ToListAsync();
    }

    public async Task<User> LoginAsync(User user)
    {
        var userr = await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == user.Phone && u.PasswordHash == user.PasswordHash);
        if (userr == null)
        {
            throw new Exception("User not found or invalid credentials");
        }
        return userr;
    }

    public async Task<User> RegisterAsync(User dto)
    {
        if (await _context.Users.AnyAsync(u => u.Id == dto.Id))
            throw new Exception("User already exists");

        var user = new User
        {
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
            IsPhoneVerified = false,
            KycDocuments = new List<KycDocument>()
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, short roleId)
    {
        var userRole = await _context.UserRoles
               .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string newHash)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return false;

        user.PasswordHash = newHash;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyPhoneAsync(Guid userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return false;

        user.IsPhoneVerified = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
