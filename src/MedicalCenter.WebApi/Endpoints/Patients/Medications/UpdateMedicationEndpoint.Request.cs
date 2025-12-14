namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Request DTO for update medication endpoint.
/// </summary>
public class UpdateMedicationRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Medication ID from route.
    /// </summary>
    public Guid MedicationId { get; set; }
    
    public string? Dosage { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

