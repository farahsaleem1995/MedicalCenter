using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Encounters.Specifications;

/// <summary>
/// Specification to get encounters within a date range.
/// Includes Patient to avoid N+1 queries.
/// Ordered by OccurredOn descending (most recent first).
/// </summary>
public class EncountersByDateRangeSpecification : Specification<Encounter>
{
    public EncountersByDateRangeSpecification(DateTime fromDate, DateTime toDate)
    {
        Query.Where(e => e.OccurredOn >= fromDate && e.OccurredOn <= toDate)
            .Include(e => e.Patient)
            .OrderByDescending(e => e.OccurredOn);
    }
}

