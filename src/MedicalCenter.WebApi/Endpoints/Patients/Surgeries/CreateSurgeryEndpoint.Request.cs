namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Request DTO for create surgery endpoint.
/// </summary>
public class CreateSurgeryRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Surgeon { get; set; }
    public string? Notes { get; set; }
}

