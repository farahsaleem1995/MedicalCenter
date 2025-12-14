namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ListUsersRequest
{
    public string? Role { get; set; }
    
    public bool? IsActive { get; set; }

    public int? PageNumber { get; set; } = 1;

    public int? PageSize { get; set; } = 10;
}

