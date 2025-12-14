using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Enums;
using MedicalCenter.Core.ValueObjects;
using Xunit;
using PatientEntity = MedicalCenter.Core.Aggregates.Patient.Patient;

namespace MedicalCenter.Core.Tests.Aggregates.Patient;

/// <summary>
/// Domain tests for Patient aggregate authorization rules.
/// Following classical school: testing business rules and invariants, not implementation details.
/// </summary>
public class PatientAuthorizationRulesTests
{
    private PatientEntity CreateTestPatient()
    {
        return PatientEntity.Create("John Doe", "john.doe@example.com", "123456789", new DateTime(1990, 1, 15));
    }

    [Fact]
    public void Allows_MedicalAttributes_ToBeUpdated_ByAuthorizedProvider()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var bloodType = BloodType.Create(BloodABO.A, BloodRh.Positive);

        // Act - Domain allows the update (authorization is infrastructure concern)
        patient.UpdateBloodType(bloodType);
        var allergy = patient.AddAllergy("Peanuts", "Severe");

        // Assert - Domain rule: Medical attributes can be updated
        patient.BloodType.Should().NotBeNull();
        patient.Allergies.Should().ContainSingle();
        patient.Allergies.First().Name.Should().Be("Peanuts");
    }

    [Fact]
    public void Enforces_MedicalAttribute_Consistency_WhenUpdating()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var allergy = patient.AddAllergy("Peanuts", "Severe");
        var allergyId = allergy.Id;

        // Act
        patient.UpdateAllergy(allergyId, "Moderate", "Updated notes");

        // Assert - Domain rule: Updates maintain consistency within aggregate
        var updatedAllergy = patient.Allergies.First(a => a.Id == allergyId);
        updatedAllergy.Severity.Should().Be("Moderate");
        updatedAllergy.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public void Prevents_Updating_NonExistent_MedicalAttribute()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Domain rule: Cannot update non-existent attribute
        var act = () => patient.UpdateAllergy(nonExistentId, "Severe", "Notes");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Allergy with ID {nonExistentId} not found.");
    }

    [Fact]
    public void Enforces_Medication_DateConstraints_WhenAdding()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2023, 12, 31); // Before start date

        // Act & Assert - Domain rule: End date cannot be before start date
        var act = () => patient.AddMedication("Aspirin", "100mg", startDate, endDate);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Enforces_ChronicDisease_DiagnosisDate_NotInFuture()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert - Domain rule: Diagnosis date cannot be in the future
        var act = () => patient.AddChronicDisease("Diabetes", futureDate);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Enforces_Surgery_Date_NotInFuture()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert - Domain rule: Surgery date cannot be in the future
        var act = () => patient.AddSurgery("Appendectomy", futureDate);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Maintains_Aggregate_Consistency_WhenRemoving_MedicalAttributes()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var allergy1 = patient.AddAllergy("Peanuts", "Severe");
        var allergy2 = patient.AddAllergy("Shellfish", "Moderate");

        // Act
        patient.RemoveAllergy(allergy1.Id);

        // Assert - Domain rule: Aggregate maintains consistency after removal
        patient.Allergies.Should().HaveCount(1);
        patient.Allergies.First().Name.Should().Be("Shellfish");
    }

    [Fact]
    public void Allows_Multiple_MedicalAttributes_OfSameType()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();

        // Act
        patient.AddAllergy("Peanuts", "Severe");
        patient.AddAllergy("Shellfish", "Moderate");
        patient.AddAllergy("Dairy", "Mild");

        // Assert - Domain rule: Multiple attributes of same type are allowed
        patient.Allergies.Should().HaveCount(3);
    }

    [Fact]
    public void Enforces_BloodType_ValueObject_Immutability()
    {
        // Arrange
        PatientEntity patient = CreateTestPatient();
        var bloodType1 = BloodType.Create(BloodABO.A, BloodRh.Positive);
        var bloodType2 = BloodType.Create(BloodABO.A, BloodRh.Positive);

        // Act
        patient.UpdateBloodType(bloodType1);

        // Assert - Domain rule: Value objects are equal if attributes are equal
        patient.BloodType.Should().Be(bloodType2);
        (patient.BloodType == bloodType2).Should().BeTrue();
    }
}

