using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.Repositories;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IIdentityService using ASP.NET Core Identity.
/// </summary>
public class IdentityService(
    MedicalCenterDbContext context,
    UserManager<ApplicationUser> userManager,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings)
    : IIdentityService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result<Guid>.Failure(Error.Conflict("A user with this email already exists."));
        }

        // Create Identity user
        var userId = Guid.NewGuid();
        var identityUser = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = true // For now, we'll skip email confirmation
        };

        var identityResult = await userManager.CreateAsync(identityUser, password);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description);
            return Result<Guid>.Failure(Error.Validation(string.Join("; ", errors)));
        }

        // Add role
        await userManager.AddToRoleAsync(identityUser, role.ToString());

        return Result<Guid>.Success(userId);
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        var result = await userManager.ChangePasswordAsync(identityUser, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Result.Failure(Error.Validation(string.Join("; ", errors)));
        }

        return Result.Success();
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // For now, we'll query Patient only
        // This will be expanded when we have SQL views for all user types
        return await patientRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);
        if (identityUser == null)
        {
            return null;
        }

        return await GetUserByIdAsync(identityUser.Id, cancellationToken);
    }

    public async Task<Result<User>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);
        if (identityUser == null)
        {
            return Result<User>.Failure(Error.Unauthorized("Invalid email or password."));
        }

        var isValidPassword = await userManager.CheckPasswordAsync(identityUser, password);
        if (!isValidPassword)
        {
            return Result<User>.Failure(Error.Unauthorized("Invalid email or password."));
        }

        var user = await GetUserByIdAsync(identityUser.Id, cancellationToken);
        if (user == null)
        {
            return Result<User>.Failure(Error.NotFound("User"));
        }

        if (!user.IsActive)
        {
            return Result<User>.Failure(Error.Unauthorized("User account is inactive."));
        }

        return Result<User>.Success(user);
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

        if (refreshToken.ExpiryDate < DateTime.UtcNow)
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

        context.RefreshTokens.Remove(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

