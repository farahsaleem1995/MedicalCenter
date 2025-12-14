using MedicalCenter.Core.Common;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Entities;

/// <summary>
/// Abstract base class for all users in the medical center system.
/// Users are domain entities (not aggregates) except for Patient which is an aggregate.
/// </summary>
public abstract class User : BaseEntity, IAuditableEntity
{
    public string FullName { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;
    public UserRole Role { get; protected set; }
    public bool IsActive { get; protected set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected User() { } // EF Core

    protected User(string fullName, string email, UserRole role)
    {
        FullName = fullName;
        Email = email;
        Role = role;
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
}

