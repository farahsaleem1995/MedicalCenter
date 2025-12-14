using MedicalCenter.Core.Common;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class ListUsersResponse
{
    public IReadOnlyList<GetUserResponse> Items { get; set; } = Array.Empty<GetUserResponse>();
    public PaginationMetadata Metadata { get; set; } = null!;
}

