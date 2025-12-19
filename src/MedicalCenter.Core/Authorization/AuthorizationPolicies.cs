namespace MedicalCenter.Core.Authorization;

/// <summary>
/// Defines all authorization policy names used throughout the application.
/// Policies determine what actions users CAN DO based on their roles and claims.
/// 
/// All policies are evaluated using ASP.NET Core's authorization framework:
/// - Role-based policies: Evaluated from JWT token claims via RequireRole()
/// - Claims-based policies: Evaluated via custom IAuthorizationHandler implementations (database lookup)
/// 
/// Consumers (endpoints) should use these constants without caring about the evaluation mechanism.
/// </summary>
public static class AuthorizationPolicies
{
    // Basic role policies (evaluated from JWT token)
    public const string RequirePatient = "RequirePatient";
    public const string RequireDoctor = "RequireDoctor";
    public const string RequireAdmin = "RequireAdmin";

    // Composite role policies (evaluated from JWT token)
    public const string RequirePractitioner = "RequirePractitioner";
    public const string RequirePatientOrPractitioner = "RequirePatientOrPractitioner";

    // Medical attributes policies (evaluated from JWT token)
    public const string CanViewMedicalAttributes = "CanViewMedicalAttributes";
    public const string CanModifyMedicalAttributes = "CanModifyMedicalAttributes";
    
    // Records policies (evaluated from JWT token)
    public const string CanViewRecords = "CanViewRecords";
    public const string CanModifyRecords = "CanModifyRecords";
    
    // Other role-based policies (evaluated from JWT token)
    public const string CanViewAllPatients = "CanViewAllPatients";
    
    // Claims-based policies (evaluated via database lookup)
    /// <summary>
    /// Policy: Can manage (create/update/delete) SystemAdmin accounts.
    /// Requirement: AdminTier claim with value "Super" (checked via database)
    /// </summary>
    public const string CanManageAdmins = "CanManageAdmins";
    
    /// <summary>
    /// Policy: Can view action log entries.
    /// Requirement: SystemAdmin role OR any AdminTier claim (checked via database)
    /// </summary>
    public const string CanViewActionLog = "CanViewActionLog";
    
    /// <summary>
    /// Policy: Can access PHI (Protected Health Information).
    /// Requirement: Certification claim "HIPAA" or "PHI-Access" (checked via database)
    /// </summary>
    public const string CanAccessPHI = "CanAccessPHI";
}

