using Bogus;
using MedicalCenter.Core.Aggregates.Doctors;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configuration for Doctor entities.
/// </summary>
public static class DoctorFaker
{
    private static readonly string[] MedicalSpecialties = new[]
    {
        "Cardiology", "Pediatrics", "Orthopedics", "Neurology", "Dermatology",
        "Oncology", "Psychiatry", "Radiology", "Surgery", "Internal Medicine",
        "Emergency Medicine", "Anesthesiology", "Pathology", "Ophthalmology",
        "Urology", "Gynecology", "Endocrinology", "Gastroenterology", "Pulmonology"
    };

    public static Faker<Doctor> Create()
    {
        return new Faker<Doctor>()
            .CustomInstantiator(f => Doctor.Create(
                f.Name.FullName(),
                f.Internet.Email(),
                f.Random.Replace("MD-####"),
                f.PickRandom(MedicalSpecialties)
            ))
            .FinishWith((f, d) => d.UpdateNationalId(f.Random.Replace("###########")));
    }
}

