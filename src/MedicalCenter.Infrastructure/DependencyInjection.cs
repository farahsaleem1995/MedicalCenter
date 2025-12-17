using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Data.Interceptors;
using MedicalCenter.Infrastructure.Identity;
using MedicalCenter.Infrastructure.Options;
using MedicalCenter.Infrastructure.Repositories;
using MedicalCenter.Infrastructure.Services;
using System.IO.Abstractions;

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
        // Register DbContext and Interceptors
        services.AddScoped<AuditableEntityInterceptor>(); // Register interceptor as scoped
        services.AddDbContext<MedicalCenterDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
            // Interceptor is added via OnConfiguring in DbContext, which resolves it from DI
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
        })
        .AddEntityFrameworkStores<MedicalCenterDbContext>()
        .AddDefaultTokenProviders();

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

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

        // Configure Authorization Policies
        services.AddAuthorizationPolicies();

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

