namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Request DTO for patient registration endpoint.
/// </summary>
public class RegisterPatientRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

