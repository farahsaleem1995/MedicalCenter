using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Patients.Specifications;

/// <summary>
/// Specification to get all active patients with medical attributes included.
/// </summary>
public class ActivePatientsSpecification : Specification<Patient>
{
    public ActivePatientsSpecification()
    {
        Query.Where(p => p.IsActive)
            .Include(p => p.Allergies)
            .Include(p => p.ChronicDiseases)
            .Include(p => p.Medications)
            .Include(p => p.Surgeries);
    }
}

