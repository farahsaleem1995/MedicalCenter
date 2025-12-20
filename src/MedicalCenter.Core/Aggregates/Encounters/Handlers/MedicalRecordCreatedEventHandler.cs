using MediatR;
using MedicalCenter.Core.Aggregates.MedicalRecords.Events;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.Encounters.Handlers;

/// <summary>
/// Handler for MedicalRecordCreatedEvent domain event.
/// Creates an Encounter when a MedicalRecord is created.
/// The handler generates a proper Reason description based on record type, title, and content.
/// </summary>
public class MedicalRecordCreatedEventHandler(
    IRepository<Encounter> encounterRepository) : INotificationHandler<MedicalRecordCreatedEvent>
{
    private readonly IRepository<Encounter> _encounterRepository = encounterRepository;

    public async Task Handle(MedicalRecordCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Create practitioner value object from event data
        var practitioner = ValueObjects.Practitioner.Create(
            notification.Record.Practitioner.FullName,
            notification.Record.Practitioner.Role);

        // Generate Reason description based on record type, title, and content
        string reason = GenerateReasonDescription(notification.Record.RecordType, notification.Record.Title, notification.Record.Content);

        // Create Encounter from event data
        // OccurredOn uses the event's OccurredOn timestamp (when the medical event occurred)
        var encounter = Encounter.Create(
            notification.Record.PatientId,
            practitioner,
            notification.OccurredOn,
            reason);

        await _encounterRepository.AddAsync(encounter, cancellationToken);
    }

    private static string GenerateReasonDescription(MedicalRecords.Enums.RecordType recordType, string title, string content)
    {
        // Build a descriptive reason that captures "what happened medically"
        string recordTypeDescription = GetRecordTypeDescription(recordType);
        
        // Combine record type, title, and a brief excerpt from content
        string reason = $"{recordTypeDescription}: {title}";
        
        // Add content excerpt if available (first 100 characters, truncated if longer)
        if (!string.IsNullOrWhiteSpace(content))
        {
            string contentExcerpt = content.Length > 100 ? $"{content[..100]}..." : content;
            reason += $" - {contentExcerpt}";
        }

        return reason;
    }

    private static string GetRecordTypeDescription(MedicalRecords.Enums.RecordType recordType)
    {
        return recordType switch
        {
            MedicalRecords.Enums.RecordType.ConsultationNote => "Consultation",
            MedicalRecords.Enums.RecordType.LaboratoryResult => "Laboratory Test",
            MedicalRecords.Enums.RecordType.ImagingReport => "Imaging Study",
            MedicalRecords.Enums.RecordType.Prescription => "Prescription",
            MedicalRecords.Enums.RecordType.Diagnosis => "Diagnosis",
            MedicalRecords.Enums.RecordType.TreatmentPlan => "Treatment Plan",
            MedicalRecords.Enums.RecordType.Other => "Medical Record",
            _ => "Medical Record"
        };
    }
}

