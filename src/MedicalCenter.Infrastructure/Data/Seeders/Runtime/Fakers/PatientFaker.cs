using Bogus;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Enums;
using MedicalCenter.Core.Aggregates.Patients.ValueObjects;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configuration for Patient entities.
/// </summary>
public static class PatientFaker
{
    public static Faker<Patient> Create()
    {
        return new Faker<Patient>()
            .CustomInstantiator(f =>
            {
                var fullName = f.Name.FullName();
                var email = f.Internet.Email(fullName.Split(' ')[0], fullName.Split(' ').Last());
                var nationalId = f.Random.Long(10000000000, 99999999999).ToString();
                var dateOfBirth = f.Date.Between(DateTime.UtcNow.AddYears(-80), DateTime.UtcNow.AddYears(-18));
                
                var patient = Patient.Create(fullName, email, nationalId, dateOfBirth);
                
                // Set blood type (70% chance of having a blood type)
                if (f.Random.Bool(0.7f))
                {
                    var abo = f.PickRandom<BloodABO>();
                    var rh = f.PickRandom<BloodRh>();
                    patient.UpdateBloodType(BloodType.Create(abo, rh));
                }
                
                return patient;
            });
    }
}

