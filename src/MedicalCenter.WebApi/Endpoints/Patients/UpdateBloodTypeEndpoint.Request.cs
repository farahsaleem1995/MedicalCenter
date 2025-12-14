using MedicalCenter.Core.Enums;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Request DTO for update blood type endpoint.
/// </summary>
public class UpdateBloodTypeRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
    
    /// <summary>
    /// ABO blood group type. If both ABO and Rh are null, blood type will be cleared.
    /// </summary>
    public BloodABO? ABO { get; set; }
    
    /// <summary>
    /// Rh factor type. If both ABO and Rh are null, blood type will be cleared.
    /// </summary>
    public BloodRh? Rh { get; set; }
}
