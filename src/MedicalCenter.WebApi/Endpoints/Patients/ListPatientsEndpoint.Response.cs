using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class ListPatientsResponse
{
    public IReadOnlyList<PatientSummaryDto> Items { get; set; } = Array.Empty<PatientSummaryDto>();
    public PaginationMetadata Metadata { get; set; } = null!;
}

public class PatientSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public static PatientSummaryDto FromPatient(Patient patient)
    {
        return new PatientSummaryDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Email = patient.Email,
            NationalId = patient.NationalId,
            DateOfBirth = patient.DateOfBirth,
            BloodType = patient.BloodType?.ToString(),
            IsActive = patient.IsActive,
            CreatedAt = patient.CreatedAt
        };
    }
}

