namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// Response DTO for create allergy endpoint.
/// </summary>
public class CreateAllergyResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

