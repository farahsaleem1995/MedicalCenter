using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.HealthcareStaff;

/// <summary>
/// Healthcare staff aggregate root (hospital/clinic staff).
/// Healthcare staff can create medical records and view patient data.
/// </summary>
public class HealthcareStaff : User, IAggregateRoot
{
    public string OrganizationName { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;

    private HealthcareStaff() { } // EF Core

    public HealthcareStaff(string fullName, string email, string organizationName, string department)
        : base(fullName, email, UserRole.HealthcareStaff)
    {
        OrganizationName = organizationName;
        Department = department;
    }

    public static HealthcareStaff Create(string fullName, string email, string organizationName, string department)
    {
        Guard.Against.NullOrWhiteSpace(organizationName, nameof(organizationName));
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        
        return new HealthcareStaff(fullName, email, organizationName, department);
    }

    public void UpdateOrganization(string organizationName, string department)
    {
        Guard.Against.NullOrWhiteSpace(organizationName, nameof(organizationName));
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        OrganizationName = organizationName;
        Department = department;
    }
}

