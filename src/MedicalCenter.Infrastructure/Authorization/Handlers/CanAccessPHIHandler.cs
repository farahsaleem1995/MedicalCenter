using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Infrastructure.Authorization.Requirements;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Authorization.Handlers;

/// <summary>
/// Authorization handler for CanAccessPHI requirement.
/// Checks if user has Certification claim "HIPAA" or "PHI-Access" in the database.
/// </summary>
public class CanAccessPHIHandler : AuthorizationHandler<CanAccessPHIRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CanAccessPHIHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanAccessPHIRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var userIdClaim = context.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
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

