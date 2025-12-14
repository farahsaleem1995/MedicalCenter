namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Request DTO for delete medication endpoint.
/// </summary>
public class DeleteMedicationRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Medication ID from route.
    /// </summary>
    public Guid MedicationId { get; set; }
}

