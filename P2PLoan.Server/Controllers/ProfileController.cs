using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.CreditScore;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Entities;
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
    private readonly ApplicationDbContext  _context;
    private readonly ICreditScoringService _creditScoring;

    public ProfileController(
        ApplicationDbContext context,
        ICreditScoringService creditScoring)
    {
        _context       = context;
        _creditScoring = creditScoring;
    }

    // ── Borrower Profile ────────────────────────────────────────────────────

    /// <summary>Borrower profilini olish.</summary>
    [HttpGet("borrower")]
    public async Task<IActionResult> GetBorrowerProfile()
    {
        var userId  = GetCurrentUserId();
        var profile = await _context.BorrowerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.UserId == userId);

        if (profile is null) return NotFound("Borrower profil topilmadi.");

        return Ok(new BorrowerProfileDto
        {
            UserId         = userId,
            PassportNumber = profile.PassportNumber,
            BirthDate      = profile.BirthDate,
            MonthlyIncome  = profile.MonthlyIncome,
            ExistingDebt   = profile.ExistingDebt,
            KycStatus      = profile.KycStatus,
            CreditScore    = profile.CreditScore,
            CreditRating   = profile.CreditRating,
            LastScoredAt   = profile.LastScoredAt
        });
    }

    /// <summary>Borrower profilini yaratish yoki yangilash.</summary>
    [HttpPut("borrower")]
    public async Task<IActionResult> UpsertBorrowerProfile([FromBody] UpdateBorrowerProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId  = GetCurrentUserId();
        var profile = await _context.BorrowerProfiles
            .FirstOrDefaultAsync(bp => bp.UserId == userId);

        if (profile is null)
        {
            profile = new BorrowerProfile { UserId = userId };
            _context.BorrowerProfiles.Add(profile);
        }

        profile.PassportNumber    = dto.PassportNumber;
        profile.PassportIssuedDate= dto.PassportIssuedDate;
        profile.BirthDate         = dto.BirthDate;
        profile.MonthlyIncome     = dto.MonthlyIncome;
        profile.ExistingDebt      = dto.ExistingDebt;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Profil yangilandi." });
    }

    // ── Credit Score ────────────────────────────────────────────────────────

    /// <summary>Kredit ballini hisoblash (recalculate).</summary>
    [HttpPost("borrower/credit-score/calculate")]
    public async Task<IActionResult> CalculateCreditScore()
    {
        var userId  = GetCurrentUserId();
        var profile = await _context.BorrowerProfiles
            .FirstOrDefaultAsync(bp => bp.UserId == userId)
            ?? throw new NotFoundException("Borrower profil topilmadi. Avval profilingizni to'ldiring.");

        var result = await _creditScoring.CalculateAndSaveAsync(profile.Id);
        return Ok(result);
    }

    /// <summary>Oxirgi kredit ball ma'lumotini olish.</summary>
    [HttpGet("borrower/credit-score")]
    public async Task<IActionResult> GetCreditScore()
    {
        var userId  = GetCurrentUserId();
        var profile = await _context.BorrowerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.UserId == userId);

        if (profile is null) return NotFound("Borrower profil topilmadi.");

        var result = await _creditScoring.GetLatestScoreAsync(profile.Id);
        if (result is null)
            return Ok(new { message = "Kredit ball hali hisoblanmagan. /calculate ga murojaat qiling." });

        return Ok(result);
    }

    // ── Lender Profile ──────────────────────────────────────────────────────

    /// <summary>Lender profilini yaratish yoki yangilash.</summary>
    [HttpPut("lender")]
    public async Task<IActionResult> UpsertLenderProfile([FromBody] LenderProfileDto dto)
    {
        var userId  = GetCurrentUserId();
        var profile = await _context.LenderProfiles
            .FirstOrDefaultAsync(lp => lp.UserId == userId);

        if (profile is null)
        {
            profile = new LenderProfile { UserId = userId };
            _context.LenderProfiles.Add(profile);
        }

        profile.PreferredMinAmount = dto.PreferredMinAmount;
        profile.PreferredMaxAmount = dto.PreferredMaxAmount;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Lender profil yangilandi." });
    }

    /// <summary>Lender profilini olish.</summary>
    [HttpGet("lender")]
    public async Task<IActionResult> GetLenderProfile()
    {
        var userId  = GetCurrentUserId();
        var profile = await _context.LenderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(lp => lp.UserId == userId);

        if (profile is null) return NotFound("Lender profil topilmadi.");

        return Ok(new LenderProfileDto
        {
            UserId            = userId,
            PreferredMinAmount= profile.PreferredMinAmount,
            PreferredMaxAmount= profile.PreferredMaxAmount
        });
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
