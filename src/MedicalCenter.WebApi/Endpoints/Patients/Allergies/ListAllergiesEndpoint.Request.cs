namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Request DTO for list allergies endpoint.
/// </summary>
public class ListAllergiesRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
}

