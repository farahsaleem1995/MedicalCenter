using System.Security.Claims;
using MedicalCenter.Core.Primitives;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for JWT token generation, validation, and refresh token management.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    Task<string> GenerateAccessTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an access token and returns the claims principal.
    /// </summary>
    public Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a refresh token and returns the user ID.
    /// </summary>
    Task<Result<Guid>> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

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

