using Ardalis.Specification;
using MedicalCenter.Core.Aggregates.MedicalRecord;

namespace MedicalCenter.Core.Aggregates.MedicalRecord.Specifications;

/// <summary>
/// Specification to get medical records for a specific patient.
/// Global query filter automatically excludes inactive (soft-deleted) records.
/// Includes Patient and Practitioner to avoid N+1 queries.
/// </summary>
public class MedicalRecordsByPatientSpecification : Specification<MedicalRecord>
{
    public MedicalRecordsByPatientSpecification(Guid patientId)
    {
        Query.Where(mr => mr.PatientId == patientId)
            .Include(mr => mr.Patient)
            .Include(mr => mr.Practitioner)
            .OrderByDescending(mr => mr.CreatedAt);
    }
}
