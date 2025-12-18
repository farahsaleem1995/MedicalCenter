using FluentAssertions;
using MedicalCenter.Core.SharedKernel;
using Xunit;
using SystemAdminAggregate = MedicalCenter.Core.Aggregates.SystemAdmins.SystemAdmin;

namespace MedicalCenter.Core.Tests.Aggregates.SystemAdmin;

public class SystemAdminTests
{
    [Fact]
    public void Creates_SystemAdmin_WithValidInput()
    {
        // Arrange & Act
        var admin = SystemAdminAggregate.Create(
            "Admin User", 
            "admin@example.com", 
            "SYS-001", 
            "IT");

        // Assert
        admin.Should().NotBeNull();
        admin.FullName.Should().Be("Admin User");
        admin.Email.Should().Be("admin@example.com");
        admin.CorporateId.Should().Be("SYS-001");
        admin.Department.Should().Be("IT");
        admin.Role.Should().Be(UserRole.SystemAdmin);
        admin.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Throws_WhenCreating_WithNullOrWhiteSpaceFullName()
    {
        // Act & Assert
        var act = () => SystemAdminAggregate.Create("", "admin@example.com", "SYS-001", "IT");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throws_WhenCreating_WithNullOrWhiteSpaceEmail()
    {
        // Act & Assert
        var act = () => SystemAdminAggregate.Create("Admin User", "", "SYS-001", "IT");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throws_WhenCreating_WithNullOrWhiteSpaceCorporateId()
    {
        // Act & Assert
        var act = () => SystemAdminAggregate.Create("Admin User", "admin@example.com", "", "IT");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throws_WhenCreating_WithNullOrWhiteSpaceDepartment()
    {
        // Act & Assert
        var act = () => SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Can_UpdateCorporateId()
    {
        // Arrange
        var admin = SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "IT");

        // Act
        admin.UpdateCorporateId("SYS-002");

        // Assert
        admin.CorporateId.Should().Be("SYS-002");
    }

    [Fact]
    public void Throws_WhenUpdatingCorporateId_WithNullOrWhiteSpace()
    {
        // Arrange
        var admin = SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "IT");

        // Act & Assert
        var act = () => admin.UpdateCorporateId("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Can_UpdateDepartment()
    {
        // Arrange
        var admin = SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "IT");

        // Act
        admin.UpdateDepartment("Medical Administration");

        // Assert
        admin.Department.Should().Be("Medical Administration");
    }

    [Fact]
    public void Throws_WhenUpdatingDepartment_WithNullOrWhiteSpace()
    {
        // Arrange
        var admin = SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "IT");

        // Act & Assert
        var act = () => admin.UpdateDepartment("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Can_Deactivate_SystemAdmin()
    {
        // Arrange
        var admin = SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "IT");

        // Act
        admin.Deactivate();

        // Assert
        admin.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Can_Activate_SystemAdmin()
    {
        // Arrange
        var admin = SystemAdminAggregate.Create("Admin User", "admin@example.com", "SYS-001", "IT");
        admin.Deactivate();

        // Act
        admin.Activate();

        // Assert
        admin.IsActive.Should().BeTrue();
    }
}

