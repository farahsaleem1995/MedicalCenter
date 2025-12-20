using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Endpoints.Patients;

public class GetSelfEncounterResponse
{
    public Guid Id { get; set; }
    public PractitionerDto Practitioner { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
    public string Reason { get; set; } = string.Empty;

    public class PractitionerDto
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

