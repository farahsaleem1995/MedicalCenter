using MedicalCenter.Core.Aggregates.Encounters;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Endpoints.Encounters;

public class ListEncountersResponse
{
    public IReadOnlyList<EncounterSummaryDto> Items { get; set; } = [];
    public PaginationMetadata Metadata { get; set; } = null!;
}

public class EncounterSummaryDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public PatientSummaryDto? Patient { get; set; }
    public PractitionerDto Practitioner { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
    public string Reason { get; set; } = string.Empty;

    public static EncounterSummaryDto FromEncounter(Encounter encounter)
    {
        return new EncounterSummaryDto
        {
            Id = encounter.Id,
            PatientId = encounter.PatientId,
            Patient = encounter.Patient != null ? new PatientSummaryDto
            {
                Id = encounter.Patient.Id,
                FullName = encounter.Patient.FullName,
                Email = encounter.Patient.Email
            } : null,
            Practitioner = new PractitionerDto
            {
                FullName = encounter.Practitioner.FullName,
                Role = encounter.Practitioner.Role
            },
            OccurredOn = encounter.OccurredOn,
            Reason = encounter.Reason
        };
    }

    public class PatientSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class PractitionerDto
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

