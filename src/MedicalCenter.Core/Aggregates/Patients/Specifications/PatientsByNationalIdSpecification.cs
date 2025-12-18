using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Patients.Specifications;

/// <summary>
/// Specification to get a patient by national ID.
/// </summary>
public class PatientsByNationalIdSpecification : Specification<Patient>
{
    public PatientsByNationalIdSpecification(string nationalId)
    {
        Query.Where(p => p.NationalId == nationalId);
    }
}

