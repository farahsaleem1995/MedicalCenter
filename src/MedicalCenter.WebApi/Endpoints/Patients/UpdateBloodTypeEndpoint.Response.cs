namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Response DTO for update blood type endpoint.
/// </summary>
public class UpdateBloodTypeResponse
{
    /// <summary>
    /// Patient ID.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// Updated blood type (e.g., "A+", "B-", "AB+", "O-") or null if cleared.
    /// </summary>
    public string? BloodType { get; set; }
}
