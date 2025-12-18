using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patients.Entities;
using Xunit;

namespace MedicalCenter.Core.Tests.Aggregates.Patient;

public class AllergyTests
{
    [Fact]
    public void Create_CreatesAllergy_WithValidName()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var name = "Peanuts";
        var severity = "Severe";
        var notes = "Causes anaphylaxis";

        // Act
        var allergy = Allergy.Create(patientId, name, severity, notes);

        // Assert
        allergy.Should().NotBeNull();
        allergy.PatientId.Should().Be(patientId);
        allergy.Name.Should().Be(name);
        allergy.Severity.Should().Be(severity);
        allergy.Notes.Should().Be(notes);
        allergy.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // Act & Assert
        var act = () => Allergy.Create(patientId, string.Empty, "Severe");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenNameIsWhitespace()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // Act & Assert
        var act = () => Allergy.Create(patientId, "   ", "Severe");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_UpdatesAllergyProperties_WhenValidPropertiesProvided()
    {
        // Arrange
        var allergy = Allergy.Create(Guid.NewGuid(), "Peanuts", "Mild", "Initial notes");

        // Act
        allergy.Update("Severe", "Updated notes");

        // Assert
        allergy.Severity.Should().Be("Severe");
        allergy.Notes.Should().Be("Updated notes");
    }
}

