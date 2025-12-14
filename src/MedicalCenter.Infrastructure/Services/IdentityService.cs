using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

        // Invalidate all refresh tokens for this user
        await InvalidateUserRefreshTokensAsync(userId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AdminChangePasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        // Remove password
        var removeResult = await userManager.RemovePasswordAsync(identityUser);
        if (!removeResult.Succeeded)
        {
            var errors = removeResult.Errors.Select(e => e.Description);
            return Result.Failure(Error.Validation(string.Join("; ", errors)));
        }

        // Add new password
        var addResult = await userManager.AddPasswordAsync(identityUser, newPassword);
        if (!addResult.Succeeded)
        {
            var errors = addResult.Errors.Select(e => e.Description);
            return Result.Failure(Error.Validation(string.Join("; ", errors)));
        }

        // Invalidate all refresh tokens for this user
        await InvalidateUserRefreshTokensAsync(userId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Guid>> CreateDoctorAsync(
        string fullName,
        string email,
        string password,
        string licenseNumber,
        string specialty,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create Identity user
            var createUserResult = await CreateUserAsync(email, password, UserRole.Doctor, cancellationToken);
            if (createUserResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return createUserResult;
            }

            var userId = createUserResult.Value;

            // Create domain entity
            var doctor = Doctor.Create(fullName, email, licenseNumber, specialty);
            SetEntityId(doctor, userId);
            context.Doctors.Add(doctor);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Guid>.Success(userId);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Guid>> CreateHealthcareEntityAsync(
        string fullName,
        string email,
        string password,
        string organizationName,
        string department,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create Identity user
            var createUserResult = await CreateUserAsync(email, password, UserRole.HealthcareStaff, cancellationToken);
            if (createUserResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return createUserResult;
            }

            var userId = createUserResult.Value;

            // Create domain entity
            var healthcareEntity = HealthcareEntity.Create(fullName, email, organizationName, department);
            SetEntityId(healthcareEntity, userId);
            context.HealthcareEntities.Add(healthcareEntity);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Guid>.Success(userId);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Guid>> CreateLaboratoryAsync(
        string fullName,
        string email,
        string password,
        string labName,
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create Identity user
            var createUserResult = await CreateUserAsync(email, password, UserRole.LabUser, cancellationToken);
            if (createUserResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return createUserResult;
            }

            var userId = createUserResult.Value;

            // Create domain entity
            var laboratory = Laboratory.Create(fullName, email, labName, licenseNumber);
            SetEntityId(laboratory, userId);
            context.Laboratories.Add(laboratory);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Guid>.Success(userId);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Guid>> CreateImagingCenterAsync(
        string fullName,
        string email,
        string password,
        string centerName,
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create Identity user
            var createUserResult = await CreateUserAsync(email, password, UserRole.ImagingUser, cancellationToken);
            if (createUserResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return createUserResult;
            }

            var userId = createUserResult.Value;

            // Create domain entity
            var imagingCenter = ImagingCenter.Create(fullName, email, centerName, licenseNumber);
            SetEntityId(imagingCenter, userId);
            context.ImagingCenters.Add(imagingCenter);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<Guid>.Success(userId);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static void SetEntityId(User entity, Guid id)
    {
        var idProperty = typeof(Core.Common.BaseEntity).GetProperty(nameof(Core.Common.BaseEntity.Id));
        idProperty?.SetValue(entity, id);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(id.ToString());
        if (identityUser == null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        if (roles.Count == 0 || !Enum.TryParse<UserRole>(roles[0], out var role))
        {
            return null;
        }

        return new IdentityUserWrapper(identityUser, role);
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

        if (refreshToken.Revoked)
        {
            return Result<Guid>.Failure(Error.Unauthorized("Refresh token has been revoked."));
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

        refreshToken.Revoked = true;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> InvalidateUserRefreshTokensAsync(
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

    /// <summary>
    /// Simple wrapper to convert Identity ApplicationUser to domain User.
    /// </summary>
    private class IdentityUserWrapper : User
    {
        public IdentityUserWrapper(ApplicationUser identityUser, UserRole role)
            : base(identityUser.UserName ?? identityUser.Email ?? string.Empty, identityUser.Email ?? string.Empty, role)
        {
            Id = identityUser.Id;
            IsActive = !identityUser.LockoutEnabled || identityUser.LockoutEnd == null || identityUser.LockoutEnd <= DateTimeOffset.UtcNow;
        }
    }
}

