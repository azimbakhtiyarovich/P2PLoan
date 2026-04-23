using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Exceptions;
using P2PLoan.Services.Interface;
using System.Security.Claims;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService       _profileService;
    private readonly ICreditScoringService _creditScoring;

    public ProfileController(
        IProfileService profileService,
        ICreditScoringService creditScoring)
    {
        _profileService = profileService;
        _creditScoring  = creditScoring;
    }

    /// <summary>Profilni olish.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId  = GetCurrentUserId();
        var profile = await _profileService.GetProfileAsync(userId);

        if (profile is null) return NotFound("Profil topilmadi.");

        return Ok(profile);
    }

    /// <summary>Profilni yangilash.</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        await _profileService.UpdateProfileAsync(userId, dto);
        return Ok(new { message = "Profil yangilandi." });
    }

    // ── Credit Score ────────────────────────────────────────────────────────

    /// <summary>Kredit ballini hisoblash (recalculate).</summary>
    [HttpPost("credit-score/calculate")]
    public async Task<IActionResult> CalculateCreditScore()
    {
        var userId = GetCurrentUserId();
        var result = await _creditScoring.CalculateAndSaveAsync(userId);
        return Ok(result);
    }

    /// <summary>Oxirgi kredit ball ma'lumotini olish.</summary>
    [HttpGet("credit-score")]
    public async Task<IActionResult> GetCreditScore()
    {
        var userId = GetCurrentUserId();
        var result = await _creditScoring.GetLatestScoreAsync(userId);

        if (result is null)
            return Ok(new { message = "Kredit ball hali hisoblanmagan. /calculate ga murojaat qiling." });

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new UnauthorizedException();
        return Guid.Parse(sub);
    }
}
