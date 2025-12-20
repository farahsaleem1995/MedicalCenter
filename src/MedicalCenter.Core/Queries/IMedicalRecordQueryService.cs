using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.Core.Queries;

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
        PaginationQuery<ListRecordsQuery> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists medical records for a patient with pagination and filtering.
    /// Includes Patient to avoid N+1 queries.
    /// Practitioner is automatically loaded as an owned entity.
    /// </summary>
    Task<PaginatedList<MedicalRecord>> ListRecordsByPatientAsync(
        PaginationQuery<ListRecordsByPatientQuery> query,
        CancellationToken cancellationToken = default);
}

public class ListRecordsQuery
{
    public Guid? PatientId { get; set; }
    public Guid? PractitionerId { get; set; }
    public RecordType? RecordType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ListRecordsByPatientQuery
{
    public Guid PatientId { get; set; }
    public RecordType? RecordType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

