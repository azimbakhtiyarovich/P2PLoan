using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Auth;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        // Input validation first
        if (await _context.Users.AnyAsync(u => u.Phone == dto.PhoneNumber))
            throw new ConflictException($"Telefon raqami allaqachon ro'yxatdan o'tgan: {dto.PhoneNumber}");

        if (await _context.UserProfiles.AnyAsync(up => up.Email == dto.Email))
            throw new ConflictException($"Email allaqachon ro'yxatdan o'tgan: {dto.Email}");

        ValidatePasswordStrength(dto.Password);

        var user = new User
        {
            Phone           = dto.PhoneNumber,
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsPhoneVerified = false
        };

        var profile = new UserProfile
        {
            UserId = user.Id,
            Email  = dto.Email
        };

        var wallet = new Wallet { UserId = user.Id };

        _context.Users.Add(user);
        _context.UserProfiles.Add(profile);
        _context.Wallets.Add(wallet);

        // Har bir yangi foydalanuvchiga "User" roli beriladi
        var userRoleId = (short)RoleType.User;
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == userRoleId);
        if (roleExists)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId     = user.Id,
                RoleId     = userRoleId,
                AssignedAt = DateTimeOffset.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> LoginAsync(string phone, string rawPassword)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == phone);

        if (user is null || !BCrypt.Net.BCrypt.Verify(rawPassword, user.PasswordHash))
            throw new UnauthorizedException("Telefon raqami yoki parol noto'g'ri.");

        user.LastLogin = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByPhoneAsync(string phone)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<bool> VerifyPhoneAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return false;

        user.IsPhoneVerified = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new UnauthorizedException("Joriy parol noto'g'ri.");

        ValidatePasswordStrength(newPassword);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role!)
            .ToListAsync();
    }

    public async Task<bool> AssignRoleAsync(Guid userId, short roleId)
    {
        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (exists) return false;

        _context.UserRoles.Add(new UserRole
        {
            UserId     = userId,
            RoleId     = roleId,
            AssignedAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, short roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole is null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null || user.IsDeleted) return false;

        user.IsDeleted  = true;
        user.DeletedAt  = DateTimeOffset.UtcNow;
        user.Phone      = $"DELETED_{userId}_{user.Phone}";
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void ValidatePasswordStrength(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("Kamida 8 ta belgi bo'lishi kerak.");
        if (!password.Any(char.IsUpper))
            errors.Add("Kamida bitta katta harf bo'lishi kerak.");
        if (!password.Any(char.IsDigit))
            errors.Add("Kamida bitta raqam bo'lishi kerak.");

        if (errors.Any())
            throw new ValidationException("password", string.Join(" ", errors));
    }
}
