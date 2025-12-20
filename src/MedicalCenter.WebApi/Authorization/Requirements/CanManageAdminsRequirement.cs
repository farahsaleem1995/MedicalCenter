using Microsoft.AspNetCore.Authorization;

namespace MedicalCenter.WebApi.Authorization.Requirements;

/// <summary>
/// Authorization requirement: User must have AdminTier claim with value "Super"
/// to manage SystemAdmin accounts.
/// </summary>
public class CanManageAdminsRequirement : IAuthorizationRequirement
{
}

