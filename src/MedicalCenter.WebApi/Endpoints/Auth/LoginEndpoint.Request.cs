namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Request DTO for login endpoint.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

