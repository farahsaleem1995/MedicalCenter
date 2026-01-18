namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime;

/// <summary>
/// Options for database seeding operations.
/// </summary>
public class SeedingOptions
{
    public int DoctorCount { get; set; } = 20;
    public int HealthcareStaffCount { get; set; } = 15;
    public int LaboratoryCount { get; set; } = 5;
    public int ImagingCenterCount { get; set; } = 5;
    public int PatientCount { get; set; } = 100;
    public int MedicalRecordsPerPatientMin { get; set; } = 2;
    public int MedicalRecordsPerPatientMax { get; set; } = 10;
    public bool ClearExistingData { get; set; } = false;
    
    /// <summary>
    /// Default password for all seeded users.
    /// </summary>
    public string DefaultPassword { get; set; } = "Test@123!";
}

