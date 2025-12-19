using Microsoft.AspNetCore.Authorization;

namespace MedicalCenter.Infrastructure.Authorization.Requirements;

/// <summary>
/// Authorization requirement: User must be SystemAdmin role OR have any AdminTier claim
/// to view action log entries.
/// </summary>
public class CanViewActionLogRequirement : IAuthorizationRequirement
{
}
