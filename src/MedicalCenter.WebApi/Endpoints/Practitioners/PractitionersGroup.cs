using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Practitioners;

/// <summary>
/// OpenAPI group for practitioner endpoints.
/// </summary>
public class PractitionersGroup : Group
{
    public PractitionersGroup()
    {
        Configure("practitioners", ep =>
        {
            ep.Description(d => d
                .WithTags("Practitioners")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}


