using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.MedicalRecords.ValueObjects;

/// <summary>
/// Practitioner value object representing the healthcare practitioner who created the medical record.
/// Contains common user properties (FullName, Email, Role) for display purposes.
/// </summary>
public class Practitioner : ValueObject
{
    public string FullName { get; } = string.Empty;
    public string Email { get; } = string.Empty;
    public UserRole Role { get; }

    private Practitioner() { } // EF Core

    private Practitioner(string fullName, string email, UserRole role)
    {
        FullName = fullName;
        Email = email;
        Role = role;
    }

    public static Practitioner Create(string fullName, string email, UserRole role)
    {
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));
        Guard.Against.InvalidInput(role, nameof(role), r => Enum.IsDefined(typeof(UserRole), r), "Role must be a valid enum value.");

        return new Practitioner(fullName, email, role);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return Email;
        yield return Role;
    }
}

