using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.Core.Queries;

/// <summary>
/// Query service for retrieving patient entities.
/// Used for read-only operations that need optimized queries.
/// </summary>
public interface IPatientQueryService
{
    /// <summary>
    /// Gets a patient by ID.
    /// </summary>
    Task<Patient?> GetPatientByIdAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all patients with pagination and filtering.
    /// </summary>
    Task<PaginatedList<Patient>> ListPatientsAsync(
        PaginationQuery<ListPatientsQuery> query,
        CancellationToken cancellationToken = default);
}

public class ListPatientsQuery
{
    public string? SearchTerm { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
}

