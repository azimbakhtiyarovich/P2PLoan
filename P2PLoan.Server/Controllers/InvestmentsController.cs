using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PLoan.Core.DTO.Investment;
using P2PLoan.Core.Exceptions;
using P2PLoan.Services.Interface;
using System.Security.Claims;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvestmentsController : ControllerBase
{
    private readonly IInvestmentService _investmentService;

    public InvestmentsController(IInvestmentService investmentService)
    {
        _investmentService = investmentService;
    }

    /// <summary>Mening investitsiyalarim.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetCurrentUserId();
        var result = await _investmentService.GetByUserAsync(userId);
        return Ok(result);
    }

    /// <summary>Loan bo'yicha barcha investitsiyalar.</summary>
    [HttpGet("by-loan/{loanId:guid}")]
    public async Task<IActionResult> GetByLoan(Guid loanId)
    {
        var result = await _investmentService.GetByLoanAsync(loanId);
        return Ok(result);
    }

    /// <summary>Investitsiya qilish.</summary>
    [HttpPost]
    public async Task<IActionResult> Invest([FromBody] InvestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId     = GetCurrentUserId();
        var investment = await _investmentService.InvestAsync(userId, dto);
        return Ok(new { investment.Id, investment.Amount, investment.InvestedAt });
    }

    /// <summary>Investitsiyani qaytarish (loan faollashguncha).</summary>
    [HttpDelete("{investmentId:guid}")]
    public async Task<IActionResult> Withdraw(Guid investmentId)
    {
        var userId = GetCurrentUserId();
        await _investmentService.WithdrawInvestmentAsync(investmentId, userId);
        return Ok(new { message = "Investitsiya muvaffaqiyatli qaytarildi." });
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new UnauthorizedException();
        return Guid.Parse(sub);
    }
}
