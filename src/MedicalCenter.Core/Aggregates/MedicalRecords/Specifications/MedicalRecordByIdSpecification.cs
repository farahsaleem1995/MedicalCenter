using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.MedicalRecords.Specifications;

/// <summary>
/// Specification to get a medical record by ID.
/// Includes Patient and Practitioner to avoid N+1 queries.
/// </summary>
public class MedicalRecordByIdSpecification : Specification<MedicalRecord>
{
    public MedicalRecordByIdSpecification(Guid recordId)
    {
        Query.Where(mr => mr.Id == recordId)
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner);
    }
}

