using MedicalCenter.Core.Aggregates.Encounters;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.Core.Queries;

/// <summary>
/// Query service for retrieving encounters with Patient populated.
/// Used for read-only operations that need optimized queries with includes.
/// </summary>
public interface IEncounterQueryService
{
    /// <summary>
    /// Gets an encounter by ID with Patient included.
    /// Practitioner is automatically loaded as an owned entity.
    /// </summary>
    Task<Encounter?> GetEncounterByIdAsync(Guid encounterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all encounters with pagination and filtering.
    /// Includes Patient to avoid N+1 queries.
    /// Practitioner is automatically loaded as an owned entity.
    /// </summary>
    Task<PaginatedList<Encounter>> ListEncountersAsync(
        PaginationQuery<ListEncountersQuery> query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Query criteria for listing encounters.
/// </summary>
public class ListEncountersQuery
{
    public Guid? PatientId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

