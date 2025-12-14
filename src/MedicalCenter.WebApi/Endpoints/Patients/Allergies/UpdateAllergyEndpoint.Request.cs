namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Request DTO for update allergy endpoint.
/// </summary>
public class UpdateAllergyRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Allergy ID from route.
    /// </summary>
    public Guid AllergyId { get; set; }
    
    public string? Severity { get; set; }
    public string? Notes { get; set; }
}

