using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Dev;

/// <summary>
/// Development-only endpoints group.
/// These endpoints should only be available in Development environment.
/// </summary>
public class DevGroup : Group
{
    public DevGroup()
    {
        Configure("dev", ep =>
        {
            ep.Description(d => d
                .WithTags("Dev")
                .Produces(200)
                .Produces(400)
                .Produces(500));
        });
    }
}

