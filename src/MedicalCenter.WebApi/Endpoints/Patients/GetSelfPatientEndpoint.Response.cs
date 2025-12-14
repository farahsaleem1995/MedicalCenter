namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Response DTO for get self patient endpoint.
/// </summary>
public class GetSelfPatientResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public bool IsActive { get; set; }
    public List<AllergySummaryDto> Allergies { get; set; } = new();
    public List<ChronicDiseaseSummaryDto> ChronicDiseases { get; set; } = new();
    public List<MedicationSummaryDto> Medications { get; set; } = new();
    public List<SurgerySummaryDto> Surgeries { get; set; } = new();
}

