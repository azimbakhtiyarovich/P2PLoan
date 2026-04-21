using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.CreditScore;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
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
    private readonly ApplicationDbContext  _context;

    public ProfileController(
        IProfileService profileService,
        ICreditScoringService creditScoring,
        ApplicationDbContext context)
    {
        _profileService = profileService;
        _creditScoring  = creditScoring;
        _context        = context;
    }

    // ── Borrower Profile ────────────────────────────────────────────────────

    /// <summary>Borrower profilini olish.</summary>
    [HttpGet("borrower")]
    public async Task<IActionResult> GetBorrowerProfile()
    {
        var userId  = GetCurrentUserId();
        var profile = await _profileService.GetBorrowerProfileAsync(userId);

        if (profile is null) return NotFound("Borrower profil topilmadi.");

        return Ok(profile);
    }

    /// <summary>Borrower profilini yaratish yoki yangilash.</summary>
    [HttpPut("borrower")]
    public async Task<IActionResult> UpsertBorrowerProfile([FromBody] UpdateBorrowerProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        await _profileService.UpsertBorrowerProfileAsync(userId, dto);
        return Ok(new { message = "Profil yangilandi." });
    }

    // ── Credit Score ────────────────────────────────────────────────────────

    /// <summary>Kredit ballini hisoblash (recalculate).</summary>
    [HttpPost("borrower/credit-score/calculate")]
    public async Task<IActionResult> CalculateCreditScore()
    {
        var userId  = GetCurrentUserId();
        var profileId = await _profileService.GetBorrowerProfileIdAsync(userId)
            ?? throw new NotFoundException("Borrower profil topilmadi. Avval profilingizni to'ldiring.");

        var result = await _creditScoring.CalculateAndSaveAsync(profileId);
        return Ok(result);
    }

    /// <summary>Oxirgi kredit ball ma'lumotini olish.</summary>
    [HttpGet("borrower/credit-score")]
    public async Task<IActionResult> GetCreditScore()
    {
        var userId    = GetCurrentUserId();
        var profileId = await _profileService.GetBorrowerProfileIdAsync(userId);

        if (profileId is null) return NotFound("Borrower profil topilmadi.");

        var result = await _creditScoring.GetLatestScoreAsync(profileId.Value);
        if (result is null)
            return Ok(new { message = "Kredit ball hali hisoblanmagan. /calculate ga murojaat qiling." });

        return Ok(result);
    }

    // ── Lender Profile ──────────────────────────────────────────────────────

    /// <summary>Lender profilini yaratish yoki yangilash.</summary>
    [HttpPut("lender")]
    public async Task<IActionResult> UpsertLenderProfile([FromBody] LenderProfileDto dto)
    {
        var userId = GetCurrentUserId();
        await _profileService.UpsertLenderProfileAsync(userId, dto);
        return Ok(new { message = "Lender profil yangilandi." });
    }

    /// <summary>Lender profilini olish.</summary>
    [HttpGet("lender")]
    public async Task<IActionResult> GetLenderProfile()
    {
        var userId  = GetCurrentUserId();
        var profile = await _profileService.GetLenderProfileAsync(userId);

        if (profile is null) return NotFound("Lender profil topilmadi.");

        return Ok(profile);
    }

    // ── Notifications ───────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new UnauthorizedException();
        return Guid.Parse(sub);
    }
}
