using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Encounters.Specifications;

/// <summary>
/// Specification to get an encounter by ID.
/// Includes Patient to avoid N+1 queries.
/// </summary>
public class EncounterByIdSpecification : Specification<Encounter>
{
    public EncounterByIdSpecification(Guid encounterId)
    {
        Query.Where(e => e.Id == encounterId)
            .Include(e => e.Patient);
    }
}

