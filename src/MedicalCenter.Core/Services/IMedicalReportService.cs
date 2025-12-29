namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for generating medical reports for patients.
/// Abstracts PDF generation implementation.
/// </summary>
public interface IMedicalReportService
{
    /// <summary>
    /// Generates a PDF medical report for a patient.
    /// Includes patient information, medical attributes, and medical records filtered by date range.
    /// </summary>
    /// <param name="patientId">The patient ID to generate the report for</param>
    /// <param name="dateFrom">Optional start date filter for medical records (inclusive)</param>
    /// <param name="dateTo">Optional end date filter for medical records (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file as a stream</returns>
    Task<MedicalReportResult> GenerateReportAsync(
        Guid patientId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a medical report generation operation.
/// </summary>
public class MedicalReportResult
{
    /// <summary>
    /// The PDF file content as a stream.
    /// </summary>
    public Stream PdfStream { get; init; } = null!;

    /// <summary>
    /// The suggested filename for the PDF report.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// The content type for the PDF file.
    /// </summary>
    public string ContentType { get; init; } = "application/pdf";
}

