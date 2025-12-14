namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ChangePasswordRequest
{
    public Guid Id { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

