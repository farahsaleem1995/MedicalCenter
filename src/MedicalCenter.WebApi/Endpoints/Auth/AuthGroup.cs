using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// OpenAPI group for authentication endpoints.
/// </summary>
public class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure("auth", ep =>
        {
            ep.Description(d => d
                .WithTags("Authentication")
                .Produces(401)
                .Produces(403));
        });
    }
}

