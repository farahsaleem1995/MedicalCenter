using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.Services;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of ITokenProvider for JWT token generation and validation.
/// </summary>
public class TokenProvider(IOptions<JwtSettings> jwtSettings) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("userId", user.Id.ToString()),
            new("userRole", ((int)user.Role).ToString())
        };

        // Add role-specific claims
        switch (user)
        {
            case Doctor doctor:
                claims.Add(new Claim("specialty", doctor.Specialty));
                break;
            case HealthcareEntity healthcare:
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
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
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
        // For now, we'll validate refresh tokens by checking format
        // In production, refresh tokens should be stored in database and validated against it
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            // Basic validation - check if it's a valid base64 string
            Convert.FromBase64String(token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

