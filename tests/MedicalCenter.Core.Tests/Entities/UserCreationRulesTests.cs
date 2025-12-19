using FluentAssertions;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.SharedKernel;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

/// <summary>
/// Domain tests for user creation business rules.
/// Following classical school: testing business rules, not implementation details.
/// </summary>
public class UserCreationRulesTests
{
    [Fact]
    public void Creates_Patient_WithCorrectRole()
    {
        // Arrange & Act
        var patient = new Patient(Guid.NewGuid(), "John Doe", "john.doe@example.com", "123456789", new DateTime(1990, 1, 15));

        // Assert - Domain rule: Patient has Patient role
        patient.Role.Should().Be(UserRole.Patient);
        patient.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Creates_Doctor_WithCorrectRole()
    {
        // Arrange & Act
        var doctor = Doctor.Create("Dr. Smith", "dr.smith@example.com", "LIC123", "Cardiology");

        // Assert - Domain rule: Doctor has Doctor role
        doctor.Role.Should().Be(UserRole.Doctor);
        doctor.Specialty.Should().Be("Cardiology");
        doctor.LicenseNumber.Should().Be("LIC123");
        doctor.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Creates_HealthcareStaff_WithCorrectRole()
    {
        // Arrange & Act
        var healthcare = HealthcareStaff.Create("Jane Nurse", "jane@hospital.com", "City Hospital", "Emergency");

        // Assert - Domain rule: HealthcareStaff has HealthcareStaff role
        healthcare.Role.Should().Be(UserRole.HealthcareStaff);
        healthcare.OrganizationName.Should().Be("City Hospital");
        healthcare.Department.Should().Be("Emergency");
        healthcare.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Creates_Laboratory_WithCorrectRole()
    {
        // Arrange & Act
        var lab = Laboratory.Create("Lab Tech", "tech@lab.com", "City Lab", "LAB123");

        // Assert - Domain rule: Laboratory has LabUser role
        lab.Role.Should().Be(UserRole.LabUser);
        lab.LabName.Should().Be("City Lab");
        lab.LicenseNumber.Should().Be("LAB123");
        lab.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Creates_ImagingCenter_WithCorrectRole()
    {
        // Arrange & Act
        var imaging = ImagingCenter.Create("Imaging Tech", "tech@imaging.com", "City Imaging", "IMG123");

        // Assert - Domain rule: ImagingCenter has ImagingUser role
        imaging.Role.Should().Be(UserRole.ImagingUser);
        imaging.CenterName.Should().Be("City Imaging");
        imaging.LicenseNumber.Should().Be("IMG123");
        imaging.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AllUsers_AreActive_ByDefault()
    {
        // Arrange & Act
        var patient = new Patient(Guid.NewGuid(), "John Doe", "john@example.com", "123", DateTime.Now);
        var doctor = Doctor.Create("Dr. Smith", "dr@example.com", "LIC1", "Cardiology");

        // Assert - Domain rule: All users are active by default
        patient.IsActive.Should().BeTrue();
        doctor.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Users_CanBe_Deactivated()
    {
        // Arrange
        var patient = new Patient(Guid.NewGuid(), "John Doe", "john@example.com", "123", DateTime.Now);

        // Act
        patient.Deactivate();

        // Assert - Domain rule: Users can be deactivated
        patient.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Users_CanBe_Reactivated()
    {
        // Arrange
        var patient = new Patient(Guid.NewGuid(), "John Doe", "john@example.com", "123", DateTime.Now);
        patient.Deactivate();

        // Act
        patient.Activate();

        // Assert - Domain rule: Users can be reactivated
        patient.IsActive.Should().BeTrue();
    }
}

