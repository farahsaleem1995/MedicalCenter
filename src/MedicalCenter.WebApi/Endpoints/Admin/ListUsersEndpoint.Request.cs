using MedicalCenter.Core.Enums;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ListUsersRequest
{
    public UserRole? Role { get; set; }
    
    public bool? IsActive { get; set; }

    public int? PageNumber { get; set; } = 1;

    public int? PageSize { get; set; } = 10;
}

