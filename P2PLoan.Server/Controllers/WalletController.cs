using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PLoan.Core.Exceptions;
using P2PLoan.Services.Interface;
using System.Security.Claims;

namespace P2PLoan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService       _walletService;
    private readonly INotificationService _notifications;

    public WalletController(
        IWalletService walletService,
        INotificationService notifications)
    {
        _walletService = walletService;
        _notifications = notifications;
    }

    /// <summary>Hisob balansi.</summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetCurrentUserId();
        var dto    = await _walletService.GetBalanceAsync(userId);
        return Ok(dto);
    }

    /// <summary>Tranzaksiyalar tarixi.</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var txs    = await _walletService.GetTransactionsAsync(userId, page, pageSize);
        return Ok(txs);
    }

    /// <summary>Xabarnomalar (notifications).</summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        var items = unreadOnly
            ? await _notifications.GetUnreadAsync(userId)
            : await _notifications.GetAllAsync(userId, page, pageSize);

        return Ok(items.Select(n => new
        {
            n.Id, n.Title, n.Message, n.Read, n.CreatedAt
        }));
    }

    /// <summary>Xabarnomani o'qilgan deb belgilash.</summary>
    [HttpPost("notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = GetCurrentUserId();
        await _notifications.MarkAsReadAsync(id, userId);
        return Ok(new { message = "O'qilgan." });
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value
               ?? throw new UnauthorizedException();
        return Guid.Parse(sub);
    }
}
