using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PLoan.Core.DTO.Loan;
using P2PLoan.Core.DTO.Payment;
using P2PLoan.Core.Exceptions;
using P2PLoan.Services.Interface;
using System.Security.Claims;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    /// <summary>Barcha ochiq loanlar.</summary>
    [HttpGet]
    public async Task<IActionResult> GetOpenLoans(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var loans = await _loanService.GetOpenLoansAsync(page, pageSize);
        return Ok(loans.Select(l => new LoanSummaryDto
        {
            Id           = l.Id,
            Title        = l.Title,
            Amount       = l.Amount,
            FundedAmount = l.FundedAmount,
            DurationDays = l.DurationDays,
            InterestRate = l.InterestRate,
            Status       = l.Status
        }));
    }

    /// <summary>Loan batafsil ma'lumoti.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);
        if (loan is null) return NotFound();

        return Ok(new LoanDetailDto
        {
            Id           = loan.Id,
            Title        = loan.Title,
            Amount       = loan.Amount,
            FundedAmount = loan.FundedAmount,
            DurationDays = loan.DurationDays,
            InterestRate = loan.InterestRate,
            Status       = loan.Status,
            Description  = loan.Description,
            CreatedAt    = loan.CreatedAt,
            Repayments   = loan.Repayments.Select(r => new RepaymentDto
            {
                Id              = r.Id,
                DueDate         = r.DueDate,
                Amount          = r.Amount,
                PrincipalAmount = r.PrincipalAmount,
                InterestAmount  = r.InterestAmount,
                PaidAmount      = r.PaidAmount,
                Status          = r.Status
            })
        });
    }

    /// <summary>Mening loanlarim.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyLoans()
    {
        var userId = GetCurrentUserId();
        var loans  = await _loanService.GetLoansByUserAsync(userId);
        return Ok(loans.Select(l => new LoanSummaryDto
        {
            Id           = l.Id,
            Title        = l.Title,
            Amount       = l.Amount,
            FundedAmount = l.FundedAmount,
            DurationDays = l.DurationDays,
            InterestRate = l.InterestRate,
            Status       = l.Status
        }));
    }

    /// <summary>Yangi loan yaratish.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var loan   = await _loanService.CreateLoanAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id },
            new { loan.Id, loan.Status, loan.Amount });
    }

    /// <summary>Kreditni qabul qilish (Funded → Active).</summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var userId = GetCurrentUserId();
        await _loanService.AcceptLoanAsync(id, userId);
        return Ok(new { message = "Kredit muvaffaqiyatli qabul qilindi va faollashtirildi." });
    }

    /// <summary>To'lov jadvali.</summary>
    [HttpGet("{id:guid}/repayments")]
    public async Task<IActionResult> GetRepayments(Guid id)
    {
        var schedule = await _loanService.GetRepaymentScheduleAsync(id);
        return Ok(schedule.Select(r => new RepaymentDto
        {
            Id              = r.Id,
            DueDate         = r.DueDate,
            Amount          = r.Amount,
            PrincipalAmount = r.PrincipalAmount,
            InterestAmount  = r.InterestAmount,
            PaidAmount      = r.PaidAmount,
            Status          = r.Status
        }));
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new UnauthorizedException();
        return Guid.Parse(sub);
    }
}
