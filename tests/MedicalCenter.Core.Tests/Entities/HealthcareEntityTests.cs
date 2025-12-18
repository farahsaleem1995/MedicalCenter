using FluentAssertions;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.SharedKernel;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class HealthcareStaffTests
{
    [Fact]
    public void Constructor_SetsHealthcareStaffProperties_WhenCreated()
    {
        // Arrange
        var fullName = "Nurse Jane Doe";
        var email = "jane.doe@hospital.com";
        var organizationName = "City General Hospital";
        var department = "Emergency Department";

        // Act
        var healthcareEntity = new HealthcareStaff(fullName, email, organizationName, department);

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
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Assert
        healthcareEntity.Role.Should().Be(UserRole.HealthcareStaff);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Assert
        healthcareEntity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Assert
        healthcareEntity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ReturnsHealthcareStaff_WithCorrectProperties()
    {
        // Arrange
        var fullName = "Nurse Jane Doe";
        var email = "jane.doe@hospital.com";
        var organizationName = "City General Hospital";
        var department = "Emergency Department";

        // Act
        var healthcareEntity = HealthcareStaff.Create(fullName, email, organizationName, department);

        // Assert
        healthcareEntity.Should().NotBeNull();
        healthcareEntity.FullName.Should().Be(fullName);
        healthcareEntity.Email.Should().Be(email);
        healthcareEntity.OrganizationName.Should().Be(organizationName);
        healthcareEntity.Department.Should().Be(department);
        healthcareEntity.Role.Should().Be(UserRole.HealthcareStaff);
    }

    [Fact]
    public void Deactivate_DeactivatesHealthcareStaff_WhenCalled()
    {
        // Arrange
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Act
        healthcareEntity.Deactivate();

        // Assert
        healthcareEntity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesHealthcareStaff_WhenCalled()
    {
        // Arrange
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");
        healthcareEntity.Deactivate();

        // Act
        healthcareEntity.Activate();

        // Assert
        healthcareEntity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateOrganization_UpdatesOrganizationAndDepartment_WhenValidValuesProvided()
    {
        // Arrange
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");
        var newOrganizationName = "Regional Medical Center";
        var newDepartment = "Intensive Care Unit";

        // Act
        healthcareEntity.UpdateOrganization(newOrganizationName, newDepartment);

        // Assert
        healthcareEntity.OrganizationName.Should().Be(newOrganizationName);
        healthcareEntity.Department.Should().Be(newDepartment);
    }

    [Theory]
    [InlineData(null, "Department")]
    [InlineData("", "Department")]
    [InlineData("   ", "Department")]
    [InlineData("Organization", null)]
    [InlineData("Organization", "")]
    [InlineData("Organization", "   ")]
    public void UpdateOrganization_ThrowsArgumentException_WhenValuesAreNullOrWhiteSpace(string? invalidOrganization, string? invalidDepartment)
    {
        // Arrange
        var healthcareEntity = new HealthcareStaff("Nurse Jane Doe", "jane.doe@hospital.com", "City General Hospital", "Emergency Department");

        // Act & Assert
        var act = () => healthcareEntity.UpdateOrganization(invalidOrganization!, invalidDepartment!);
        act.Should().Throw<ArgumentException>();
    }
}

