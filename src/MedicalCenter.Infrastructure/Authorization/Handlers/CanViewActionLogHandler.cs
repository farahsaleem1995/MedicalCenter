using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Authorization.Requirements;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Authorization.Handlers;

/// <summary>
/// Authorization handler for CanViewActionLog requirement.
/// Checks if user is SystemAdmin role OR has any AdminTier claim in the database.
/// </summary>
public class CanViewActionLogHandler : AuthorizationHandler<CanViewActionLogRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CanViewActionLogHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanViewActionLogRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        string? userIdClaim = context.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return;
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return;
        }

        // Check if user is SystemAdmin role
        IList<string> userRoles = await _userManager.GetRolesAsync(user);
        bool isSystemAdmin = userRoles.Contains(UserRole.SystemAdmin.ToString());
        
        if (isSystemAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        // Check if user has any AdminTier claim
        IList<System.Security.Claims.Claim> userClaims = await _userManager.GetClaimsAsync(user);
        bool hasAdminTierClaim = userClaims.Any(c => c.Type == IdentityClaimTypes.AdminTier);

        if (hasAdminTierClaim)
        {
            context.Succeed(requirement);
        }
    }
}
