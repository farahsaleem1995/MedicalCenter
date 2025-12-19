using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.ActionLogs;

/// <summary>
/// OpenAPI group for action log endpoints.
/// </summary>
public class ActionLogsGroup : Group
{
    public ActionLogsGroup()
    {
        Configure("action-logs", ep =>
        {
            ep.Description(d => d
                .WithTags("Action-Logs")
                .Produces(401)
                .Produces(403));
        });
    }
}

