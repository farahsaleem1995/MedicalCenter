namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// Response DTO for list chronic diseases endpoint.
/// </summary>
public class ListChronicDiseasesResponse
{
    public List<ChronicDiseaseDto> ChronicDiseases { get; set; } = new();
}

public class ChronicDiseaseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DiagnosisDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

