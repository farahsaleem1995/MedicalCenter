namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// Request DTO for list surgeries endpoint.
/// </summary>
public class ListSurgeriesRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
}

