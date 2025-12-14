namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Request DTO for list medications endpoint.
/// </summary>
public class ListMedicationsRequest
{
    /// <summary>
    /// Patient ID from route.
    /// </summary>
    public Guid PatientId { get; set; }
}

