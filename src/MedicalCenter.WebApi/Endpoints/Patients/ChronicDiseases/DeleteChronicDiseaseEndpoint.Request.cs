namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Request DTO for delete chronic disease endpoint.
/// </summary>
public class DeleteChronicDiseaseRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Chronic disease ID from route.
    /// </summary>
    public Guid ChronicDiseaseId { get; set; }
}

