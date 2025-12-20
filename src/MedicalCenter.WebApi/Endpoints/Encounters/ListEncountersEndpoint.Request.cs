namespace MedicalCenter.WebApi.Endpoints.Encounters;

public class ListEncountersRequest
{
    public Guid? PatientId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}

