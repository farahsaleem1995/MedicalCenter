using FluentAssertions;
using MedicalCenter.Core.Aggregates;
using MedicalCenter.Core.Common;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class DoctorTests
{
    [Fact]
    public void Constructor_SetsDoctorProperties_WhenCreated()
    {
        // Arrange
        var fullName = "Dr. John Smith";
        var email = "john.smith@example.com";
        var licenseNumber = "MD123456";
        var specialty = "Cardiology";

        // Act
        var doctor = new Doctor(fullName, email, licenseNumber, specialty);

        // Assert
        doctor.FullName.Should().Be(fullName);
        doctor.Email.Should().Be(email);
        doctor.LicenseNumber.Should().Be(licenseNumber);
        doctor.Specialty.Should().Be(specialty);
    }

    [Fact]
    public void Constructor_SetsRoleToDoctor_WhenCreated()
    {
        // Arrange & Act
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");

        // Assert
        doctor.Role.Should().Be(UserRole.Doctor);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");

        // Assert
        doctor.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");

        // Assert
        doctor.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ReturnsDoctor_WithCorrectProperties()
    {
        // Arrange
        var fullName = "Dr. John Smith";
        var email = "john.smith@example.com";
        var licenseNumber = "MD123456";
        var specialty = "Cardiology";

        // Act
        var doctor = Doctor.Create(fullName, email, licenseNumber, specialty);

        // Assert
        doctor.Should().NotBeNull();
        doctor.FullName.Should().Be(fullName);
        doctor.Email.Should().Be(email);
        doctor.LicenseNumber.Should().Be(licenseNumber);
        doctor.Specialty.Should().Be(specialty);
        doctor.Role.Should().Be(UserRole.Doctor);
    }

    [Fact]
    public void Deactivate_DeactivatesDoctor_WhenCalled()
    {
        // Arrange
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");

        // Act
        doctor.Deactivate();

        // Assert
        doctor.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesDoctor_WhenCalled()
    {
        // Arrange
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");
        doctor.Deactivate();

        // Act
        doctor.Activate();

        // Assert
        doctor.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateSpecialty_UpdatesSpecialty_WhenValidSpecialtyProvided()
    {
        // Arrange
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");
        var newSpecialty = "Neurology";

        // Act
        doctor.UpdateSpecialty(newSpecialty);

        // Assert
        doctor.Specialty.Should().Be(newSpecialty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateSpecialty_ThrowsArgumentException_WhenSpecialtyIsNullOrWhiteSpace(string? invalidSpecialty)
    {
        // Arrange
        var doctor = new Doctor("Dr. John Smith", "john.smith@example.com", "MD123456", "Cardiology");

        // Act & Assert
        var act = () => doctor.UpdateSpecialty(invalidSpecialty!);
        act.Should().Throw<ArgumentException>();
    }
}

