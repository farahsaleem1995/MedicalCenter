using Microsoft.AspNetCore.Authorization;

namespace MedicalCenter.WebApi.Authorization.Requirements;

/// <summary>
/// Authorization requirement: User must have Certification claim "HIPAA" or "PHI-Access"
/// to access Protected Health Information.
/// </summary>
public class CanAccessPHIRequirement : IAuthorizationRequirement
{
}

