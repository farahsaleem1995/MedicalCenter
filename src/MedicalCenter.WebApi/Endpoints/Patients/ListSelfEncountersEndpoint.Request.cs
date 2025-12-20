namespace MedicalCenter.WebApi.Endpoints.Patients;

public class ListSelfEncountersRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}

