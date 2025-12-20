using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of ITokenProvider for JWT token generation, validation, and refresh token management.
/// </summary>
public class TokenProvider(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    IDateTimeProvider dateTimeProvider,
    MedicalCenterDbContext context,
    IUnitOfWork unitOfWork) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public async Task<string> GenerateAccessTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidCastException("Failed to generate access token. User not found.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
        };

        var role = await userManager.GetRolesAsync(user);
        var roleClaims = role.Select(r => new Claim(ClaimTypes.Role, r));
        claims.AddRange(roleClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: _dateTimeProvider.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = GenerateRefreshToken();

        var expiryDate = _dateTimeProvider.Now.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        
        // Add new refresh token
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiryDate = expiryDate
        };

        context.RefreshTokens.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return token;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public async Task<Result> SaveRefreshTokenAsync(
        string token,
        Guid userId,
        DateTime expiryDate,
        CancellationToken cancellationToken = default)
    {
        

        return Result.Success();
    }

    public Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        ClaimsPrincipal? principal = null;

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        catch
        {
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    public async Task<Result<Guid>> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateRefreshTokenFormat(token))
        {
            return Result<Guid>.Failure(Error.Unauthorized("Invalid refresh token."));
        }

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null)
        {
            return Result<Guid>.Failure(Error.Unauthorized("Invalid refresh token."));
        }

        if (refreshToken.Revoked)
        {
            return Result<Guid>.Failure(Error.Unauthorized("Refresh token has been revoked."));
        }

        if (refreshToken.ExpiryDate < _dateTimeProvider.Now)
        {
            // Token expired, remove it
            context.RefreshTokens.Remove(refreshToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Failure(Error.Unauthorized("Refresh token has expired."));
        }

        return Result<Guid>.Success(refreshToken.UserId);
    }

    private static bool ValidateRefreshTokenFormat(string token)
    {
        // Basic format validation - check if it's a valid base64 string
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            Convert.FromBase64String(token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Result> RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null)
        {
            return Result.Failure(Error.NotFound("Refresh token"));
        }

        refreshToken.Revoked = true;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RevokeUserRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var refreshTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.Revoked)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.Revoked = true;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

