namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Response DTO for create medication endpoint.
/// </summary>
public class CreateMedicationResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

