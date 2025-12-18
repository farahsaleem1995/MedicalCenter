using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patients.Entities;
using Xunit;

namespace MedicalCenter.Core.Tests.Aggregates.Patient;

public class SurgeryTests
{
    [Fact]
    public void Create_CreatesSurgery_WithValidProperties()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var name = "Appendectomy";
        var date = new DateTime(2023, 6, 15);
        var surgeon = "Dr. Smith";
        var notes = "Successful procedure";

        // Act
        var surgery = Surgery.Create(patientId, name, date, surgeon, notes);

        // Assert
        surgery.Should().NotBeNull();
        surgery.PatientId.Should().Be(patientId);
        surgery.Name.Should().Be(name);
        surgery.Date.Should().Be(date);
        surgery.Surgeon.Should().Be(surgeon);
        surgery.Notes.Should().Be(notes);
        surgery.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var date = DateTime.UtcNow.AddDays(-100);

        // Act & Assert
        var act = () => Surgery.Create(patientId, string.Empty, date);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenDateIsInFuture()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var act = () => Surgery.Create(patientId, "Appendectomy", futureDate);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_UpdatesSurgeryProperties_WhenValidPropertiesProvided()
    {
        // Arrange
        var surgery = Surgery.Create(Guid.NewGuid(), "Appendectomy", DateTime.UtcNow.AddDays(-100), "Dr. Smith", "Initial notes");

        // Act
        surgery.Update("Dr. Jones", "Updated notes");

        // Assert
        surgery.Surgeon.Should().Be("Dr. Jones");
        surgery.Notes.Should().Be("Updated notes");
    }
}

