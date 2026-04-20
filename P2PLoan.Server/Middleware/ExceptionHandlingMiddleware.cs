using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.Exceptions;

namespace P2PLoan.Server.Middleware;

/// <summary>
/// Barcha unhandled exceptionlarni tutib, standart JSON xato formatida qaytaradi.
/// Controller da try/catch yozish shart emas.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {Message}", ex.Message);
            await WriteErrorAsync(context, ex.StatusCode, ex.Message,
                ex is ValidationException ve ? (object?)ve.Errors : null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict");
            await WriteErrorAsync(context, 409,
                "Ma'lumot boshqa foydalanuvchi tomonidan o'zgartirildi. Sahifani yangilab qayta urinib ko'ring.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, 500, "Ichki server xatosi.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context, int status, string message, object? errors = null)
    {
        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/json";

        var body = new
        {
            status,
            message,
            errors,
            timestamp = DateTimeOffset.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
