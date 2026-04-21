using Microsoft.EntityFrameworkCore;
using P2PLoan.DataAccess;
using P2PLoan.Server.DependencyInjection;
using P2PLoan.Server.Filters;
using P2PLoan.Server.Middleware;
using P2PLoan.Services.Service;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. Application Services ───────────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<OverdueDetectionService>();

// ── 3. JWT Authentication (cookie-based) ─────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// ── 4. Rate Limiting ──────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // Login: har bir IP uchun 1 daqiqada maksimal 5 ta urinish
    options.AddPolicy("login-policy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window       = TimeSpan.FromMinutes(1),
                PermitLimit  = 5,
                QueueLimit   = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            """{"status":429,"message":"Juda ko'p urinish. 1 daqiqadan so'ng qayta urinib ko'ring."}""",
            ct);
    };
});

// ── 5. Webhook signature filter (DI orqali ServiceFilter uchun) ───────────────
builder.Services.AddScoped<ValidateWebhookSignatureFilter>();

// ── 6. Controllers + API ──────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── 7. Middleware pipeline ────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5279/scalar/v1
}

app.UseHttpsRedirection();
app.UseRateLimiter();        // Rate limiting (Authentication dan oldin)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
