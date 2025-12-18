using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.SystemAdmins;

/// <summary>
/// System administrator aggregate root.
/// </summary>
public class SystemAdmin : User, IAggregateRoot
{
    /// <summary>
    /// Unique identifier within the organization (e.g., HR-assigned staff number).
    /// </summary>
    public string CorporateId { get; private set; } = string.Empty;
    
    /// <summary>
    /// The organizational unit or department this admin belongs to.
    /// </summary>
    public string Department { get; private set; } = string.Empty;

    private SystemAdmin() { } // EF Core

    private SystemAdmin(string fullName, string email, string corporateId, string department)
        : base(fullName, email, UserRole.SystemAdmin)
    {
        CorporateId = corporateId;
        Department = department;
    }

    public static SystemAdmin Create(
        string fullName, 
        string email, 
        string corporateId, 
        string department)
    {
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));
        Guard.Against.NullOrWhiteSpace(corporateId, nameof(corporateId));
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        
        return new SystemAdmin(fullName, email, corporateId, department);
    }

    public void UpdateCorporateId(string corporateId)
    {
        Guard.Against.NullOrWhiteSpace(corporateId, nameof(corporateId));
        CorporateId = corporateId;
    }
    
    public void UpdateDepartment(string department)
    {
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        Department = department;
    }
}

