using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of ITokenProvider for JWT token generation, validation, and refresh token management.
/// </summary>
public class TokenProvider(
    IOptions<JwtSettings> jwtSettings,
    IDateTimeProvider dateTimeProvider,
    MedicalCenterDbContext context,
    IUnitOfWork unitOfWork) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Add role-specific claims
        switch (user)
        {
            case Doctor doctor:
                claims.Add(new Claim("specialty", doctor.Specialty));
                break;
            case HealthcareStaff healthcare:
                claims.Add(new Claim("organizationName", healthcare.OrganizationName));
                break;
            case Laboratory lab:
                claims.Add(new Claim("labName", lab.LabName));
                break;
            case ImagingCenter imaging:
                claims.Add(new Claim("centerName", imaging.CenterName));
                break;
        }

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

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateAccessToken(string token, out ClaimsPrincipal? principal)
    {
        principal = null;

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
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateRefreshToken(string token)
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

    public async Task<Result> SaveRefreshTokenAsync(
        string token,
        Guid userId,
        DateTime expiryDate,
        CancellationToken cancellationToken = default)
    {
        // Remove any existing refresh tokens for this user (single active token per user)
        var existingTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);

        context.RefreshTokens.RemoveRange(existingTokens);

        // Add new refresh token
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiryDate = expiryDate
        };

        context.RefreshTokens.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Guid>> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
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

