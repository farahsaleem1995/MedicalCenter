namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Request DTO for delete surgery endpoint.
/// </summary>
public class DeleteSurgeryRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Surgery ID from route.
    /// </summary>
    public Guid SurgeryId { get; set; }
}

