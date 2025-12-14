namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Request DTO for create medication endpoint.
/// </summary>
public class CreateMedicationRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

