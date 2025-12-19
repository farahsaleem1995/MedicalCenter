using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.SharedKernel;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class PatientTests
{
    [Fact]
    public void Constructor_SetsPatientProperties_WhenCreated()
    {
        // Arrange
        var fullName = "Jane Smith";
        var email = "jane.smith@example.com";
        var nationalId = "1234567890";
        var dateOfBirth = new DateTime(1990, 1, 15);

        // Act
        var patient = new Patient(Guid.NewGuid(), fullName, email, nationalId, dateOfBirth);

        // Assert
        patient.FullName.Should().Be(fullName);
        patient.Email.Should().Be(email);
        patient.NationalId.Should().Be(nationalId);
        patient.DateOfBirth.Should().Be(dateOfBirth);
    }

    [Fact]
    public void Constructor_SetsRoleToPatient_WhenCreated()
    {
        // Arrange & Act
        var patient = new Patient(Guid.NewGuid(), "Jane Smith", "jane.smith@example.com", "1234567890", new DateTime(1990, 1, 15));

        // Assert
        patient.Role.Should().Be(UserRole.Patient);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var patient = new Patient(Guid.NewGuid(), "Jane Smith", "jane.smith@example.com", "1234567890", new DateTime(1990, 1, 15));

        // Assert
        patient.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var patient = new Patient(Guid.NewGuid(), "Jane Smith", "jane.smith@example.com", "1234567890", new DateTime(1990, 1, 15));

        // Assert
        patient.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ReturnsPatient_WithCorrectProperties()
    {
        // Arrange
        var fullName = "Jane Smith";
        var email = "jane.smith@example.com";
        var nationalId = "1234567890";
        var dateOfBirth = new DateTime(1990, 1, 15);

        // Act
        var patient = Patient.Create(fullName, email, nationalId, dateOfBirth);

        // Assert
        patient.Should().NotBeNull();
        patient.FullName.Should().Be(fullName);
        patient.Email.Should().Be(email);
        patient.NationalId.Should().Be(nationalId);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.Role.Should().Be(UserRole.Patient);
    }

    [Fact]
    public void Deactivate_DeactivatesPatient_WhenCalled()
    {
        // Arrange
        var patient = new Patient(Guid.NewGuid(), "Jane Smith", "jane.smith@example.com", "1234567890", new DateTime(1990, 1, 15));

        // Act
        patient.Deactivate();

        // Assert
        patient.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesPatient_WhenCalled()
    {
        // Arrange
        var patient = new Patient(Guid.NewGuid(), "Jane Smith", "jane.smith@example.com", "1234567890", new DateTime(1990, 1, 15));
        patient.Deactivate();

        // Act
        patient.Activate();

        // Assert
        patient.IsActive.Should().BeTrue();
    }
}

