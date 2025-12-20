using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Encounters.Specifications;

/// <summary>
/// Specification to get encounters for a specific patient.
/// Includes Patient to avoid N+1 queries.
/// Ordered by OccurredOn descending (most recent first).
/// </summary>
public class EncountersByPatientSpecification : Specification<Encounter>
{
    public EncountersByPatientSpecification(Guid patientId)
    {
        Query.Where(e => e.PatientId == patientId)
            .Include(e => e.Patient)
            .OrderByDescending(e => e.OccurredOn);
    }
}

