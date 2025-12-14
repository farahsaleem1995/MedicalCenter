namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Request DTO for update chronic disease endpoint.
/// </summary>
public class UpdateChronicDiseaseRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Chronic disease ID from route.
    /// </summary>
    public Guid ChronicDiseaseId { get; set; }
    
    public string? Notes { get; set; }
}

