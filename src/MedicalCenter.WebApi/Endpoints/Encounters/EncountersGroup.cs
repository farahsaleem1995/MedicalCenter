using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Encounters;

/// <summary>
/// OpenAPI group for encounter endpoints.
/// </summary>
public class EncountersGroup : Group
{
    public EncountersGroup()
    {
        Configure("encounters", ep =>
        {
            ep.Description(d => d
                .WithTags("Encounters")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}

