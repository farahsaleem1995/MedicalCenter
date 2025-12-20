using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Infrastructure.Authorization.Requirements;
using MedicalCenter.Infrastructure.Identity;
using MedicalCenter.Core.Services;

namespace MedicalCenter.Infrastructure.Authorization.Handlers;

/// <summary>
/// Authorization handler for CanAccessPHI requirement.
/// Checks if user has Certification claim "HIPAA" or "PHI-Access" in the database.
/// </summary>
public class CanAccessPHIHandler(
    UserManager<ApplicationUser> userManager,
    IUserContext userContext) : AuthorizationHandler<CanAccessPHIRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserContext _userContext = userContext;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanAccessPHIRequirement requirement)
    {
        if (!_userContext.IsAuthenticated)
        {
            return;
        }

        Guid userId = _userContext.UserId;

        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return;
        }

        var userClaims = await _userManager.GetClaimsAsync(user);
        bool hasPHIAccess = userClaims.Any(c => 
            c.Type == IdentityClaimTypes.Certification 
            && (c.Value == "HIPAA" || c.Value == "PHI-Access"));

        if (hasPHIAccess)
        {
            context.Succeed(requirement);
        }
    }
}

