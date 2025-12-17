using Ardalis.GuardClauses;
using MedicalCenter.Core.Common;

namespace MedicalCenter.Core.Aggregates;

/// <summary>
/// Healthcare entity aggregate root (hospital/clinic staff).
/// Healthcare staff can create medical records and view patient data.
/// </summary>
public class HealthcareEntity : User, IAggregateRoot
{
    public string OrganizationName { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;

    private HealthcareEntity() { } // EF Core

    public HealthcareEntity(string fullName, string email, string organizationName, string department)
        : base(fullName, email, UserRole.HealthcareStaff)
    {
        OrganizationName = organizationName;
        Department = department;
    }

    public static HealthcareEntity Create(string fullName, string email, string organizationName, string department)
    {
        Guard.Against.NullOrWhiteSpace(organizationName, nameof(organizationName));
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        
        return new HealthcareEntity(fullName, email, organizationName, department);
    }

    public void UpdateOrganization(string organizationName, string department)
    {
        Guard.Against.NullOrWhiteSpace(organizationName, nameof(organizationName));
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        OrganizationName = organizationName;
        Department = department;
    }
}
