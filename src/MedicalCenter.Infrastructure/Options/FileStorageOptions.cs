namespace MedicalCenter.Infrastructure.Options;

/// <summary>
/// Configuration options for file storage service.
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Root directory path for storing files.
    /// Default: "./attachments"
    /// </summary>
    public string Path { get; set; } = "./attachments";

    /// <summary>
    /// Maximum file size in bytes.
    /// Default: 10MB (10 * 1024 * 1024)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Allowed content types for file uploads.
    /// </summary>
    public List<string> AllowedContentTypes { get; set; } =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/jpg",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/msword", // .doc
        "application/vnd.ms-excel" // .xls
    ];

    /// <summary>
    /// Maximum number of attachments allowed per medical record.
    /// Default: 10
    /// </summary>
    public int MaxAttachmentsPerRecord { get; set; } = 10;
}
