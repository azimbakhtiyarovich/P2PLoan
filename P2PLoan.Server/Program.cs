using Microsoft.EntityFrameworkCore;
using P2PLoan.DataAccess;
using P2PLoan.Server.DependencyInjection;
using P2PLoan.Server.Middleware;
using P2PLoan.Services.Service;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. Application Services ───────────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<OverdueDetectionService>();

// ── 3. JWT Authentication ─────────────────────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// ── 4. Controllers + API ──────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── 5. Middleware pipeline ────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5279/scalar/v1
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
