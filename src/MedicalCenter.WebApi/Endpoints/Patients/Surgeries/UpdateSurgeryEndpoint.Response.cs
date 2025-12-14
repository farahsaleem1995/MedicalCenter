namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Response DTO for update surgery endpoint.
/// </summary>
public class UpdateSurgeryResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Surgeon { get; set; }
    public string? Notes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

