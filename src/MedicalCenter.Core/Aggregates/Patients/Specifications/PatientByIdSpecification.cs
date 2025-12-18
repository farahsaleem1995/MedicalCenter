using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Patients.Specifications;

/// <summary>
/// Specification to get a patient by ID with medical attributes included.
/// </summary>
public class PatientByIdSpecification : Specification<Patient>
{
    public PatientByIdSpecification(Guid patientId)
    {
        Query.Where(p => p.Id == patientId)
            .Include(p => p.Allergies)
            .Include(p => p.ChronicDiseases)
            .Include(p => p.Medications)
            .Include(p => p.Surgeries);
    }
}

