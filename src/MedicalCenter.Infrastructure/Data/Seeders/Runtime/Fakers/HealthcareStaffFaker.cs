using Bogus;
using MedicalCenter.Core.Aggregates.HealthcareStaff;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configuration for HealthcareStaff entities.
/// </summary>
public static class HealthcareStaffFaker
{
    private static readonly string[] OrganizationNames = new[]
    {
        "City General Hospital", "Regional Medical Center", "Community Health Clinic",
        "Metropolitan Hospital", "Riverside Medical Center", "Sunset Healthcare Facility",
        "Downtown Medical Center", "Valley Hospital", "Coastal Health Services"
    };

    private static readonly string[] Departments = new[]
    {
        "Emergency", "ICU", "Surgery", "Pediatrics", "Cardiology", "Oncology",
        "Radiology", "Laboratory", "Pharmacy", "Nursing", "Administration"
    };

    public static Faker<HealthcareStaff> Create()
    {
        return new Faker<HealthcareStaff>()
            .CustomInstantiator(f => HealthcareStaff.Create(
                f.Name.FullName(),
                f.Internet.Email(),
                f.PickRandom(OrganizationNames),
                f.PickRandom(Departments)
            ));
    }
}

