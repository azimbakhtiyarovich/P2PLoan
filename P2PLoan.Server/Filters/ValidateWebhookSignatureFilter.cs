using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace P2PLoan.Server.Filters;

/// <summary>
/// Provider callbacklarini HMAC-SHA256 imzo bilan tekshiradi.
///
/// Provider har bir so'rovga quyidagi header qo'shishi kerak:
///   X-Webhook-Signature: sha256=&lt;hex-encoded-hmac&gt;
///
/// HMAC hisoblash: HMACSHA256(key: PaymentWebhookSecret, data: raw-request-body)
///
/// Ishlatish:
///   [ServiceFilter(typeof(ValidateWebhookSignatureFilter))]
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateWebhookSignatureFilter : ActionFilterAttribute, IAsyncActionFilter
{
    private const string SignatureHeader = "X-Webhook-Signature";
    private const string SignaturePrefix = "sha256=";

    async Task IAsyncActionFilter.OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        // ── 1. Signature headerini tekshir ────────────────────────────────────
        if (!httpContext.Request.Headers.TryGetValue(SignatureHeader, out var sigValues)
            || sigValues.Count == 0)
        {
            context.Result = Unauthorized(httpContext, "X-Webhook-Signature header mavjud emas.");
            return;
        }

        var sigHeader = sigValues[0]!;
        if (!sigHeader.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = Unauthorized(httpContext, "Signature formati noto'g'ri. Kutilgan: sha256=<hex>");
            return;
        }

        var receivedHex = sigHeader[SignaturePrefix.Length..];

        // ── 2. Webhook secretni konfiguratsiyadan o'qi ────────────────────────
        var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var secret = config["PaymentWebhookSecret"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            var logger = httpContext.RequestServices
                .GetRequiredService<ILogger<ValidateWebhookSignatureFilter>>();
            logger.LogError("PaymentWebhookSecret konfiguratsiyada yo'q. user-secrets yoki env var o'rnating.");
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            return;
        }

        // ── 3. Raw request body ni o'qi (buffering kerak) ────────────────────
        httpContext.Request.EnableBuffering();
        using var reader = new StreamReader(
            httpContext.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var bodyText = await reader.ReadToEndAsync();
        httpContext.Request.Body.Position = 0; // Controller ham o'qiy olishi uchun

        // ── 4. HMAC-SHA256 hisoblash ──────────────────────────────────────────
        var keyBytes  = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(bodyText);

        byte[] expectedBytes;
        using (var hmac = new HMACSHA256(keyBytes))
            expectedBytes = hmac.ComputeHash(bodyBytes);

        // ── 5. Constant-time taqqoslash (timing attack dan himoya) ───────────
        byte[] receivedBytes;
        try
        {
            receivedBytes = Convert.FromHexString(receivedHex);
        }
        catch (FormatException)
        {
            context.Result = Unauthorized(httpContext, "Signature hex formati noto'g'ri.");
            return;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes))
        {
            var logger = httpContext.RequestServices
                .GetRequiredService<ILogger<ValidateWebhookSignatureFilter>>();
            logger.LogWarning("Webhook signature tekshiruvi muvaffaqiyatsiz. IP: {IP}",
                httpContext.Connection.RemoteIpAddress);

            context.Result = Unauthorized(httpContext, "Imzo tekshiruvi muvaffaqiyatsiz.");
            return;
        }

        await next();
    }

    private static IActionResult Unauthorized(HttpContext ctx, string message)
    {
        ctx.Response.ContentType = "application/json";
        return new JsonResult(new { status = 401, message })
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }
}
