namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Response DTO for list allergies endpoint.
/// </summary>
public class ListAllergiesResponse
{
    public List<AllergyDto> Allergies { get; set; } = new();
}

public class AllergyDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

