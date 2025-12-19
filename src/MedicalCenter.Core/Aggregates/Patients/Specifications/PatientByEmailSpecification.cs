using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Patients.Specifications;

/// <summary>
/// Specification to get a patient by email address.
/// </summary>
public class PatientByEmailSpecification : Specification<Patient>
{
    public PatientByEmailSpecification(string email)
    {
        Query.Where(p => p.Email == email);
    }
}

