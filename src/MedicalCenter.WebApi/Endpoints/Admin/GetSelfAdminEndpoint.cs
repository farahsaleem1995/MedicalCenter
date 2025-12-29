using FastEndpoints;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Get current system admin's own custom attributes endpoint.
/// </summary>
public class GetSelfAdminEndpoint(
    IRepository<SystemAdmin> systemAdminRepository,
    IUserContext userContext)
    : EndpointWithoutRequest<GetSelfAdminResponse>
{
    public override void Configure()
    {
        Get("/self");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Get current system admin's custom attributes";
            s.Description = "Returns the authenticated system admin's custom attributes (CorporateId and Department)";
            s.Responses[200] = "Admin attributes retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - user is not a system admin";
            s.Responses[404] = "System admin not found";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = userContext.UserId;

        var systemAdmin = await systemAdminRepository.GetByIdAsync(userId, ct);

        if (systemAdmin == null)
        {
            ThrowError("System admin not found", 404);
            return;
        }

        await Send.OkAsync(new GetSelfAdminResponse
        {
            CorporateId = systemAdmin.CorporateId,
            Department = systemAdmin.Department
        }, ct);
    }
}

