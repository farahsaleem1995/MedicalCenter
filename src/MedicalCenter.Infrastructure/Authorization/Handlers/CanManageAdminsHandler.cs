using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicalCenter.Core.Authorization;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Authorization.Requirements;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Authorization.Handlers;

/// <summary>
/// Authorization handler for CanManageAdmins requirement.
/// Checks if user has AdminTier claim with value "Super" in the database.
/// </summary>
public class CanManageAdminsHandler(
    UserManager<ApplicationUser> userManager,
    IUserContext userContext) : AuthorizationHandler<CanManageAdminsRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserContext _userContext = userContext;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanManageAdminsRequirement requirement)
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
        bool hasSuperAdminClaim = userClaims.Any(c => 
            c.Type == IdentityClaimTypes.AdminTier 
            && c.Value == IdentityClaimValues.AdminTier.Super);

        if (hasSuperAdminClaim)
        {
            context.Succeed(requirement);
        }
    }
}

