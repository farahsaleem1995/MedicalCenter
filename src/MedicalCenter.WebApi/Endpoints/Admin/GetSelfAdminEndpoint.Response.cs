namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Response DTO for getting current system admin's custom attributes.
/// </summary>
public class GetSelfAdminResponse
{
    /// <summary>
    /// Unique identifier within the organization (e.g., HR-assigned staff number).
    /// </summary>
    public string CorporateId { get; set; } = string.Empty;

    /// <summary>
    /// The organizational unit or department this admin belongs to.
    /// </summary>
    public string Department { get; set; } = string.Empty;
}

