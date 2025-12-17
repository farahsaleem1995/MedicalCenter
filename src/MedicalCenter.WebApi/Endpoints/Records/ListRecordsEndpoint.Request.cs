using MedicalCenter.Core.Aggregates.MedicalRecord;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class ListRecordsRequest
{
    public Guid? PractitionerId { get; set; }
    public Guid? PatientId { get; set; }
    public RecordType? RecordType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}
