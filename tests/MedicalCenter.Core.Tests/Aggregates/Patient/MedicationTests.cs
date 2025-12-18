using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patients.Entities;
using Xunit;

namespace MedicalCenter.Core.Tests.Aggregates.Patient;

public class MedicationTests
{
    [Fact]
    public void Create_CreatesMedication_WithValidProperties()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var name = "Aspirin";
        var dosage = "100mg daily";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var notes = "For heart health";

        // Act
        var medication = Medication.Create(patientId, name, dosage, startDate, endDate, notes);

        // Assert
        medication.Should().NotBeNull();
        medication.PatientId.Should().Be(patientId);
        medication.Name.Should().Be(name);
        medication.Dosage.Should().Be(dosage);
        medication.StartDate.Should().Be(startDate);
        medication.EndDate.Should().Be(endDate);
        medication.Notes.Should().Be(notes);
        medication.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_CreatesMedication_WithoutEndDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);

        // Act
        var medication = Medication.Create(patientId, "Aspirin", "100mg", startDate);

        // Assert
        medication.EndDate.Should().BeNull();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);

        // Act & Assert
        var act = () => Medication.Create(patientId, string.Empty, "100mg", startDate);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var startDate = new DateTime(2024, 6, 1);
        var endDate = new DateTime(2024, 1, 1);

        // Act & Assert
        var act = () => Medication.Create(patientId, "Aspirin", "100mg", startDate, endDate);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_UpdatesMedicationProperties_WhenValidPropertiesProvided()
    {
        // Arrange
        var medication = Medication.Create(Guid.NewGuid(), "Aspirin", "100mg", new DateTime(2024, 1, 1));
        var newEndDate = new DateTime(2024, 12, 31);

        // Act
        medication.Update("200mg", newEndDate, "Updated notes");

        // Assert
        medication.Dosage.Should().Be("200mg");
        medication.EndDate.Should().Be(newEndDate);
        medication.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public void Update_ThrowsArgumentException_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 6, 1);
        var medication = Medication.Create(Guid.NewGuid(), "Aspirin", "100mg", startDate);
        var invalidEndDate = new DateTime(2024, 1, 1);

        // Act & Assert
        var act = () => medication.Update("200mg", invalidEndDate, "Notes");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

