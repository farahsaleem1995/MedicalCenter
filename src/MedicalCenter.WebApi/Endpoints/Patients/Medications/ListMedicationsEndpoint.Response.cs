namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Response DTO for list medications endpoint.
/// </summary>
public class ListMedicationsResponse
{
    public List<MedicationDto> Medications { get; set; } = new();
}

public class MedicationDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

