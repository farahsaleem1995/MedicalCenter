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
            UpdatedAt = patient.UpdatedAt
        };
    }
}

