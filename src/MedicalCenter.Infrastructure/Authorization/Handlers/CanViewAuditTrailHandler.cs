using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Authorization.Requirements;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Authorization.Handlers;

/// <summary>
/// Authorization handler for CanViewAuditTrail requirement.
/// Checks if user is SystemAdmin role OR has any AdminTier claim in the database.
/// </summary>
public class CanViewAuditTrailHandler : AuthorizationHandler<CanViewAuditTrailRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CanViewAuditTrailHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanViewAuditTrailRequirement requirement)
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

        // Check if user is SystemAdmin role
        var userRoles = await _userManager.GetRolesAsync(user);
        bool isSystemAdmin = userRoles.Contains(UserRole.SystemAdmin.ToString());
        
        if (isSystemAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        // Check if user has any AdminTier claim
        var userClaims = await _userManager.GetClaimsAsync(user);
        bool hasAdminTierClaim = userClaims.Any(c => c.Type == IdentityClaimTypes.AdminTier);

        if (hasAdminTierClaim)
        {
            context.Succeed(requirement);
        }
    }
}

