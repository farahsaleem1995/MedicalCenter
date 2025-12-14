using FluentAssertions;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class HealthcareEntityTests
{
    [Fact]
    public void Constructor_SetsHealthcareEntityProperties_WhenCreated()
    {
        // Arrange
        var fullName = "Nurse Jane Doe";
        var email = "jane.doe@hospital.com";
        var organizationName = "City General Hospital";
        var department = "Emergency Department";

        // Act
        var healthcareEntity = new HealthcareEntity(fullName, email, organizationName, department);

        // Assert
        healthcareEntity.FullName.Should().Be(fullName);
        healthcareEntity.Email.Should().Be(email);
        healthcareEntity.OrganizationName.Should().Be(organizationName);
        healthcareEntity.Department.Should().Be(department);
    }

    [Fact]
    public void Constructor_SetsRoleToHealthcareStaff_WhenCreated()
    {
        // Arrange & Act
        var healthcareEntity = new HealthcareEntity("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Assert
        healthcareEntity.Role.Should().Be(UserRole.HealthcareStaff);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var healthcareEntity = new HealthcareEntity("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Assert
        healthcareEntity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var healthcareEntity = new HealthcareEntity("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Assert
        healthcareEntity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ReturnsHealthcareEntity_WithCorrectProperties()
    {
        // Arrange
        var fullName = "Nurse Jane Doe";
        var email = "jane.doe@hospital.com";
        var organizationName = "City General Hospital";
        var department = "Emergency Department";

        // Act
        var healthcareEntity = HealthcareEntity.Create(fullName, email, organizationName, department);

        // Assert
        healthcareEntity.Should().NotBeNull();
        healthcareEntity.FullName.Should().Be(fullName);
        healthcareEntity.Email.Should().Be(email);
        healthcareEntity.OrganizationName.Should().Be(organizationName);
        healthcareEntity.Department.Should().Be(department);
        healthcareEntity.Role.Should().Be(UserRole.HealthcareStaff);
    }

    [Fact]
    public void Deactivate_DeactivatesHealthcareEntity_WhenCalled()
    {
        // Arrange
        var healthcareEntity = new HealthcareEntity("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Act
        healthcareEntity.Deactivate();

        // Assert
        healthcareEntity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesHealthcareEntity_WhenCalled()
    {
        // Arrange
        var healthcareEntity = new HealthcareEntity("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");
        healthcareEntity.Deactivate();

        // Act
        healthcareEntity.Activate();

        // Assert
        healthcareEntity.IsActive.Should().BeTrue();
    }
}

