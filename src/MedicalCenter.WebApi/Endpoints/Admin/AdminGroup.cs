using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// OpenAPI group for admin endpoints.
/// </summary>
public class AdminGroup : Group
{
    public AdminGroup()
    {
        Configure("admin", ep =>
        {
            ep.Description(d => d
                .WithTags("Admin")
                .Produces(401)
                .Produces(403));
        });
    }
}

