using MedicalCenter.Core.Common;

namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for file storage operations.
/// Abstracts file storage implementation (local filesystem, cloud storage, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns metadata.
    /// </summary>
    Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file by its ID.
    /// </summary>
    Task<Result<FileDownloadResult>> DownloadFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its ID.
    /// </summary>
    Task<Result> DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a file upload operation.
/// </summary>
public class FileUploadResult
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}

/// <summary>
/// Result of a file download operation.
/// </summary>
public class FileDownloadResult
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
}
