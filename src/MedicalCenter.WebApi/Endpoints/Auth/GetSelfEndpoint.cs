using FastEndpoints;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Queries;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Get current authenticated user's generic information endpoint.
/// </summary>
public class GetSelfEndpoint(
    IUserQueryService userQueryService,
    IUserContext userContext)
    : EndpointWithoutRequest<GetSelfResponse>
{
    public override void Configure()
    {
        Get("/self");
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Get current user information";
            s.Description = "Returns the authenticated user's generic information (ID, email, full name, role, active status)";
            s.Responses[200] = "User information retrieved successfully";
            s.Responses[401] = "Unauthorized";
            s.Responses[404] = "User not found";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Guid userId = userContext.UserId;

        User? user = await userQueryService.GetUserByIdAsync(userId, ct);

        if (user == null)
        {
            ThrowError("User not found", 404);
            return;
        }

        await Send.OkAsync(new GetSelfResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        }, ct);
    }
}

