namespace MedicalCenter.WebApi.Endpoints.Patients;

public class ListPatientsRequest
{
    public string? SearchTerm { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}

