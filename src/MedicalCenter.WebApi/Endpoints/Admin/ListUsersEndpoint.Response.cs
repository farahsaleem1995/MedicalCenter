using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ListUsersResponse
{
    public IReadOnlyList<GetUserResponse> Items { get; set; } = Array.Empty<GetUserResponse>();
    public PaginationMetadata Metadata { get; set; } = null!;
}

