using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PLoan.Core.DTO.Auth;
using P2PLoan.Services.Interface;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService  _jwtService;

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

        var user  = await _userService.RegisterAsync(dto);
        var roles = await _userService.GetUserRolesAsync(user.Id);
        var token = _jwtService.GenerateToken(user, roles.Select(r => r.Name));

        return Ok(new AuthResponseDto(
            UserId:     user.Id,
            AccessToken: token,
            ExpiresAt:  DateTime.UtcNow.AddHours(1),
            Roles:      roles.Select(r => r.Name),
            ActiveRole: "Unassigned"
        ));
    }

    /// <summary>Tizimga kirish.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto.PhoneNumber is null || dto.Password is null)
            return BadRequest("PhoneNumber va Password kiritilishi shart.");

        var user  = await _userService.LoginAsync(dto.PhoneNumber, dto.Password);
        var roles = await _userService.GetUserRolesAsync(user.Id);
        var token = _jwtService.GenerateToken(user, roles.Select(r => r.Name));

        return Ok(new AuthResponseDto(
            UserId:     user.Id,
            AccessToken: token,
            ExpiresAt:  DateTime.UtcNow.AddHours(1),
            Roles:      roles.Select(r => r.Name),
            ActiveRole: roles.FirstOrDefault()?.Name ?? "Unassigned"
        ));
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

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new InvalidOperationException("Token ichida userId topilmadi.");
        return Guid.Parse(sub);
    }
}

// ── DTO (faqat bu controller uchun) ──────────────────────────────────────────
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
