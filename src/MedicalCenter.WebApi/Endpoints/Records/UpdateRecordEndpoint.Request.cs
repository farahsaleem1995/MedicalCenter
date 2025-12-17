namespace MedicalCenter.WebApi.Endpoints.Records;

public class UpdateRecordRequest
{
    public Guid RecordId { get; set; } // FastEndpoints binds {recordId} to this property (case-insensitive)
    public string? Title { get; set; }
    public string? Content { get; set; }
}
