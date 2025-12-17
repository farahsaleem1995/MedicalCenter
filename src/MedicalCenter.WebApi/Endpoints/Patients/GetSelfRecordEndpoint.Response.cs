using MedicalCenter.Core.Enums;
using MedicalCenter.WebApi.Endpoints.Records;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class GetSelfRecordResponse
{
    public Guid Id { get; set; }
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PractitionerDto Practitioner { get; set; } = null!;
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
