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

        return services;
    }

    /// <summary>JWT Bearer authentication sozlamasini qo'shadi.</summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwt    = configuration.GetSection("JwtSettings");
        var secret = jwt["SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey topilmadi.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
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
