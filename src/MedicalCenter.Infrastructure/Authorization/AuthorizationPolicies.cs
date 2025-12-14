namespace MedicalCenter.Infrastructure.Authorization;

/// <summary>
/// Constants for authorization policy names.
/// Use these constants instead of magic strings to ensure type safety and avoid typos.
/// </summary>
public static class AuthorizationPolicies
{
    // Basic role policies
    public const string RequirePatient = "RequirePatient";
    public const string RequireDoctor = "RequireDoctor";
    public const string RequireAdmin = "RequireAdmin";

    // Composite role policies
    public const string RequireProvider = "RequireProvider";
    public const string RequirePatientOrProvider = "RequirePatientOrProvider";

    // Claims-based policies
    public const string CanModifyMedicalAttributes = "CanModifyMedicalAttributes";
    public const string CanCreateRecords = "CanCreateRecords";
    public const string CanViewAllPatients = "CanViewAllPatients";
}

