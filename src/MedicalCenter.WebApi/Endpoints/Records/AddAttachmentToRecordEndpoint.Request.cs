using Microsoft.AspNetCore.Http;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class AddAttachmentToRecordRequest
{
    public Guid RecordId { get; set; } // FastEndpoints binds {recordId} to this property (case-insensitive)
    public IFormFile? File { get; set; }
}
