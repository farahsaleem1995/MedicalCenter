namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Response DTO for update medication endpoint.
/// </summary>
public class UpdateMedicationResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

