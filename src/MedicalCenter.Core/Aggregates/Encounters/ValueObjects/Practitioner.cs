using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.Encounters.ValueObjects;

/// <summary>
/// Practitioner value object for Encounters.
/// Represents the healthcare practitioner who was responsible for the encounter.
/// Contains snapshot of practitioner information at the time of the encounter.
/// This is a separate value object from MedicalRecords.Practitioner to avoid coupling.
/// </summary>
public class Practitioner : ValueObject
{
    public string FullName { get; } = string.Empty;
    public UserRole Role { get; }

    private Practitioner() { } // EF Core

    private Practitioner(string fullName, UserRole role)
    {
        FullName = fullName;
        Role = role;
    }

    public static Practitioner Create(string fullName, UserRole role)
    {
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against.InvalidInput(role, nameof(role), r => Enum.IsDefined(typeof(UserRole), r), "Role must be a valid enum value.");

        return new Practitioner(fullName, role);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return Role;
    }
}

