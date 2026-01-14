using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Primitives.Pagination;

namespace MedicalCenter.WebApi.Endpoints.Practitioners;

public class ListSelfRecordsResponse
{
    public IReadOnlyList<PractitionerRecordSummaryDto> Items { get; set; } = Array.Empty<PractitionerRecordSummaryDto>();
    public PaginationMetadata Metadata { get; set; } = null!;
}

public class PractitionerRecordSummaryDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public PatientSummaryDto? Patient { get; set; }
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }

    public static PractitionerRecordSummaryDto FromMedicalRecord(MedicalRecord record)
    {
        return new PractitionerRecordSummaryDto
        {
            Id = record.Id,
            PatientId = record.PatientId,
            Patient = record.Patient != null ? new PatientSummaryDto
            {
                Id = record.Patient.Id,
                FullName = record.Patient.FullName,
                Email = record.Patient.Email
            } : null,
            RecordType = record.RecordType,
            Title = record.Title,
            CreatedAt = record.CreatedAt,
            AttachmentCount = record.Attachments.Count
        };
    }

    public class PatientSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}


