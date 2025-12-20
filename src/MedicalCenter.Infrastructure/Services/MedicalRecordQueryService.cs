using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Extensions;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IMedicalRecordQueryService for querying medical records.
/// Uses DbContext directly for optimized queries with includes.
/// </summary>
public class MedicalRecordQueryService(MedicalCenterDbContext dbContext) : IMedicalRecordQueryService
{
    public async Task<MedicalRecord?> GetRecordByIdAsync(Guid recordId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<MedicalRecord>()
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner)
            .FirstOrDefaultAsync(mr => mr.Id == recordId, cancellationToken);
    }

    public async Task<PaginatedList<MedicalRecord>> ListRecordsAsync(
        PaginationQuery<ListRecordsQuery> query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MedicalRecord> dbQuery = dbContext.Set<MedicalRecord>()
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner);

        var criteria = query.Criteria;
        if (criteria != null)
        {
            dbQuery = dbQuery.Where(mr => mr.PractitionerId == criteria.PractitionerId)
                .Where(mr => criteria.PatientId.HasValue && mr.PatientId == criteria.PatientId.Value)
                .Where(mr => criteria.RecordType.HasValue && mr.RecordType == criteria.RecordType.Value)
                .Where(mr => criteria.DateFrom.HasValue && mr.CreatedAt >= criteria.DateFrom.Value)
                .Where(mr => criteria.DateTo.HasValue && mr.CreatedAt <= criteria.DateTo.Value);
        }

        return await dbQuery.ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }

    public async Task<PaginatedList<MedicalRecord>> ListRecordsByPatientAsync(
        PaginationQuery<ListRecordsByPatientQuery> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query.Criteria?.PatientId, nameof(query.Criteria.PatientId));

        var criteria = query.Criteria;

        IQueryable<MedicalRecord> dbQuery = dbContext.Set<MedicalRecord>()
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner)
            .Where(mr => mr.PatientId == criteria.PatientId)
            .Where(mr => criteria.RecordType.HasValue && mr.RecordType == criteria.RecordType.Value)
            .Where(mr => criteria.DateFrom.HasValue && mr.CreatedAt >= criteria.DateFrom.Value)
            .Where(mr => criteria.DateTo.HasValue && mr.CreatedAt <= criteria.DateTo.Value);

        // Apply ordering
        dbQuery = dbQuery.OrderByDescending(mr => mr.CreatedAt);

        return await dbQuery.ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }
}
