using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PLoan.Core.DTO.Payment;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.Server.Filters;
using P2PLoan.Services.Interface;
using System.Security.Claims;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Hisobga pul qo'shish (deposit).</summary>
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] CreatePaymentDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Miqdor musbat bo'lishi kerak.");

        var userId  = GetCurrentUserId();
        var payment = await _paymentService.CreateDepositAsync(userId, dto.Amount, dto.Provider);

        return Ok(new
        {
            payment.Id,
            payment.Amount,
            payment.Provider,
            payment.Status,
            message = "To'lov yaratildi. Provider callback kutilmoqda."
        });
    }

    /// <summary>
    /// Provider callback (Payme / Click / UzumPay dan keladi).
    /// HMAC-SHA256 imzo tekshiriladi: X-Webhook-Signature: sha256=&lt;hex&gt;
    /// </summary>
    [HttpPost("callback")]
    [AllowAnonymous]                                // Provider dan keladi — token yo'q
    [ServiceFilter(typeof(ValidateWebhookSignatureFilter))]  // HMAC imzo tekshiruvi
    public async Task<IActionResult> Callback([FromBody] PaymentCallbackDto dto)
    {
        await _paymentService.ProcessCallbackAsync(dto);
        return Ok(new { message = "OK" });
    }

    /// <summary>Repayment to'lash.</summary>
    [HttpPost("pay-repayment/{repaymentId:guid}")]
    public async Task<IActionResult> PayRepayment(Guid repaymentId)
    {
        var userId = GetCurrentUserId();
        await _paymentService.PayRepaymentAsync(repaymentId, userId);
        return Ok(new { message = "To'lov muvaffaqiyatli amalga oshirildi." });
    }

    /// <summary>Mening to'lovlarim tarixi.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var userId   = GetCurrentUserId();
        var payments = await _paymentService.GetByUserAsync(userId);
        return Ok(payments.Select(p => new
        {
            p.Id, p.Amount, p.Provider, p.Status,
            p.CreatedAt, p.UpdatedAt
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
