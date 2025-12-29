namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// Request DTO for generating a medical report.
/// </summary>
public class GenerateReportRequest
{
    /// <summary>
    /// Optional start date filter for medical records (inclusive).
    /// Only records created on or after this date will be included.
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Optional end date filter for medical records (inclusive).
    /// Only records created on or before this date will be included.
    /// </summary>
    public DateTime? DateTo { get; set; }
}

