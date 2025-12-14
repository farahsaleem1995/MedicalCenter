namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Response DTO for list surgeries endpoint.
/// </summary>
public class ListSurgeriesResponse
{
    public List<SurgeryDto> Surgeries { get; set; } = new();
}

public class SurgeryDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Surgeon { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

