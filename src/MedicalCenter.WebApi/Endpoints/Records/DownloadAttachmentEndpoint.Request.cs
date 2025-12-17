namespace MedicalCenter.WebApi.Endpoints.Records;

public class DownloadAttachmentRequest
{
    public Guid RecordId { get; set; }
    public Guid AttachmentId { get; set; }
}
