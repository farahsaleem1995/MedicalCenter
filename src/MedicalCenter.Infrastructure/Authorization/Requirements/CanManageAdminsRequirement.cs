using Microsoft.AspNetCore.Authorization;

namespace MedicalCenter.Infrastructure.Authorization.Requirements;

/// <summary>
/// Authorization requirement: User must have AdminTier claim with value "Super"
/// to manage SystemAdmin accounts.
/// </summary>
public class CanManageAdminsRequirement : IAuthorizationRequirement
{
}

