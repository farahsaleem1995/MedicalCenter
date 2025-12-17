namespace MedicalCenter.WebApi.Endpoints.Records;

public class RemoveAttachmentFromRecordRequest
{
    public Guid RecordId { get; set; } // FastEndpoints binds {recordId} to this property (case-insensitive)
    public Guid AttachmentId { get; set; } // FastEndpoints binds {attachmentId} to this property (case-insensitive)
}
