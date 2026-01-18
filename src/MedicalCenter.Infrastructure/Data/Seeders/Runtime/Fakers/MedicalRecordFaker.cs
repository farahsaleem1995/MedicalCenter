using Bogus;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Enums;
using MedicalCenter.Core.Aggregates.MedicalRecords.ValueObjects;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configuration for MedicalRecord entities.
/// Note: This faker requires patient and practitioner information to be provided.
/// </summary>
public static class MedicalRecordFaker
{
    private static readonly Dictionary<RecordType, string[]> RecordTitles = new()
    {
        { RecordType.ConsultationNote, new[] { "Follow-up Consultation", "Initial Visit", "Routine Check-up", "Emergency Consultation", "Post-operative Follow-up" } },
        { RecordType.LaboratoryResult, new[] { "Complete Blood Count", "Lipid Panel", "Liver Function Test", "Thyroid Function Test", "Blood Glucose Test" } },
        { RecordType.ImagingReport, new[] { "Chest X-Ray Report", "MRI Brain Scan", "CT Abdominal Scan", "Ultrasound Report", "Bone Scan" } },
        { RecordType.Prescription, new[] { "Prescription for Antibiotics", "Pain Management Prescription", "Chronic Medication Refill", "Post-surgery Medication" } },
        { RecordType.Diagnosis, new[] { "Primary Diagnosis", "Differential Diagnosis", "Final Diagnosis", "Clinical Diagnosis" } },
        { RecordType.TreatmentPlan, new[] { "Treatment Plan", "Therapy Plan", "Rehabilitation Plan", "Long-term Care Plan" } },
        { RecordType.Other, new[] { "Medical Note", "Clinical Note", "Progress Note", "Discharge Summary" } }
    };

    public static MedicalRecord Create(
        Guid patientId,
        Guid practitionerId,
        Practitioner practitioner,
        RecordType recordType,
        Faker faker,
        DateTime? createdAt = null)
    {
        var titles = RecordTitles[recordType];
        var title = faker.PickRandom(titles);
        var content = GenerateMedicalContent(recordType, faker);
        
        var record = MedicalRecord.Create(patientId, practitionerId, practitioner, recordType, title, content);
        
        // Set CreatedAt if provided (for historical data)
        if (createdAt.HasValue)
        {
            // Use reflection to set CreatedAt since it's a setter property
            typeof(MedicalRecord).GetProperty(nameof(MedicalRecord.CreatedAt))?.SetValue(record, createdAt.Value);
        }
        
        return record;
    }

    private static string GenerateMedicalContent(RecordType recordType, Faker faker)
    {
        return recordType switch
        {
            RecordType.ConsultationNote => GenerateConsultationNote(faker),
            RecordType.LaboratoryResult => GenerateLaboratoryResult(faker),
            RecordType.ImagingReport => GenerateImagingReport(faker),
            RecordType.Prescription => GeneratePrescription(faker),
            RecordType.Diagnosis => GenerateDiagnosis(faker),
            RecordType.TreatmentPlan => GenerateTreatmentPlan(faker),
            _ => faker.Lorem.Paragraphs(faker.Random.Int(2, 4))
        };
    }

    private static string GenerateConsultationNote(Faker faker)
    {
        return $"Patient presented with {faker.Lorem.Sentence()}. " +
               $"Physical examination revealed {faker.Lorem.Sentence()}. " +
               $"Assessment: {faker.Lorem.Sentence()}. " +
               $"Plan: {faker.Lorem.Sentence()}.";
    }

    private static string GenerateLaboratoryResult(Faker faker)
    {
        return $"Test Results:\n" +
               $"- {faker.PickRandom(new[] { "Hemoglobin", "White Blood Count", "Platelet Count" })}: {faker.Random.Double(10, 20):F2}\n" +
               $"- {faker.PickRandom(new[] { "Glucose", "Cholesterol", "Triglycerides" })}: {faker.Random.Double(70, 200):F2}\n" +
               $"Reference ranges within normal limits. {faker.Lorem.Sentence()}";
    }

    private static string GenerateImagingReport(Faker faker)
    {
        return $"Imaging Study: {faker.PickRandom(new[] { "CT", "MRI", "X-Ray", "Ultrasound" })}\n" +
               $"Findings: {faker.Lorem.Sentence()}\n" +
               $"Impression: {faker.Lorem.Sentence()}\n" +
               $"Recommendation: {faker.Lorem.Sentence()}";
    }

    private static string GeneratePrescription(Faker faker)
    {
        return $"Prescribed Medication: {faker.PickRandom(new[] { "Metformin", "Lisinopril", "Atorvastatin" })}\n" +
               $"Dosage: {faker.PickRandom(new[] { "10mg daily", "5mg twice daily", "20mg once daily" })}\n" +
               $"Duration: {faker.Random.Int(7, 30)} days\n" +
               $"Instructions: {faker.Lorem.Sentence()}";
    }

    private static string GenerateDiagnosis(Faker faker)
    {
        return $"Primary Diagnosis: {faker.PickRandom(new[] { "Hypertension", "Type 2 Diabetes", "Asthma", "Arthritis" })}\n" +
               $"ICD-10 Code: {faker.Random.Replace("##.###")}\n" +
               $"Clinical Notes: {faker.Lorem.Paragraph()}";
    }

    private static string GenerateTreatmentPlan(Faker faker)
    {
        return $"Treatment Plan:\n" +
               $"1. {faker.Lorem.Sentence()}\n" +
               $"2. {faker.Lorem.Sentence()}\n" +
               $"3. {faker.Lorem.Sentence()}\n" +
               $"Follow-up: {faker.Date.Future().ToShortDateString()}";
    }
}

