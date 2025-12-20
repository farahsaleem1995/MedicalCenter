using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IIdentityService using ASP.NET Core Identity.
/// </summary>
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    IDateTimeProvider dateTimeProvider)
    : IIdentityService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        UserRole role,
        bool requireEmailConfirmation = false,
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
            EmailConfirmed = !requireEmailConfirmation // Set based on flag
        };

        var identityResult = await userManager.CreateAsync(identityUser, password);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description);
            return Result<Guid>.Failure(Error.Validation(string.Join("; ", errors)));
        }

        // Add role
        var roleResult = await userManager.AddToRoleAsync(identityUser, role.ToString());
        if (!roleResult.Succeeded)
        {
            // Rollback: delete user if role assignment fails
            await userManager.DeleteAsync(identityUser);
            var errors = roleResult.Errors.Select(e => e.Description);
            return Result<Guid>.Failure(Error.Validation(string.Join("; ", errors)));
        }

        return Result<Guid>.Success(userId);
    }

    public async Task<Guid?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);
        if (identityUser == null)
        {
            return null;
        }
        return identityUser.Id;
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

    public async Task<Result> UpdatePasswordAsync(
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

        return Result.Success();
    }


    public async Task<Result<Guid>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);
        if (identityUser == null)
        {
            return Result<Guid>.Failure(Error.Unauthorized("Invalid email or password."));
        }

        var isValidPassword = await userManager.CheckPasswordAsync(identityUser, password);
        if (!isValidPassword)
        {
            return Result<Guid>.Failure(Error.Unauthorized("Invalid email or password."));
        }

        // Check if user account is locked out (inactive)
        var isLockedOut = identityUser.LockoutEnabled && 
                         identityUser.LockoutEnd != null && 
                         identityUser.LockoutEnd > DateTimeOffset.UtcNow;
        if (isLockedOut)
        {
            return Result<Guid>.Failure(Error.Unauthorized("User account is inactive."));
        }

        return Result<Guid>.Success(identityUser.Id);
    }


    public async Task<bool> IsUserUnconfirmedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return false; // User doesn't exist
        }

        return !identityUser.EmailConfirmed;
    }

    public async Task<Result<string>> GenerateEmailConfirmationCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return Result<string>.Failure(Error.NotFound("User"));
        }

        if (identityUser.EmailConfirmed)
        {
            return Result<string>.Failure(Error.Validation("Email is already confirmed."));
        }

        // Use Identity's built-in TOTP token provider for email confirmation
        // This generates a 6-digit numeric code
        var code = await userManager.GenerateTwoFactorTokenAsync(
            identityUser, 
            TokenOptions.DefaultEmailProvider);

        return Result<string>.Success(code);
    }

    public async Task<Result> ConfirmEmailAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        if (identityUser.EmailConfirmed)
        {
            return Result.Failure(Error.Validation("Email is already confirmed."));
        }

        // Verify the TOTP code using Identity's built-in verification
        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(
            identityUser,
            TokenOptions.DefaultEmailProvider,
            code);

        if (!isValidCode)
        {
            return Result.Failure(Error.Validation("Invalid or expired confirmation code."));
        }

        // Generate email confirmation token and confirm email
        var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);
        var result = await userManager.ConfirmEmailAsync(identityUser, token);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Result.Failure(Error.Validation(string.Join("; ", errors)));
        }

        return Result.Success();
    }

    public async Task<Result<string>> GeneratePasswordResetCodeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return Result<string>.Failure(Error.NotFound("User"));
        }

        // Use Identity's built-in TOTP token provider for password reset
        // This generates a 6-digit numeric code
        var code = await userManager.GenerateTwoFactorTokenAsync(
            identityUser,
            TokenOptions.DefaultEmailProvider);

        return Result<string>.Success(code);
    }

    public async Task<Result> ResetPasswordAsync(
        Guid userId,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser == null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        // Verify the TOTP code using Identity's built-in verification
        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(
            identityUser,
            TokenOptions.DefaultEmailProvider,
            code);

        if (!isValidCode)
        {
            return Result.Failure(Error.Validation("Invalid or expired reset code."));
        }

        // Generate password reset token and reset password
        var token = await userManager.GeneratePasswordResetTokenAsync(identityUser);
        var result = await userManager.ResetPasswordAsync(identityUser, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Result.Failure(Error.Validation(string.Join("; ", errors)));
        }

        return Result.Success();
    }
}

