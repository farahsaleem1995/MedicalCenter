using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.Services;
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
        int pageNumber,
        int pageSize,
        Guid? practitionerId = null,
        Guid? patientId = null,
        RecordType? recordType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MedicalRecord> query = dbContext.Set<MedicalRecord>()
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner);

        // Apply filters
        if (practitionerId.HasValue)
        {
            query = query.Where(mr => mr.PractitionerId == practitionerId.Value);
        }
        if (patientId.HasValue)
        {
            query = query.Where(mr => mr.PatientId == patientId.Value);
        }
        if (recordType.HasValue)
        {
            query = query.Where(mr => mr.RecordType == recordType.Value);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(mr => mr.CreatedAt >= dateFrom.Value);
        }
        if (dateTo.HasValue)
        {
            query = query.Where(mr => mr.CreatedAt <= dateTo.Value);
        }

        // Apply ordering
        query = query.OrderByDescending(mr => mr.CreatedAt);

        return await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<PaginatedList<MedicalRecord>> ListRecordsByPatientAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        RecordType? recordType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MedicalRecord> query = dbContext.Set<MedicalRecord>()
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner)
            .Where(mr => mr.PatientId == patientId);

        // Apply filters
        if (recordType.HasValue)
        {
            query = query.Where(mr => mr.RecordType == recordType.Value);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(mr => mr.CreatedAt >= dateFrom.Value);
        }
        if (dateTo.HasValue)
        {
            query = query.Where(mr => mr.CreatedAt <= dateTo.Value);
        }

        // Apply ordering
        query = query.OrderByDescending(mr => mr.CreatedAt);

        return await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
    }
}
