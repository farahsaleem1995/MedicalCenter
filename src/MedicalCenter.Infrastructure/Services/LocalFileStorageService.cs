using System.IO.Abstractions;
using System.Text.Json;
using Ardalis.GuardClauses;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Local filesystem implementation of file storage service.
/// Stores files in a configurable directory on the local filesystem.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRootPath;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly FileStorageOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LocalFileStorageService(
        IOptions<FileStorageOptions> options,
        IFileSystem fileSystem,
        ILogger<LocalFileStorageService> logger,
        IDateTimeProvider dateTimeProvider)
    {
        _options = options.Value;
        _fileSystem = fileSystem;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;

        // Resolve storage path (relative paths are resolved relative to current directory)
        _storageRootPath = Path.IsPathRooted(_options.Path)
            ? _options.Path
            : Path.Combine(Directory.GetCurrentDirectory(), _options.Path);

        // Ensure storage directory exists
        if (!_fileSystem.Directory.Exists(_storageRootPath))
        {
            _fileSystem.Directory.CreateDirectory(_storageRootPath);
            _logger.LogInformation("Created file storage directory: {StoragePath}", _storageRootPath);
        }
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.Null(fileStream, nameof(fileStream));
            Guard.Against.NullOrWhiteSpace(fileName, nameof(fileName));
            Guard.Against.NullOrWhiteSpace(contentType, nameof(contentType));

            // Validate file size
            if (fileStream.CanSeek && fileStream.Length > _options.MaxFileSizeBytes)
            {
                return Result<FileUploadResult>.Failure(
                    new Error("FileStorage.FileSizeExceeded", 
                        $"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / (1024 * 1024)}MB"));
            }

            // Validate content type
            if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            {
                return Result<FileUploadResult>.Failure(
                    new Error("FileStorage.ContentTypeNotAllowed", 
                        $"Content type '{contentType}' is not allowed. Allowed types: {string.Join(", ", _options.AllowedContentTypes)}"));
            }

            var fileId = Guid.NewGuid();
            var fileDirectory = Path.Combine(_storageRootPath, fileId.ToString());
            var filePath = Path.Combine(fileDirectory, "file.bin");

            // Create directory for this file
            _fileSystem.Directory.CreateDirectory(fileDirectory);

            // Write file
            await using var fileStreamWriter = _fileSystem.FileStream.New(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);

            var fileSize = _fileSystem.FileInfo.New(filePath).Length;
            
            // Validate file size after write (in case stream length wasn't available)
            if (fileSize > _options.MaxFileSizeBytes)
            {
                // Clean up the file
                _fileSystem.Directory.Delete(fileDirectory, recursive: true);
                return Result<FileUploadResult>.Failure(
                    new Error("FileStorage.FileSizeExceeded", 
                        $"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / (1024 * 1024)}MB"));
            }

            var uploadedAt = _dateTimeProvider.Now;

            // Store metadata (optional, for easier debugging)
            var metadataPath = Path.Combine(fileDirectory, "metadata.json");
            var metadata = new
            {
                OriginalFileName = fileName,
                ContentType = contentType,
                FileSize = fileSize,
                UploadedAt = uploadedAt
            };
            await _fileSystem.File.WriteAllTextAsync(
                metadataPath,
                JsonSerializer.Serialize(metadata),
                cancellationToken);

            _logger.LogInformation(
                "File uploaded successfully. FileId: {FileId}, FileName: {FileName}, Size: {FileSize} bytes",
                fileId, fileName, fileSize);

            return Result<FileUploadResult>.Success(new FileUploadResult
            {
                FileId = fileId,
                FileName = fileName,
                FileSize = fileSize,
                ContentType = contentType,
                UploadedAt = uploadedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return Result<FileUploadResult>.Failure(new Error("FileStorage.UploadFailed", $"Failed to upload file: {ex.Message}"));
        }
    }

    public async Task<Result<FileDownloadResult>> DownloadFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.Default(fileId, nameof(fileId));

            var fileDirectory = Path.Combine(_storageRootPath, fileId.ToString());
            var filePath = Path.Combine(fileDirectory, "file.bin");
            var metadataPath = Path.Combine(fileDirectory, "metadata.json");

            if (!_fileSystem.File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FileId}", fileId);
                return Result<FileDownloadResult>.Failure(Error.NotFound("File"));
            }

            // Read metadata if available
            string fileName = fileId.ToString();
            string contentType = "application/octet-stream";
            long fileSize = 0;

            if (_fileSystem.File.Exists(metadataPath))
            {
                try
                {
                    var metadataJson = await _fileSystem.File.ReadAllTextAsync(metadataPath, cancellationToken);
                    var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);
                    if (metadata.ValueKind == JsonValueKind.Object)
                    {
                        if (metadata.TryGetProperty("OriginalFileName", out var nameElement))
                            fileName = nameElement.GetString() ?? fileName;
                        if (metadata.TryGetProperty("ContentType", out var typeElement))
                            contentType = typeElement.GetString() ?? contentType;
                        if (metadata.TryGetProperty("FileSize", out var sizeElement))
                            fileSize = sizeElement.GetInt64();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read metadata for file: {FileId}", fileId);
                }
            }

            // If fileSize not from metadata, get from file
            if (fileSize == 0)
            {
                fileSize = _fileSystem.FileInfo.New(filePath).Length;
            }

            var fileStream = _fileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return Result<FileDownloadResult>.Success(new FileDownloadResult
            {
                FileStream = fileStream,
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileId}", fileId);
            return Result<FileDownloadResult>.Failure(new Error("FileStorage.DownloadFailed", $"Failed to download file: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.Default(fileId, nameof(fileId));

            var fileDirectory = Path.Combine(_storageRootPath, fileId.ToString());

            if (!_fileSystem.Directory.Exists(fileDirectory))
            {
                _logger.LogWarning("File directory not found: {FileId}", fileId);
                return Result.Success(); // Already deleted, consider success
            }

            // Delete entire directory (file + metadata)
            _fileSystem.Directory.Delete(fileDirectory, recursive: true);

            _logger.LogInformation("File deleted successfully: {FileId}", fileId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileId}", fileId);
            return Result.Failure(new Error("FileStorage.DeleteFailed", $"Failed to delete file: {ex.Message}"));
        }
    }
}
