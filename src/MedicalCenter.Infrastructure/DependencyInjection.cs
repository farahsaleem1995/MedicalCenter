using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Queries;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Data.Interceptors;
using MedicalCenter.Infrastructure.Identity;
using MedicalCenter.Infrastructure.Options;
using MedicalCenter.Infrastructure.Repositories;
using MedicalCenter.Infrastructure.Services;
using System.IO.Abstractions;
using MedicalCenter.Core.SharedKernel.Events;

namespace MedicalCenter.Infrastructure;

/// <summary>
/// Extension methods for dependency injection configuration.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<DomainEventBase>();
        });

        // Register DbContext and Interceptors
        services.AddScoped<AuditableEntityInterceptor>(); // Register interceptor as scoped
        services.AddScoped<DomainEventDispatcherInterceptor>(); // Register domain event dispatcher interceptor
        services.AddDbContext<MedicalCenterDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
            // Interceptors are added via OnConfiguring in DbContext, which resolves them from DI
        });

        // Configure ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // Token settings - use Email provider for email confirmation (generates numeric codes)
            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        })
        .AddEntityFrameworkStores<MedicalCenterDbContext>()
        .AddDefaultTokenProviders();

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        string secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });

        // Register Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register DateTimeProvider as singleton (stateless, thread-safe)
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Register Identity Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IMedicalRecordQueryService, MedicalRecordQueryService>();

        // Configure File Storage Options
        services.Configure<FileStorageOptions>(
            configuration.GetSection(FileStorageOptions.SectionName));

        // Register File Storage Service
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IFileSystem, FileSystem>();

        // Configure JWT Settings
        services.Configure<JwtSettings>(jwtSettings);

        // Register SMTP client
        services.AddScoped<ISmtpClient, SmtpClient>();

        // Configure SMTP options
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        // Configure Action Log Options
        services.Configure<ActionLogOptions>(
            configuration.GetSection(ActionLogOptions.SectionName));

        // Register Action Log Queue (singleton - shared across requests)
        services.AddSingleton<IActionLogQueue, ActionLogQueue>();

        // Register Action Log Service (scoped - uses DbContext)
        services.AddScoped<ActionLogService>();
        services.AddScoped<IActionLogger>(sp => sp.GetRequiredService<ActionLogService>());
        services.AddScoped<IActionLogQueryService>(sp => sp.GetRequiredService<ActionLogService>());

        // Register Action Log Background Service
        services.AddHostedService<ActionLogBackgroundService>();

        return services;
    }
}

/// <summary>
/// JWT settings configuration class.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}

