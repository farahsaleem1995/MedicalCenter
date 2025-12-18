using Microsoft.AspNetCore.Authorization;

namespace MedicalCenter.Infrastructure.Authorization.Requirements;

/// <summary>
/// Authorization requirement: User must have Certification claim "HIPAA" or "PHI-Access"
/// to access Protected Health Information.
/// </summary>
public class CanAccessPHIRequirement : IAuthorizationRequirement
{
}

