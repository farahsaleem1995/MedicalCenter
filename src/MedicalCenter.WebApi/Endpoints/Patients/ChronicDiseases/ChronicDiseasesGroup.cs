using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Patients.ChronicDiseases;

/// <summary>
/// OpenAPI group for patient chronic diseases endpoints.
/// </summary>
public class ChronicDiseasesGroup : Group
{
    public ChronicDiseasesGroup()
    {
        Configure("chronic-diseases", ep =>
        {
            ep.Description(d => d
                .WithTags("Chronic-Diseases")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}

