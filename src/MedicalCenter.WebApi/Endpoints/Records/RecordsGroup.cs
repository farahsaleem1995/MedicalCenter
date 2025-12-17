using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// OpenAPI group for medical records endpoints.
/// </summary>
public class RecordsGroup : Group
{
    public RecordsGroup()
    {
        Configure("records", ep =>
        {
            ep.Description(d => d
                .WithTags("Records")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}
