using Bogus;
using MedicalCenter.Core.Aggregates.Laboratories;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configuration for Laboratory entities.
/// </summary>
public static class LaboratoryFaker
{
    private static readonly string[] LabNames = new[]
    {
        "Advanced Diagnostics Lab", "Clinical Pathology Lab", "Medical Testing Center",
        "Precision Diagnostics", "BioLab Services", "Health Diagnostics Lab",
        "Central Laboratory", "Regional Testing Lab", "Comprehensive Diagnostics"
    };

    public static Faker<Laboratory> Create()
    {
        return new Faker<Laboratory>()
            .CustomInstantiator(f => Laboratory.Create(
                f.Name.FullName(),
                f.Internet.Email(),
                f.PickRandom(LabNames),
                f.Random.Replace("LAB-####")
            ));
    }
}

