using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class GetSelfRecordsResponse
{
    public IReadOnlyCollection<PatientRecordSummaryDto> Records { get; set; } = [];
    public PaginationMetadata Metadata { get; set; } = null!;
}

public class PatientRecordSummaryDto
{
    public Guid Id { get; set; }
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }

    public static PatientRecordSummaryDto FromMedicalRecord(MedicalRecord record)
    {
        return new PatientRecordSummaryDto
        {
            Id = record.Id,
            RecordType = record.RecordType,
            Title = record.Title,
            CreatedAt = record.CreatedAt,
            AttachmentCount = record.Attachments.Count
        };
    }
}
