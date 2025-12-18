namespace MedicalCenter.Core.Authorization;

/// <summary>
/// Defines claim types that describe user identity attributes.
/// Claims answer "WHO is this user?" not "WHAT can they do?"
/// 
/// IMPORTANT: Claims are stored ONLY in the database (AspNetUserClaims table),
/// NOT in JWT tokens. This avoids token size issues since claims are unlimited.
/// </summary>
public static class IdentityClaimTypes
{
    /// <summary>
    /// Administrative tier within the organization.
    /// Values: "Super", "Standard"
    /// </summary>
    public const string AdminTier = "MedicalCenter.AdminTier";
    
    /// <summary>
    /// Department the user belongs to (identity attribute).
    /// Values: e.g., "IT", "Medical Administration", "HR"
    /// </summary>
    public const string Department = "MedicalCenter.Department";
    
    /// <summary>
    /// Professional certifications held by the user.
    /// Values: e.g., "HIPAA", "PHI-Access"
    /// </summary>
    public const string Certification = "MedicalCenter.Certification";
}

/// <summary>
/// Well-known claim values for type safety.
/// </summary>
public static class IdentityClaimValues
{
    public static class AdminTier
    {
        /// <summary>
        /// Super admin - can manage other SystemAdmin accounts.
        /// </summary>
        public const string Super = "Super";
        
        /// <summary>
        /// Standard admin - cannot manage other SystemAdmin accounts.
        /// </summary>
        public const string Standard = "Standard";
    }
}

