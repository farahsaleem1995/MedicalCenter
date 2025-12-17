using MedicalCenter.Core.Aggregates.MedicalRecord;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class GetSelfRecordsRequest
{
    public RecordType? RecordType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
