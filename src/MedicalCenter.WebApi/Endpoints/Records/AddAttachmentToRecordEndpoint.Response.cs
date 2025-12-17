namespace MedicalCenter.WebApi.Endpoints.Records;

public class AddAttachmentToRecordResponse
{
    public Guid AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
