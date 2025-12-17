using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Common;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class UpdateRecordResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid PractitionerId { get; set; }
    public PractitionerDto Practitioner { get; set; } = null!;
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<AttachmentDto> Attachments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public class PractitionerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
