namespace MedicalCenter.Core.Common;

/// <summary>
/// Metadata for paginated results.
/// </summary>
public class PaginationMetadata
{
    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Current page number (alias for PageNumber for clarity).
    /// </summary>
    public int CurrentPage => PageNumber;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}

