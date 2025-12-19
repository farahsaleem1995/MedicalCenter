using System.Security.Claims;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for JWT token generation, validation, and refresh token management.
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
    /// Validates a refresh token format (stateless validation).
    /// </summary>
    bool ValidateRefreshToken(string token);

    /// <summary>
    /// Saves a refresh token for a user.
    /// </summary>
    Task<Result> SaveRefreshTokenAsync(
        string token,
        Guid userId,
        DateTime expiryDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and retrieves user ID from a refresh token.
    /// </summary>
    Task<Result<Guid>> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token (marks it as invalid).
    /// </summary>
    Task<Result> RevokeRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    Task<Result> RevokeUserRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

