using Microsoft.AspNetCore.Identity;

namespace MedicalCenter.Infrastructure.Data.Seeders;

/// <summary>
/// Helper class to generate password hashes for seeding migrations.
/// This uses the same password hasher that ASP.NET Core Identity uses.
/// </summary>
public static class PasswordHashGenerator
{
    /// <summary>
    /// Generates a password hash for the given password using ASP.NET Core Identity's default hasher.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password string.</returns>
    public static string GenerateHash(string password)
    {
        var hasher = new PasswordHasher<object>();
        return hasher.HashPassword(null!, password);
    }
}

