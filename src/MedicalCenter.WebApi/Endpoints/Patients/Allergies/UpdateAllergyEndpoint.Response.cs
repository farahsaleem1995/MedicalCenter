namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Response DTO for update allergy endpoint.
/// </summary>
public class UpdateAllergyResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? Notes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

