namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Request DTO for create allergy endpoint.
/// </summary>
public class CreateAllergyRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? Notes { get; set; }
}

