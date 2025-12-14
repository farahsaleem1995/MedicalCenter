using FluentAssertions;
using MedicalCenter.Core.Common;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class UserTests
{
    private class TestUser : User
    {
        public TestUser(string fullName, string email, UserRole role)
            : base(fullName, email, role)
        {
        }
    }

    [Fact]
    public void Constructor_SetsProperties_WhenCreated()
    {
        // Arrange
        var fullName = "John Doe";
        var email = "john.doe@example.com";
        var role = UserRole.Patient;

        // Act
        var user = new TestUser(fullName, email, role);

        // Assert
        user.FullName.Should().Be(fullName);
        user.Email.Should().Be(email);
        user.Role.Should().Be(role);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var user = new TestUser("John Doe", "john.doe@example.com", UserRole.Patient);

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var user = new TestUser("John Doe", "john.doe@example.com", UserRole.Patient);

        // Assert
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse_WhenCalled()
    {
        // Arrange
        var user = new TestUser("John Doe", "john.doe@example.com", UserRole.Patient);
        user.IsActive.Should().BeTrue(); // Verify initial state

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue_WhenCalled()
    {
        // Arrange
        var user = new TestUser("John Doe", "john.doe@example.com", UserRole.Patient);
        user.Deactivate();
        user.IsActive.Should().BeFalse(); // Verify initial state

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_CanReactivateDeactivatedUser_WhenCalled()
    {
        // Arrange
        var user = new TestUser("John Doe", "john.doe@example.com", UserRole.Patient);
        user.Deactivate();
        user.Deactivate(); // Multiple deactivations

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(UserRole.SystemAdmin)]
    [InlineData(UserRole.Patient)]
    [InlineData(UserRole.Doctor)]
    [InlineData(UserRole.HealthcareStaff)]
    [InlineData(UserRole.LabUser)]
    [InlineData(UserRole.ImagingUser)]
    public void Constructor_SetsRoleCorrectly_ForAllUserRoles(UserRole role)
    {
        // Arrange & Act
        var user = new TestUser("Test User", "test@example.com", role);

        // Assert
        user.Role.Should().Be(role);
    }
}

