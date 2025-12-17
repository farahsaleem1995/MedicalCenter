using MedicalCenter.Core.Aggregates.MedicalRecord;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class CreateRecordRequest
{
    public Guid PatientId { get; set; }
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<Guid>? AttachmentIds { get; set; }
}
