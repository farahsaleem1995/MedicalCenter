namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Response DTO for get self endpoint.
/// </summary>
public class GetSelfResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

