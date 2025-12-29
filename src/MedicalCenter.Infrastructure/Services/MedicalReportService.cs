using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IMedicalReportService for generating PDF medical reports.
/// Uses QuestPDF library for PDF generation.
/// </summary>
public class MedicalReportService(
    MedicalCenterDbContext dbContext,
    IRepository<Patient> patientRepository) : IMedicalReportService
{
    public async Task<MedicalReportResult> GenerateReportAsync(
        Guid patientId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        // Load patient with all medical attributes
        var specification = new PatientByIdSpecification(patientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");
        }

        // Load medical records filtered by date range
        IQueryable<MedicalRecord> recordsQuery = dbContext.Set<MedicalRecord>()
            .Include(mr => mr.Practitioner)
            .Where(mr => mr.PatientId == patientId && mr.IsActive);

        if (dateFrom.HasValue)
        {
            recordsQuery = recordsQuery.Where(mr => mr.CreatedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            recordsQuery = recordsQuery.Where(mr => mr.CreatedAt <= dateTo.Value);
        }

        var records = await recordsQuery
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync(cancellationToken);

        // Generate PDF
        var pdfStream = new MemoryStream();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header with branding in top left corner
                page.Header()
                    .Row(row =>
                    {
                        row.RelativeItem().Element(GenerateBranding);
                        row.RelativeItem().AlignRight().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingTop(20)
                    .Column(column =>
                    {
                        column.Item().Element(x => GenerateReportTitle(x));
                        column.Item().PaddingVertical(10);
                        column.Item().Element(x => GeneratePatientInfo(x, patient));
                        column.Item().PaddingVertical(10);
                        column.Item().Element(x => GenerateMedicalAttributes(x, patient));
                        column.Item().PaddingVertical(10);
                        column.Item().Element(x => GenerateMedicalRecords(x, records));
                        column.Item().PaddingVertical(10);
                        column.Item().Element(GenerateFooter);
                    });
            });
        });

        document.GeneratePdf(pdfStream);
        pdfStream.Position = 0;

        string fileName = $"MedicalReport_{patient.FullName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        return new MedicalReportResult
        {
            PdfStream = pdfStream,
            FileName = fileName,
            ContentType = "application/pdf"
        };
    }

    private static void GenerateBranding(IContainer container)
    {
        container
            .Column(column =>
            {
                column.Item().Text("Medical Center")
                    .FontSize(16)
                    .FontColor(Colors.Blue.Darken2)
                    .Bold();
                column.Item().Text("Healthcare Excellence")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1)
                    .Italic();
            });
    }

    private static void GenerateReportTitle(IContainer container)
    {
        container
            .Background(Colors.Blue.Medium)
            .Padding(15)
            .Text("Patient Medical Report")
            .FontSize(20)
            .FontColor(Colors.White)
            .Bold()
            .AlignCenter();
    }

    private static void GeneratePatientInfo(IContainer container, Patient patient)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text("Patient Information").FontSize(14).Bold();
                column.Item().PaddingVertical(5);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Full Name:").FontSize(10);
                    row.RelativeItem(2).Text(patient.FullName).FontSize(10);
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Email:").FontSize(10);
                    row.RelativeItem(2).Text(patient.Email).FontSize(10);
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("National ID:").FontSize(10);
                    row.RelativeItem(2).Text(patient.NationalId).FontSize(10);
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Date of Birth:").FontSize(10);
                    row.RelativeItem(2).Text(patient.DateOfBirth.ToString("yyyy-MM-dd")).FontSize(10);
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Blood Type:").FontSize(10);
                    row.RelativeItem(2).Text(patient.BloodType?.ToString() ?? "Not specified").FontSize(10);
                });
            });
    }

    private static void GenerateMedicalAttributes(IContainer container, Patient patient)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text("Medical Attributes").FontSize(14).Bold();
                column.Item().PaddingVertical(5);

                // Allergies
                column.Item().PaddingTop(5).Text("Allergies").FontSize(12).Bold();
                if (patient.Allergies.Any())
                {
                    foreach (var allergy in patient.Allergies)
                    {
                        column.Item().PaddingLeft(10).Text($"{allergy.Name}")
                            .FontSize(10);
                        if (!string.IsNullOrWhiteSpace(allergy.Severity))
                        {
                            column.Item().PaddingLeft(15).Text($"Severity: {allergy.Severity}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                        if (!string.IsNullOrWhiteSpace(allergy.Notes))
                        {
                            column.Item().PaddingLeft(15).Text($"Notes: {allergy.Notes}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    }
                }
                else
                {
                    column.Item().PaddingLeft(10).Text("None recorded").FontSize(10).FontColor(Colors.Grey.Medium);
                }

                // Chronic Diseases
                column.Item().PaddingTop(5).Text("Chronic Diseases").FontSize(12).Bold();
                if (patient.ChronicDiseases.Any())
                {
                    foreach (var disease in patient.ChronicDiseases)
                    {
                        column.Item().PaddingLeft(10).Text($"{disease.Name} (Diagnosed: {disease.DiagnosisDate:yyyy-MM-dd})")
                            .FontSize(10);
                        if (!string.IsNullOrWhiteSpace(disease.Notes))
                        {
                            column.Item().PaddingLeft(15).Text($"Notes: {disease.Notes}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    }
                }
                else
                {
                    column.Item().PaddingLeft(10).Text("None recorded").FontSize(10).FontColor(Colors.Grey.Medium);
                }

                // Medications
                column.Item().PaddingTop(5).Text("Medications").FontSize(12).Bold();
                if (patient.Medications.Any())
                {
                    foreach (var medication in patient.Medications)
                    {
                        string dateRange = medication.EndDate.HasValue
                            ? $"{medication.StartDate:yyyy-MM-dd} to {medication.EndDate.Value:yyyy-MM-dd}"
                            : $"Since {medication.StartDate:yyyy-MM-dd}";
                        column.Item().PaddingLeft(10).Text($"{medication.Name} - {dateRange}")
                            .FontSize(10);
                        if (!string.IsNullOrWhiteSpace(medication.Dosage))
                        {
                            column.Item().PaddingLeft(15).Text($"Dosage: {medication.Dosage}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                        if (!string.IsNullOrWhiteSpace(medication.Notes))
                        {
                            column.Item().PaddingLeft(15).Text($"Notes: {medication.Notes}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    }
                }
                else
                {
                    column.Item().PaddingLeft(10).Text("None recorded").FontSize(10).FontColor(Colors.Grey.Medium);
                }

                // Surgeries
                column.Item().PaddingTop(5).Text("Surgeries").FontSize(12).Bold();
                if (patient.Surgeries.Any())
                {
                    foreach (var surgery in patient.Surgeries)
                    {
                        column.Item().PaddingLeft(10).Text($"{surgery.Name} - {surgery.Date:yyyy-MM-dd}")
                            .FontSize(10);
                        if (!string.IsNullOrWhiteSpace(surgery.Surgeon))
                        {
                            column.Item().PaddingLeft(15).Text($"Surgeon: {surgery.Surgeon}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                        if (!string.IsNullOrWhiteSpace(surgery.Notes))
                        {
                            column.Item().PaddingLeft(15).Text($"Notes: {surgery.Notes}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    }
                }
                else
                {
                    column.Item().PaddingLeft(10).Text("None recorded").FontSize(10).FontColor(Colors.Grey.Medium);
                }
            });
    }

    private static void GenerateMedicalRecords(IContainer container, List<MedicalRecord> records)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text("Medical Records").FontSize(14).Bold();
                column.Item().PaddingVertical(5);

                if (records.Any())
                {
                    foreach (var record in records)
                    {
                        column.Item().Element(x => GenerateRecordSection(x, record));
                        column.Item().PaddingVertical(5);
                    }
                }
                else
                {
                    column.Item().Text("No medical records found for the specified date range.")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                }
            });
    }

    private static void GenerateRecordSection(IContainer container, MedicalRecord record)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(8)
            .Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(GetRecordTypeName(record.RecordType))
                        .FontSize(11)
                        .Bold();
                    row.AutoItem().Text(record.CreatedAt.ToString("yyyy-MM-dd"))
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
                column.Item().PaddingTop(3).Text(record.Title).FontSize(10).Bold();
                column.Item().PaddingTop(3).Text(record.Content).FontSize(9);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Practitioner: {record.Practitioner.FullName}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);
                    row.AutoItem().Text($"Attachments: {record.Attachments.Count}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
    }

    private static void GenerateFooter(IContainer container)
    {
        container
            .AlignCenter()
            .Text("This is a confidential medical document. Unauthorized access is prohibited.")
            .FontSize(8)
            .FontColor(Colors.Grey.Medium)
            .Italic();
    }

    private static string GetRecordTypeName(RecordType recordType)
    {
        return recordType switch
        {
            RecordType.ConsultationNote => "Consultation Note",
            RecordType.LaboratoryResult => "Laboratory Result",
            RecordType.ImagingReport => "Imaging Report",
            RecordType.Prescription => "Prescription",
            RecordType.Diagnosis => "Diagnosis",
            RecordType.TreatmentPlan => "Treatment Plan",
            RecordType.Other => "Other",
            _ => recordType.ToString()
        };
    }
}

