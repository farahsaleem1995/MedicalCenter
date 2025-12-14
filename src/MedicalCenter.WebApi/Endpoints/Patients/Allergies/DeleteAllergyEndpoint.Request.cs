namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Request DTO for delete allergy endpoint.
/// </summary>
public class DeleteAllergyRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Allergy ID from route.
    /// </summary>
    public Guid AllergyId { get; set; }
}

