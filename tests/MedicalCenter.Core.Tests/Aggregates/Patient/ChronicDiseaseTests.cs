using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patients.Entities;
using Xunit;

namespace MedicalCenter.Core.Tests.Aggregates.Patient;

public class ChronicDiseaseTests
{
    [Fact]
    public void Create_CreatesChronicDisease_WithValidProperties()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var name = "Diabetes";
        var diagnosisDate = new DateTime(2020, 1, 1);
        var notes = "Type 2";

        // Act
        var chronicDisease = ChronicDisease.Create(patientId, name, diagnosisDate, notes);

        // Assert
        chronicDisease.Should().NotBeNull();
        chronicDisease.PatientId.Should().Be(patientId);
        chronicDisease.Name.Should().Be(name);
        chronicDisease.DiagnosisDate.Should().Be(diagnosisDate);
        chronicDisease.Notes.Should().Be(notes);
        chronicDisease.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var diagnosisDate = DateTime.UtcNow.AddDays(-100);

        // Act & Assert
        var act = () => ChronicDisease.Create(patientId, string.Empty, diagnosisDate);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenDiagnosisDateIsInFuture()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var act = () => ChronicDisease.Create(patientId, "Diabetes", futureDate);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_UpdatesChronicDiseaseNotes_WhenValidNotesProvided()
    {
        // Arrange
        var chronicDisease = ChronicDisease.Create(Guid.NewGuid(), "Diabetes", DateTime.UtcNow.AddDays(-100), "Initial notes");

        // Act
        chronicDisease.Update("Updated notes");

        // Assert
        chronicDisease.Notes.Should().Be("Updated notes");
    }
}

