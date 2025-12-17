using MedicalCenter.Core.Enums;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class GetSelfRecordsResponse
{
    public List<PatientRecordSummaryDto> Records { get; set; } = new();
}

public class PatientRecordSummaryDto
{
    public Guid Id { get; set; }
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }
}
