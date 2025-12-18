using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using Microsoft.AspNetCore.Identity;

namespace MedicalCenter.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user entity.
/// This is the Identity framework user, separate from domain User entities.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // Additional properties can be added here if needed
    // The domain User entities (Patient, Doctor, etc.) are separate and linked via Id

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public HealthcareStaff? HealthcareStaff { get; set; }
    public Laboratory? Laboratory { get; set; }
    public ImagingCenter? ImagingCenter { get; set; }
    public SystemAdmin? SystemAdmin { get; set; }
    public ICollection<ApplicationUserRole> Roles { get; set; } = [];
}

