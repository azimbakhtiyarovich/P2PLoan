using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using P2PLoan.Core.DTO.Auth;
using P2PLoan.Services.Interface;
using System.Security.Claims;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService  _jwtService;

    // Cookie nomi bir joyda — xato kiritish ehtimolini kamaytiradi
    private const string CookieName    = "access_token";
    private const int    CookieMinutes = 60;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService  = jwtService;
    }

    /// <summary>Yangi foydalanuvchi ro'yxatdan o'tkazish.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user      = await _userService.RegisterAsync(dto);
        var roles     = await _userService.GetUserRolesAsync(user.Id);
        var roleNames = roles.Select(r => r.Name).ToList();
        var expiresAt = DateTime.UtcNow.AddMinutes(CookieMinutes);
        var token     = _jwtService.GenerateToken(user, roleNames);

        SetAuthCookie(token, expiresAt);

        // Token response body da emas — cookie da.
        return Ok(new UserSessionDto(
            UserId:     user.Id,
            ExpiresAt:  expiresAt,
            Roles:      roleNames,
            ActiveRole: roleNames.FirstOrDefault() ?? "Unassigned"
        ));
    }

    /// <summary>Tizimga kirish. Rate limit: 5 urinish / 1 daqiqa (IP bo'yicha).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login-policy")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto.PhoneNumber is null || dto.Password is null)
            return BadRequest("PhoneNumber va Password kiritilishi shart.");

        var user      = await _userService.LoginAsync(dto.PhoneNumber, dto.Password);
        var roles     = await _userService.GetUserRolesAsync(user.Id);
        var roleNames = roles.Select(r => r.Name).ToList();
        var expiresAt = DateTime.UtcNow.AddMinutes(CookieMinutes);
        var token     = _jwtService.GenerateToken(user, roleNames);

        SetAuthCookie(token, expiresAt);

        return Ok(new UserSessionDto(
            UserId:     user.Id,
            ExpiresAt:  expiresAt,
            Roles:      roleNames,
            ActiveRole: roleNames.FirstOrDefault() ?? "Unassigned"
        ));
    }

    /// <summary>
    /// Joriy sessiyani tekshirish.
    /// Cookie mavjud va to'g'ri bo'lsa foydalanuvchi ma'lumotlarini qaytaradi.
    /// Sahifa yangilaganda Angular bu endpoint orqali sessiyani tiklaydi.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId    = GetCurrentUserId();
        var roles     = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var expClaim  = User.FindFirst("exp")?.Value;
        var expiresAt = expClaim is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime
            : DateTime.UtcNow.AddMinutes(CookieMinutes);

        return Ok(new UserSessionDto(
            UserId:     userId,
            ExpiresAt:  expiresAt,
            Roles:      roles,
            ActiveRole: roles.FirstOrDefault() ?? "Unassigned"
        ));
    }

    /// <summary>Tizimdan chiqish — HttpOnly cookie ni o'chiradi.</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        ClearAuthCookie();
        return Ok(new { message = "Muvaffaqiyatli chiqildi." });
    }

    /// <summary>Parolni o'zgartirish.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        return Ok(new { message = "Parol muvaffaqiyatli o'zgartirildi." });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetAuthCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,                  // JS orqali o'qib bo'lmaydi (XSS himoyasi)
            Secure   = true,                  // Faqat HTTPS (prod va dev HTTPS ishlatadi)
            SameSite = SameSiteMode.Strict,   // CSRF himoyasi
            Expires  = expiresAt,
            Path     = "/"
        });
    }

    private void ClearAuthCookie()
    {
        Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        });
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new InvalidOperationException("Token ichida userId topilmadi.");
        return Guid.Parse(sub);
    }
}

// ── DTOs (faqat bu controller uchun) ─────────────────────────────────────────

/// <summary>Login/Register/Me javob DTO. AccessToken QAYTARILMAYDI — cookie da.</summary>
public record UserSessionDto(
    Guid                UserId,
    DateTime            ExpiresAt,
    IEnumerable<string> Roles,
    string              ActiveRole);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);
