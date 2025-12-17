using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Query service for retrieving medical records with Patient and Practitioner populated.
/// Used for read-only operations that need optimized queries with includes.
/// </summary>
public interface IMedicalRecordQueryService
{
    /// <summary>
    /// Gets a medical record by ID with Patient included.
    /// Practitioner is automatically loaded as an owned entity.
    /// </summary>
    Task<MedicalRecord?> GetRecordByIdAsync(Guid recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all medical records with pagination and filtering.
    /// Includes Patient to avoid N+1 queries.
    /// Practitioner is automatically loaded as an owned entity.
    /// </summary>
    Task<PaginatedList<MedicalRecord>> ListRecordsAsync(
        int pageNumber,
        int pageSize,
        Guid? practitionerId = null,
        Guid? patientId = null,
        RecordType? recordType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists medical records for a patient with pagination and filtering.
    /// Includes Patient to avoid N+1 queries.
    /// Practitioner is automatically loaded as an owned entity.
    /// </summary>
    Task<PaginatedList<MedicalRecord>> ListRecordsByPatientAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        RecordType? recordType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);
}
