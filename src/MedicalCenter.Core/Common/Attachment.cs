using Ardalis.GuardClauses;

namespace MedicalCenter.Core.Common;

/// <summary>
/// Attachment value object representing a file attached to a medical record.
/// Attachments are immutable - once added to a record, they cannot be modified or removed.
/// </summary>
public class Attachment : ValueObject
{
    public Guid FileId { get; }
    public string FileName { get; } = string.Empty;
    public long FileSize { get; }
    public string ContentType { get; } = string.Empty;
    public DateTime UploadedAt { get; }

    private Attachment() { } // EF Core

    private Attachment(Guid fileId, string fileName, long fileSize, string contentType, DateTime uploadedAt)
    {
        FileId = fileId;
        FileName = fileName;
        FileSize = fileSize;
        ContentType = contentType;
        UploadedAt = uploadedAt;
    }

    public static Attachment Create(Guid fileId, string fileName, long fileSize, string contentType, DateTime uploadedAt)
    {
        Guard.Against.Default(fileId, nameof(fileId));
        Guard.Against.NullOrWhiteSpace(fileName, nameof(fileName));
        Guard.Against.NegativeOrZero(fileSize, nameof(fileSize));
        Guard.Against.NullOrWhiteSpace(contentType, nameof(contentType));
        Guard.Against.OutOfRange(uploadedAt, nameof(uploadedAt), DateTime.MinValue, DateTime.UtcNow);

        // File size validation is done at the service/endpoint level using configuration
        // This value object only ensures basic constraints (positive size)

        return new Attachment(fileId, fileName, fileSize, contentType, uploadedAt);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FileId;
        yield return FileName;
        yield return FileSize;
        yield return ContentType;
        yield return UploadedAt;
    }
}
