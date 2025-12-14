namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Response DTO for create chronic disease endpoint.
/// </summary>
public class CreateChronicDiseaseResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DiagnosisDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

