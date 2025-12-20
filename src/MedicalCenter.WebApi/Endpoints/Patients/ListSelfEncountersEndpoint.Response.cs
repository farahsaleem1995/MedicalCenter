using MedicalCenter.Core.Aggregates.Encounters;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class ListSelfEncountersResponse
{
    public IReadOnlyCollection<PatientEncounterSummaryDto> Encounters { get; set; } = [];
    public PaginationMetadata Metadata { get; set; } = null!;
}

public class PatientEncounterSummaryDto
{
    public Guid Id { get; set; }
    public PractitionerDto Practitioner { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
    public string Reason { get; set; } = string.Empty;

    public static PatientEncounterSummaryDto FromEncounter(Encounter encounter)
    {
        return new PatientEncounterSummaryDto
        {
            Id = encounter.Id,
            Practitioner = new PractitionerDto
            {
                FullName = encounter.Practitioner.FullName,
                Role = encounter.Practitioner.Role
            },
            OccurredOn = encounter.OccurredOn,
            Reason = encounter.Reason
        };
    }

    public class PractitionerDto
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

