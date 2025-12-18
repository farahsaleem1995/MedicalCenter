using Microsoft.AspNetCore.Authorization;

namespace MedicalCenter.Infrastructure.Authorization.Requirements;

/// <summary>
/// Authorization requirement: User must be SystemAdmin role OR have any AdminTier claim
/// to view audit trail entries.
/// </summary>
public class CanViewAuditTrailRequirement : IAuthorizationRequirement
{
}

