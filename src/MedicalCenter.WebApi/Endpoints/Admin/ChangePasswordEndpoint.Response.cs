namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ChangePasswordResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}

