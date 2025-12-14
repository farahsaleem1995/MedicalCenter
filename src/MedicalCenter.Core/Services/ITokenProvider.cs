using System.Security.Claims;
using MedicalCenter.Core.Entities;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an access token and returns the claims principal.
    /// </summary>
    bool ValidateAccessToken(string token, out ClaimsPrincipal? principal);

    /// <summary>
    /// Validates a refresh token.
    /// </summary>
    bool ValidateRefreshToken(string token);
}

