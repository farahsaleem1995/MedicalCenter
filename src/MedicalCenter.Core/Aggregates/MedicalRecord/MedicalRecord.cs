using Ardalis.GuardClauses;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.ValueObjects;
using Patient = MedicalCenter.Core.Aggregates.Patient.Patient;

namespace MedicalCenter.Core.Aggregates.MedicalRecord;

/// <summary>
/// Medical record aggregate root.
/// Represents a medical record created by a provider for a patient.
/// Business rules:
/// - Only the practitioner can modify the record
/// - Only the practitioner can add or remove attachments
/// </summary>
public class MedicalRecord : BaseEntity, IAggregateRoot, IAuditableEntity
{
    public Guid PatientId { get; private set; }
    public MedicalCenter.Core.Aggregates.Patient.Patient? Patient { get; private set; } // Navigation property to Patient aggregate
    public Guid PractitionerId { get; private set; } // Practitioner who created the record
    public Practitioner Practitioner { get; private set; } = null!; // Practitioner value object (owned entity)
    public RecordType RecordType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Attachments collection - immutable once added
    private readonly List<Attachment> _attachments = new();
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();

    private MedicalRecord() { } // EF Core

    private MedicalRecord(Guid patientId, Guid practitionerId, Practitioner practitioner, RecordType recordType, string title, string content)
    {
        PatientId = patientId;
        PractitionerId = practitionerId;
        Practitioner = practitioner;
        RecordType = recordType;
        Title = title;
        Content = content;
        IsActive = true;
    }

    public static MedicalRecord Create(Guid patientId, Guid practitionerId, Practitioner practitioner, RecordType recordType, string title, string content)
    {
        Guard.Against.Default(patientId, nameof(patientId));
        Guard.Against.Default(practitionerId, nameof(practitionerId));
        Guard.Against.Null(practitioner, nameof(practitioner));
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.InvalidInput(recordType, nameof(recordType), rt => Enum.IsDefined(typeof(RecordType), rt), "Record type must be a valid enum value.");

        return new MedicalRecord(patientId, practitionerId, practitioner, recordType, title, content);
    }

    /// <summary>
    /// Updates the record. Only the practitioner can modify.
    /// </summary>
    public void Update(Guid modifierId, string? title, string? content)
    {
        Guard.Against.Default(modifierId, nameof(modifierId));

        if (modifierId != PractitionerId)
        {
            throw new InvalidOperationException("Only the practitioner of the record can modify it.");
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot modify a deleted record.");
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        if (!string.IsNullOrWhiteSpace(content))
        {
            Content = content;
        }
    }

    /// <summary>
    /// Adds an attachment to the record. Only the practitioner can add attachments.
    /// </summary>
    public void AddAttachment(Guid adderId, Attachment attachment)
    {
        Guard.Against.Default(adderId, nameof(adderId));
        Guard.Against.Null(attachment, nameof(attachment));

        if (adderId != PractitionerId)
        {
            throw new InvalidOperationException("Only the practitioner of the record can add attachments.");
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot add attachments to a deleted record.");
        }

        _attachments.Add(attachment);
    }

    /// <summary>
    /// Removes an attachment from the record by fileId. Only the practitioner can remove attachments.
    /// </summary>
    public void RemoveAttachment(Guid removerId, Guid fileId)
    {
        Guard.Against.Default(removerId, nameof(removerId));
        Guard.Against.Default(fileId, nameof(fileId));

        if (removerId != PractitionerId)
        {
            throw new InvalidOperationException("Only the practitioner of the record can remove attachments.");
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot remove attachments from a deleted record.");
        }

        var attachment = _attachments.FirstOrDefault(a => a.FileId == fileId);
        if (attachment == null)
        {
            throw new InvalidOperationException($"Attachment with fileId {fileId} not found in this record.");
        }

        _attachments.Remove(attachment);
    }

    /// <summary>
    /// Soft deletes the record. Only the practitioner can delete.
    /// </summary>
    public void Delete(Guid deleterId)
    {
        Guard.Against.Default(deleterId, nameof(deleterId));

        if (deleterId != PractitionerId)
        {
            throw new InvalidOperationException("Only the practitioner of the record can delete it.");
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Record is already deleted.");
        }

        IsActive = false;
    }
}
