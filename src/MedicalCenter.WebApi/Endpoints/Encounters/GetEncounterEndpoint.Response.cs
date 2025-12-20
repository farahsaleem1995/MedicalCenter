using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Endpoints.Encounters;

public class GetEncounterResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public PatientSummaryDto? Patient { get; set; }
    public PractitionerDto Practitioner { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
    public string Reason { get; set; } = string.Empty;

    public class PatientSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class PractitionerDto
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

