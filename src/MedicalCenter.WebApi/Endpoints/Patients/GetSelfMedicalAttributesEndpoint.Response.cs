namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Response DTO for get self medical attributes endpoint.
/// </summary>
public class GetSelfMedicalAttributesResponse
{
    public List<AllergySummaryDto> Allergies { get; set; } = new();
    public List<ChronicDiseaseSummaryDto> ChronicDiseases { get; set; } = new();
    public List<MedicationSummaryDto> Medications { get; set; } = new();
    public List<SurgerySummaryDto> Surgeries { get; set; } = new();
}

public class AllergySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? Notes { get; set; }
}

public class ChronicDiseaseSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DiagnosisDate { get; set; }
    public string? Notes { get; set; }
}

public class MedicationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class SurgerySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Surgeon { get; set; }
    public string? Notes { get; set; }
}

