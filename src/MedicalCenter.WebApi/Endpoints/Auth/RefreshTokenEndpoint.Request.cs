namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Request DTO for refresh token endpoint.
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

