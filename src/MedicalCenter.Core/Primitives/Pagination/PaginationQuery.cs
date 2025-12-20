namespace MedicalCenter.Core.Primitives.Pagination;

public class PaginationQuery(int pageNumber = 1, int pageSize = 10)
{
    public int PageNumber { get; init; } = pageNumber;
    public int PageSize { get; init; } = pageSize;
}

public class PaginationQuery<TCriteria>(int pageNumber = 1, int pageSize = 10) 
    : PaginationQuery(pageNumber, pageSize) where TCriteria : class
{
    public TCriteria? Criteria { get; init; }
}