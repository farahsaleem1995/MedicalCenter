using FluentAssertions;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.ValueObjects;
using Xunit;
using MedicalRecord = MedicalCenter.Core.Aggregates.MedicalRecord.MedicalRecord;

namespace MedicalCenter.Core.Tests.Aggregates.MedicalRecord;

using MedicalRecord = MedicalCenter.Core.Aggregates.MedicalRecord.MedicalRecord;

public class MedicalRecordTests
{
    private static Practitioner CreateTestPractitioner()
    {
        return Practitioner.Create(
            "Dr. Test Practitioner",
            "practitioner@test.com",
            UserRole.Doctor);
    }

    [Fact]
    public void Create_WithValidData_CreatesRecord()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var recordType = RecordType.ConsultationNote;
        var title = "Test Record";
        var content = "Test content";

        // Act
        var record = MedicalCenter.Core.Aggregates.MedicalRecord.MedicalRecord.Create(patientId, practitionerId, practitioner, recordType, title, content);

        // Assert
        record.Should().NotBeNull();
        record.PatientId.Should().Be(patientId);
        record.PractitionerId.Should().Be(practitionerId);
        record.Practitioner.Should().Be(practitioner);
        record.RecordType.Should().Be(recordType);
        record.Title.Should().Be(title);
        record.Content.Should().Be(content);
        record.IsActive.Should().BeTrue();
        record.Attachments.Should().BeEmpty();
    }

    [Fact]
    public void Update_ByPractitioner_UpdatesRecord()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Original Title",
            "Original Content");

        // Act
        record.Update(practitionerId, "Updated Title", "Updated Content");

        // Assert
        record.Title.Should().Be("Updated Title");
        record.Content.Should().Be("Updated Content");
    }

    [Fact]
    public void Update_ByNonPractitioner_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        // Act & Assert
        var act = () => record.Update(otherUserId, "New Title", null);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only the practitioner of the record can modify it.");
    }

    [Fact]
    public void Update_OnlyTitle_UpdatesOnlyTitle()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Original Title",
            "Original Content");

        // Act
        record.Update(practitionerId, "New Title", null);

        // Assert
        record.Title.Should().Be("New Title");
        record.Content.Should().Be("Original Content");
    }

    [Fact]
    public void Update_OnlyContent_UpdatesOnlyContent()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Original Title",
            "Original Content");

        // Act
        record.Update(practitionerId, null, "New Content");

        // Assert
        record.Title.Should().Be("Original Title");
        record.Content.Should().Be("New Content");
    }

    [Fact]
    public void Update_DeletedRecord_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        record.Delete(practitionerId);

        // Act & Assert
        var act = () => record.Update(practitionerId, "New Title", null);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot modify a deleted record.");
    }

    [Fact]
    public void AddAttachment_ByPractitioner_AddsAttachment()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var attachment = Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);

        // Act
        record.AddAttachment(practitionerId, attachment);

        // Assert
        record.Attachments.Should().ContainSingle();
        record.Attachments.First().Should().Be(attachment);
    }

    [Fact]
    public void AddAttachment_ByNonPractitioner_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var attachment = Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);

        // Act & Assert
        var act = () => record.AddAttachment(otherUserId, attachment);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only the practitioner of the record can add attachments.");
    }

    [Fact]
    public void AddAttachment_DeletedRecord_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        record.Delete(practitionerId);
        var attachment = Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);

        // Act & Assert
        var act = () => record.AddAttachment(practitionerId, attachment);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add attachments to a deleted record.");
    }

    [Fact]
    public void RemoveAttachment_ByPractitioner_RemovesAttachment()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var fileId = Guid.NewGuid();
        var attachment = Attachment.Create(
            fileId,
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);
        record.AddAttachment(practitionerId, attachment);

        // Act
        record.RemoveAttachment(practitionerId, fileId);

        // Assert
        record.Attachments.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAttachment_ByNonPractitioner_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var fileId = Guid.NewGuid();
        var attachment = Attachment.Create(
            fileId,
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);
        record.AddAttachment(practitionerId, attachment);

        // Act & Assert
        var act = () => record.RemoveAttachment(otherUserId, fileId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only the practitioner of the record can remove attachments.");
    }

    [Fact]
    public void RemoveAttachment_DeletedRecord_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var fileId = Guid.NewGuid();
        var attachment = Attachment.Create(
            fileId,
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);
        record.AddAttachment(practitionerId, attachment);
        record.Delete(practitionerId);

        // Act & Assert
        var act = () => record.RemoveAttachment(practitionerId, fileId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot remove attachments from a deleted record.");
    }

    [Fact]
    public void RemoveAttachment_AttachmentNotFound_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var nonExistentFileId = Guid.NewGuid();

        // Act & Assert
        var act = () => record.RemoveAttachment(practitionerId, nonExistentFileId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Attachment with fileId {nonExistentFileId} not found in this record.");
    }

    [Fact]
    public void RemoveAttachment_MultipleAttachments_RemovesCorrectOne()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var fileId1 = Guid.NewGuid();
        var fileId2 = Guid.NewGuid();
        var attachment1 = Attachment.Create(fileId1, "test1.pdf", 1024, "application/pdf", DateTime.UtcNow);
        var attachment2 = Attachment.Create(fileId2, "test2.pdf", 2048, "application/pdf", DateTime.UtcNow);
        record.AddAttachment(practitionerId, attachment1);
        record.AddAttachment(practitionerId, attachment2);

        // Act
        record.RemoveAttachment(practitionerId, fileId1);

        // Assert
        record.Attachments.Should().ContainSingle();
        record.Attachments.First().FileId.Should().Be(fileId2);
    }

    [Fact]
    public void Delete_ByPractitioner_SoftDeletesRecord()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        // Act
        record.Delete(practitionerId);

        // Assert
        record.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Delete_ByNonPractitioner_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        // Act & Assert
        var act = () => record.Delete(otherUserId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only the practitioner of the record can delete it.");
    }

    [Fact]
    public void Delete_AlreadyDeleted_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        record.Delete(practitionerId);

        // Act & Assert
        var act = () => record.Delete(practitionerId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Record is already deleted.");
    }

    [Fact]
    public void Create_WithEmptyPatientId_ThrowsException()
    {
        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.Empty,
            Guid.NewGuid(),
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyPractitionerId_ThrowsException()
    {
        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.NewGuid(),
            Guid.Empty,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsException()
    {
        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            practitioner,
            RecordType.ConsultationNote,
            string.Empty,
            "Content");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ThrowsException()
    {
        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            practitioner,
            RecordType.ConsultationNote,
            "   ",
            "Content");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyContent_ThrowsException()
    {
        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceContent_ThrowsException()
    {
        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidRecordType_ThrowsException()
    {
        // Arrange - Cast an integer that doesn't correspond to any enum value
        var invalidRecordType = (RecordType)999;

        // Act & Assert
        var practitioner = CreateTestPractitioner();
        var act = () => MedicalRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            practitioner,
            invalidRecordType,
            "Title",
            "Content");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Record type must be a valid enum value*");
    }

    [Fact]
    public void Update_WithWhitespaceTitle_DoesNotUpdate()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Original Title",
            "Original Content");

        // Act
        record.Update(practitionerId, "   ", "Updated Content");

        // Assert
        record.Title.Should().Be("Original Title");
        record.Content.Should().Be("Updated Content");
    }

    [Fact]
    public void Update_WithWhitespaceContent_DoesNotUpdate()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Original Title",
            "Original Content");

        // Act
        record.Update(practitionerId, "Updated Title", "   ");

        // Assert
        record.Title.Should().Be("Updated Title");
        record.Content.Should().Be("Original Content");
    }

    [Fact]
    public void Update_WithBothNullValues_DoesNotChangeAnything()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Original Title",
            "Original Content");

        // Act
        record.Update(practitionerId, null, null);

        // Assert
        record.Title.Should().Be("Original Title");
        record.Content.Should().Be("Original Content");
    }

    [Fact]
    public void AddAttachment_WithNullAttachment_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        // Act & Assert
        var act = () => record.AddAttachment(practitionerId, null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAttachment_MultipleAttachments_AddsAll()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");
        var attachment1 = Attachment.Create(
            Guid.NewGuid(),
            "test1.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);
        var attachment2 = Attachment.Create(
            Guid.NewGuid(),
            "test2.jpg",
            2048,
            "image/jpeg",
            DateTime.UtcNow);

        // Act
        record.AddAttachment(practitionerId, attachment1);
        record.AddAttachment(practitionerId, attachment2);

        // Assert
        record.Attachments.Should().HaveCount(2);
        record.Attachments.Should().Contain(attachment1);
        record.Attachments.Should().Contain(attachment2);
    }

    [Fact]
    public void Update_WithEmptyGuid_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        // Act & Assert
        var act = () => record.Update(Guid.Empty, "New Title", null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Delete_WithEmptyGuid_ThrowsException()
    {
        // Arrange
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var record = MedicalRecord.Create(
            Guid.NewGuid(),
            practitionerId,
            practitioner,
            RecordType.ConsultationNote,
            "Title",
            "Content");

        // Act & Assert
        var act = () => record.Delete(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(RecordType.ConsultationNote)]
    [InlineData(RecordType.LaboratoryResult)]
    [InlineData(RecordType.ImagingReport)]
    [InlineData(RecordType.Prescription)]
    [InlineData(RecordType.Diagnosis)]
    [InlineData(RecordType.TreatmentPlan)]
    [InlineData(RecordType.Other)]
    public void Create_WithAllRecordTypes_CreatesRecord(RecordType recordType)
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var practitionerId = Guid.NewGuid();
        var practitioner = CreateTestPractitioner();
        var title = "Test Record";
        var content = "Test content";

        // Act
        var record = MedicalRecord.Create(patientId, practitionerId, practitioner, recordType, title, content);

        // Assert
        record.Should().NotBeNull();
        record.RecordType.Should().Be(recordType);
    }
}
