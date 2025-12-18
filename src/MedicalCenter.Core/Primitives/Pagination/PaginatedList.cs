namespace MedicalCenter.Core.Primitives.Pagination;

/// <summary>
/// Represents a paginated list of items with metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PaginationMetadata Metadata { get; init; } = null!;

    /// <summary>
    /// Creates a new paginated list.
    /// </summary>
    public PaginatedList(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        Metadata = new PaginationMetadata
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates an empty paginated list.
    /// </summary>
    public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedList<T>(
            Array.Empty<T>(),
            pageNumber,
            pageSize,
            totalCount: 0);
    }
}

