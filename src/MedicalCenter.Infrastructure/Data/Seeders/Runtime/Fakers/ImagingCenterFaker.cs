using Bogus;
using MedicalCenter.Core.Aggregates.ImagingCenters;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configuration for ImagingCenter entities.
/// </summary>
public static class ImagingCenterFaker
{
    private static readonly string[] CenterNames = new[]
    {
        "Radiology Imaging Center", "MRI Diagnostic Center", "Advanced Imaging Services",
        "Medical Imaging Center", "Diagnostic Radiology", "CT Scan Center",
        "Imaging Diagnostics", "Radiology Services", "Medical Imaging Lab"
    };

    public static Faker<ImagingCenter> Create()
    {
        return new Faker<ImagingCenter>()
            .CustomInstantiator(f => ImagingCenter.Create(
                f.Name.FullName(),
                f.Internet.Email(),
                f.PickRandom(CenterNames),
                f.Random.Replace("IMG-####")
            ))
            .FinishWith((f, i) => i.UpdateNationalId(f.Random.Replace("###########")));
    }
}

