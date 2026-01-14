using MedicalCenter.Core.Aggregates.Patients;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class GetPatientResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AllergySummaryDto> Allergies { get; set; } = new();
    public List<ChronicDiseaseSummaryDto> ChronicDiseases { get; set; } = new();
    public List<MedicationSummaryDto> Medications { get; set; } = new();
    public List<SurgerySummaryDto> Surgeries { get; set; } = new();

    public static GetPatientResponse FromPatient(Patient patient)
    {
        return new GetPatientResponse
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Email = patient.Email,
            NationalId = patient.NationalId,
            DateOfBirth = patient.DateOfBirth,
            BloodType = patient.BloodType?.ToString(),
            IsActive = patient.IsActive,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            Allergies = patient.Allergies.Select(a => new AllergySummaryDto
            {
                Id = a.Id,
                Name = a.Name,
                Severity = a.Severity,
                Notes = a.Notes
            }).ToList(),
            ChronicDiseases = patient.ChronicDiseases.Select(cd => new ChronicDiseaseSummaryDto
            {
                Id = cd.Id,
                Name = cd.Name,
                DiagnosisDate = cd.DiagnosisDate,
                Notes = cd.Notes
            }).ToList(),
            Medications = patient.Medications.Select(m => new MedicationSummaryDto
            {
                Id = m.Id,
                Name = m.Name,
                Dosage = m.Dosage,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Notes = m.Notes
            }).ToList(),
            Surgeries = patient.Surgeries.Select(s => new SurgerySummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Date = s.Date,
                Surgeon = s.Surgeon,
                Notes = s.Notes
            }).ToList()
        };
    }
}

