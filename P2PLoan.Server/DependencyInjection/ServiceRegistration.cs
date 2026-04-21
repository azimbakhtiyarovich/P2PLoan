using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using P2PLoan.Services.Interface;
using P2PLoan.Services.Service;
using System.Text;

namespace P2PLoan.Server.DependencyInjection;

public static class ServiceRegistration
{
    /// <summary>Barcha application servislarini DI container ga ro'yxatdan o'tkazadi.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService,          UserService>();
        services.AddScoped<ILoanService,          LoanService>();
        services.AddScoped<IInvestmentService,    InvestmentService>();
        services.AddScoped<IPaymentService,       PaymentService>();
        services.AddScoped<INotificationService,  NotificationService>();
        services.AddScoped<IWalletService,        WalletService>();
        services.AddScoped<IAuditService,         AuditService>();
        services.AddScoped<ICreditScoringService, CreditScoringService>();
        services.AddScoped<IJwtService,           JwtService>();
        services.AddScoped<IProfileService,       ProfileService>();

        return services;
    }

    /// <summary>
    /// JWT Bearer authentication sozlamasini qo'shadi.
    /// Token <c>access_token</c> HttpOnly cookie dan o'qiladi.
    /// Dev: dotnet user-secrets set "JwtSettings:SecretKey" "..."
    /// Prod: JwtSettings__SecretKey env variable
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwt    = configuration.GetSection("JwtSettings");
        var secret = jwt["SecretKey"]
            ?? throw new InvalidOperationException(
                "JwtSettings:SecretKey topilmadi. " +
                "Dev uchun: dotnet user-secrets set \"JwtSettings:SecretKey\" \"...\" | " +
                "Prod uchun: JwtSettings__SecretKey env variable o'rnating.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Token HTTP Authorization header emas, HttpOnly cookie dan olinadi
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("access_token", out var cookieToken)
                            && !string.IsNullOrEmpty(cookieToken))
                        {
                            ctx.Token = cookieToken;
                        }
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                   Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer           = true,
                    ValidIssuer              = jwt["Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = jwt["Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };
            });

        return services;
    }
}
