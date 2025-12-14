namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Request DTO for update surgery endpoint.
/// </summary>
public class UpdateSurgeryRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Surgery ID from route.
    /// </summary>
    public Guid SurgeryId { get; set; }
    
    public string? Surgeon { get; set; }
    public string? Notes { get; set; }
}

