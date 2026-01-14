using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.Queries;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Extensions;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IPatientQueryService for querying patient entities.
/// Uses DbContext directly for optimized queries.
/// </summary>
public class PatientQueryService(MedicalCenterDbContext dbContext) : IPatientQueryService
{
    public async Task<Patient?> GetPatientByIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Patient>()
            .IgnoreQueryFilters() // Include inactive patients if needed
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
    }

    public async Task<PaginatedList<Patient>> ListPatientsAsync(
        PaginationQuery<ListPatientsQuery> query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Patient> dbQuery = dbContext.Set<Patient>();

        var criteria = query.Criteria;
        if (criteria != null)
        {
            // Follow the same clean chained-filter style as MedicalRecordQueryService.
            // Note: Patients have a global query filter (IsActive) in EF configuration,
            // so listing patients always returns active patients only.
            string? searchTerm = criteria.SearchTerm?.Trim().ToLower();

            dbQuery = dbQuery
                .Where(p =>
                    string.IsNullOrWhiteSpace(searchTerm) ||
                    p.FullName.ToLower().Contains(searchTerm) ||
                    p.Email.ToLower().Contains(searchTerm) ||
                    p.NationalId.ToLower().Contains(searchTerm))
                .Where(p =>
                    !criteria.DateOfBirthFrom.HasValue ||
                    p.DateOfBirth >= criteria.DateOfBirthFrom.Value)
                .Where(p =>
                    !criteria.DateOfBirthTo.HasValue ||
                    p.DateOfBirth <= criteria.DateOfBirthTo.Value);
        }

        return await dbQuery.ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }
}

