using FluentAssertions;
using MedicalCenter.Core.Aggregates;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.SharedKernel;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class LaboratoryTests
{
    [Fact]
    public void Constructor_SetsLaboratoryProperties_WhenCreated()
    {
        // Arrange
        var fullName = "Lab Technician Bob";
        var email = "bob@lab.com";
        var labName = "City Lab Services";
        var licenseNumber = "LAB123456";

        // Act
        var laboratory = new Laboratory(Guid.NewGuid(), fullName, email, labName, licenseNumber);

        // Assert
        laboratory.FullName.Should().Be(fullName);
        laboratory.Email.Should().Be(email);
        laboratory.LabName.Should().Be(labName);
        laboratory.LicenseNumber.Should().Be(licenseNumber);
    }

    [Fact]
    public void Constructor_SetsRoleToLabUser_WhenCreated()
    {
        // Arrange & Act
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");

        // Assert
        laboratory.Role.Should().Be(UserRole.LabUser);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");

        // Assert
        laboratory.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");

        // Assert
        laboratory.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ReturnsLaboratory_WithCorrectProperties()
    {
        // Arrange
        var fullName = "Lab Technician Bob";
        var email = "bob@lab.com";
        var labName = "City Lab Services";
        var licenseNumber = "LAB123456";

        // Act
        var laboratory = Laboratory.Create(fullName, email, labName, licenseNumber);

        // Assert
        laboratory.Should().NotBeNull();
        laboratory.FullName.Should().Be(fullName);
        laboratory.Email.Should().Be(email);
        laboratory.LabName.Should().Be(labName);
        laboratory.LicenseNumber.Should().Be(licenseNumber);
        laboratory.Role.Should().Be(UserRole.LabUser);
    }

    [Fact]
    public void Deactivate_DeactivatesLaboratory_WhenCalled()
    {
        // Arrange
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");

        // Act
        laboratory.Deactivate();

        // Assert
        laboratory.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesLaboratory_WhenCalled()
    {
        // Arrange
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");
        laboratory.Deactivate();

        // Act
        laboratory.Activate();

        // Assert
        laboratory.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateLabName_UpdatesLabName_WhenValidLabNameProvided()
    {
        // Arrange
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");
        var newLabName = "Advanced Diagnostic Lab";

        // Act
        laboratory.UpdateLabName(newLabName);

        // Assert
        laboratory.LabName.Should().Be(newLabName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateLabName_ThrowsArgumentException_WhenLabNameIsNullOrWhiteSpace(string? invalidLabName)
    {
        // Arrange
        var laboratory = new Laboratory(Guid.NewGuid(), "Lab Technician Bob", "bob@lab.com", "City Lab Services", "LAB123456");

        // Act & Assert
        var act = () => laboratory.UpdateLabName(invalidLabName!);
        act.Should().Throw<ArgumentException>();
    }
}

