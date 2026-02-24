using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;

namespace MedicalCenter.Core.SharedKernel;

/// <summary>
/// Abstract base class for all users in the medical center system.
/// </summary>
public abstract class User : BaseEntity, IAuditableEntity
{
    public string FullName { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;
    public string NationalId { get; protected set; } = string.Empty;
    public UserRole Role { get; protected set; }
    public bool IsActive { get; protected set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected User() { } // EF Core

    protected User(Guid id, string fullName, string email, UserRole role, string nationalId = "")
    {
        Guard.Against.Default(id, nameof(id));
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));
        Guard.Against.OutOfRange((int)role, nameof(role), 1, Enum.GetValues<UserRole>().Length);

        Id = id;
        FullName = fullName;
        Email = email;
        Role = role;
        NationalId = nationalId ?? string.Empty;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateFullName(string fullName)
    {
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        FullName = fullName;
    }

    public void UpdateNationalId(string nationalId)
    {
        NationalId = nationalId ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}

