using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.Encounters;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Extensions;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IEncounterQueryService for querying encounters.
/// Uses DbContext directly for optimized queries with includes.
/// </summary>
public class EncounterQueryService(MedicalCenterDbContext dbContext) : IEncounterQueryService
{
    public async Task<Encounter?> GetEncounterByIdAsync(Guid encounterId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Encounter>()
            .Include(e => e.Patient)
            .Include(e => e.Practitioner)
            .FirstOrDefaultAsync(e => e.Id == encounterId, cancellationToken);
    }

    public async Task<PaginatedList<Encounter>> ListEncountersAsync(
        PaginationQuery<ListEncountersQuery> query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Encounter> dbQuery = dbContext.Set<Encounter>()
            .Include(e => e.Patient)
            .Include(e => e.Practitioner);

        var criteria = query.Criteria;
        if (criteria != null)
        {
            dbQuery = dbQuery.Where(e => !criteria.PatientId.HasValue || e.PatientId == criteria.PatientId.Value)
                .Where(e => !criteria.DateFrom.HasValue || e.OccurredOn >= criteria.DateFrom.Value)
                .Where(e => !criteria.DateTo.HasValue || e.OccurredOn <= criteria.DateTo.Value);
        }

        return await dbQuery.OrderByDescending(e => e.OccurredOn)
            .ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }
}

