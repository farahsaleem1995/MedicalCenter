namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Response DTO for login endpoint.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

