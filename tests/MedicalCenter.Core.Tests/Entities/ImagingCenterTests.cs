using FluentAssertions;
using MedicalCenter.Core.Entities;
using MedicalCenter.Core.Enums;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class ImagingCenterTests
{
    [Fact]
    public void Constructor_SetsImagingCenterProperties_WhenCreated()
    {
        // Arrange
        var fullName = "Imaging Tech Alice";
        var email = "alice@imaging.com";
        var centerName = "City Imaging Center";
        var licenseNumber = "IMG123456";

        // Act
        var imagingCenter = new ImagingCenter(fullName, email, centerName, licenseNumber);

        // Assert
        imagingCenter.FullName.Should().Be(fullName);
        imagingCenter.Email.Should().Be(email);
        imagingCenter.CenterName.Should().Be(centerName);
        imagingCenter.LicenseNumber.Should().Be(licenseNumber);
    }

    [Fact]
    public void Constructor_SetsRoleToImagingUser_WhenCreated()
    {
        // Arrange & Act
        var imagingCenter = new ImagingCenter("Imaging Tech Alice", "alice@imaging.com", "City Imaging Center", "IMG123456");

        // Assert
        imagingCenter.Role.Should().Be(UserRole.ImagingUser);
    }

    [Fact]
    public void Constructor_SetsIsActiveToTrue_WhenCreated()
    {
        // Arrange & Act
        var imagingCenter = new ImagingCenter("Imaging Tech Alice", "alice@imaging.com", "City Imaging Center", "IMG123456");

        // Assert
        imagingCenter.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GeneratesId_WhenCreated()
    {
        // Arrange & Act
        var imagingCenter = new ImagingCenter("Imaging Tech Alice", "alice@imaging.com", "City Imaging Center", "IMG123456");

        // Assert
        imagingCenter.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ReturnsImagingCenter_WithCorrectProperties()
    {
        // Arrange
        var fullName = "Imaging Tech Alice";
        var email = "alice@imaging.com";
        var centerName = "City Imaging Center";
        var licenseNumber = "IMG123456";

        // Act
        var imagingCenter = ImagingCenter.Create(fullName, email, centerName, licenseNumber);

        // Assert
        imagingCenter.Should().NotBeNull();
        imagingCenter.FullName.Should().Be(fullName);
        imagingCenter.Email.Should().Be(email);
        imagingCenter.CenterName.Should().Be(centerName);
        imagingCenter.LicenseNumber.Should().Be(licenseNumber);
        imagingCenter.Role.Should().Be(UserRole.ImagingUser);
    }

    [Fact]
    public void Deactivate_DeactivatesImagingCenter_WhenCalled()
    {
        // Arrange
        var imagingCenter = new ImagingCenter("Imaging Tech Alice", "alice@imaging.com", "City Imaging Center", "IMG123456");

        // Act
        imagingCenter.Deactivate();

        // Assert
        imagingCenter.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesImagingCenter_WhenCalled()
    {
        // Arrange
        var imagingCenter = new ImagingCenter("Imaging Tech Alice", "alice@imaging.com", "City Imaging Center", "IMG123456");
        imagingCenter.Deactivate();

        // Act
        imagingCenter.Activate();

        // Assert
        imagingCenter.IsActive.Should().BeTrue();
    }
}

