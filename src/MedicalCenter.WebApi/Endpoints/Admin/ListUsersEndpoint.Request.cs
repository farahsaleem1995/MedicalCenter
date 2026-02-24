using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.Queries;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ListUsersRequest
{
    public UserRole? Role { get; set; }

    public bool? IsActive { get; set; }

    public string? NationalId { get; set; }

    public ListUsersSortBy? SortBy { get; set; }
    public SortDirection? SortDirection { get; set; }

    public int? PageNumber { get; set; } = 1;

    public int? PageSize { get; set; } = 10;
}

