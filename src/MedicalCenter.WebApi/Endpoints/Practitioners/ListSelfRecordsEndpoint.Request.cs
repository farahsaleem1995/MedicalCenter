using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;

namespace MedicalCenter.WebApi.Endpoints.Practitioners;

public class ListSelfRecordsRequest
{
    public Guid? PatientId { get; set; }
    public RecordType? RecordType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}


