using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Enums;
using MedicalCenter.Core.Aggregates.Patients.ValueObjects;
using Xunit;
using PatientAggregate = MedicalCenter.Core.Aggregates.Patients.Patient;

namespace MedicalCenter.Core.Tests.Aggregates.Patient;

public class PatientMedicalAttributesTests
{
    private PatientAggregate CreateTestPatient()
    {
        return PatientAggregate.Create("John Doe", "john.doe@example.com", "123456789", new DateTime(1990, 1, 15));
    }

    [Fact]
    public void UpdateBloodType_SetsBloodType_WhenValidBloodTypeProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var bloodType = BloodType.Create(BloodABO.A, BloodRh.Positive);

        // Act
        patient.UpdateBloodType(bloodType);

        // Assert
        patient.BloodType.Should().NotBeNull();
        patient.BloodType!.ABO.Should().Be(BloodABO.A);
        patient.BloodType.Rh.Should().Be(BloodRh.Positive);
    }

    [Fact]
    public void UpdateBloodType_ClearsBloodType_WhenNullProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var bloodType = BloodType.Create(BloodABO.O, BloodRh.Negative);
        patient.UpdateBloodType(bloodType);

        // Act
        patient.UpdateBloodType(null);

        // Assert
        patient.BloodType.Should().BeNull();
    }

    [Fact]
    public void AddAllergy_AddsAllergyToCollection_WhenValidAllergyProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();

        // Act
        var allergy = patient.AddAllergy("Peanuts", "Severe", "Causes anaphylaxis");

        // Assert
        patient.Allergies.Should().ContainSingle();
        patient.Allergies.First().Name.Should().Be("Peanuts");
        patient.Allergies.First().Severity.Should().Be("Severe");
        patient.Allergies.First().Notes.Should().Be("Causes anaphylaxis");
        allergy.PatientId.Should().Be(patient.Id);
    }

    [Fact]
    public void RemoveAllergy_RemovesAllergyFromCollection_WhenAllergyExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var allergy = patient.AddAllergy("Peanuts", "Severe");

        // Act
        patient.RemoveAllergy(allergy.Id);

        // Assert
        patient.Allergies.Should().BeEmpty();
    }

    [Fact]
    public void UpdateAllergy_UpdatesAllergyProperties_WhenAllergyExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var allergy = patient.AddAllergy("Peanuts", "Mild");

        // Act
        patient.UpdateAllergy(allergy.Id, "Severe", "Updated notes");

        // Assert
        var updatedAllergy = patient.Allergies.First();
        updatedAllergy.Severity.Should().Be("Severe");
        updatedAllergy.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public void UpdateAllergy_ThrowsInvalidOperationException_WhenAllergyNotFound()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var act = () => patient.UpdateAllergy(nonExistentId, "Severe", "Notes");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Allergy with ID {nonExistentId} not found.");
    }

    [Fact]
    public void AddChronicDisease_AddsChronicDiseaseToCollection_WhenValidChronicDiseaseProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var diagnosisDate = new DateTime(2020, 1, 1);

        // Act
        var chronicDisease = patient.AddChronicDisease("Diabetes", diagnosisDate, "Type 2");

        // Assert
        patient.ChronicDiseases.Should().ContainSingle();
        patient.ChronicDiseases.First().Name.Should().Be("Diabetes");
        patient.ChronicDiseases.First().DiagnosisDate.Should().Be(diagnosisDate);
        patient.ChronicDiseases.First().Notes.Should().Be("Type 2");
        chronicDisease.PatientId.Should().Be(patient.Id);
    }

    [Fact]
    public void RemoveChronicDisease_RemovesChronicDiseaseFromCollection_WhenChronicDiseaseExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var chronicDisease = patient.AddChronicDisease("Diabetes", new DateTime(2020, 1, 1));

        // Act
        patient.RemoveChronicDisease(chronicDisease.Id);

        // Assert
        patient.ChronicDiseases.Should().BeEmpty();
    }

    [Fact]
    public void UpdateChronicDisease_UpdatesChronicDiseaseNotes_WhenChronicDiseaseExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var chronicDisease = patient.AddChronicDisease("Diabetes", new DateTime(2020, 1, 1), "Initial notes");

        // Act
        patient.UpdateChronicDisease(chronicDisease.Id, "Updated notes");

        // Assert
        var updatedChronicDisease = patient.ChronicDiseases.First();
        updatedChronicDisease.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public void AddMedication_AddsMedicationToCollection_WhenValidMedicationProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var startDate = new DateTime(2024, 1, 1);

        // Act
        var medication = patient.AddMedication("Aspirin", "100mg daily", startDate, null, "For heart health");

        // Assert
        patient.Medications.Should().ContainSingle();
        patient.Medications.First().Name.Should().Be("Aspirin");
        patient.Medications.First().Dosage.Should().Be("100mg daily");
        patient.Medications.First().StartDate.Should().Be(startDate);
        patient.Medications.First().EndDate.Should().BeNull();
        medication.PatientId.Should().Be(patient.Id);
    }

    [Fact]
    public void AddMedication_AddsMedicationWithEndDate_WhenEndDateProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 6, 1);

        // Act
        var medication = patient.AddMedication("Antibiotic", "500mg", startDate, endDate);

        // Assert
        patient.Medications.First().EndDate.Should().Be(endDate);
    }

    [Fact]
    public void UpdateMedication_UpdatesMedicationProperties_WhenMedicationExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var medication = patient.AddMedication("Aspirin", "100mg", new DateTime(2024, 1, 1));
        var newEndDate = new DateTime(2024, 12, 31);

        // Act
        patient.UpdateMedication(medication.Id, "200mg", newEndDate, "Updated dosage");

        // Assert
        var updatedMedication = patient.Medications.First();
        updatedMedication.Dosage.Should().Be("200mg");
        updatedMedication.EndDate.Should().Be(newEndDate);
        updatedMedication.Notes.Should().Be("Updated dosage");
    }

    [Fact]
    public void AddSurgery_AddsSurgeryToCollection_WhenValidSurgeryProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var surgeryDate = new DateTime(2023, 6, 15);

        // Act
        var surgery = patient.AddSurgery("Appendectomy", surgeryDate, "Dr. Smith", "Successful procedure");

        // Assert
        patient.Surgeries.Should().ContainSingle();
        patient.Surgeries.First().Name.Should().Be("Appendectomy");
        patient.Surgeries.First().Date.Should().Be(surgeryDate);
        patient.Surgeries.First().Surgeon.Should().Be("Dr. Smith");
        patient.Surgeries.First().Notes.Should().Be("Successful procedure");
        surgery.PatientId.Should().Be(patient.Id);
    }

    [Fact]
    public void RemoveSurgery_RemovesSurgeryFromCollection_WhenSurgeryExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var surgery = patient.AddSurgery("Appendectomy", new DateTime(2023, 6, 15));

        // Act
        patient.RemoveSurgery(surgery.Id);

        // Assert
        patient.Surgeries.Should().BeEmpty();
    }

    [Fact]
    public void UpdateSurgery_UpdatesSurgeryProperties_WhenSurgeryExists()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();
        var surgery = patient.AddSurgery("Appendectomy", new DateTime(2023, 6, 15), "Dr. Smith");

        // Act
        patient.UpdateSurgery(surgery.Id, "Dr. Jones", "Updated notes");

        // Assert
        var updatedSurgery = patient.Surgeries.First();
        updatedSurgery.Surgeon.Should().Be("Dr. Jones");
        updatedSurgery.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public void AddMultipleMedicalAttributes_AddsAllAttributes_WhenMultipleAttributesProvided()
    {
        // Arrange
        PatientAggregate patient = CreateTestPatient();

        // Act
        patient.AddAllergy("Peanuts", "Severe");
        patient.AddAllergy("Shellfish", "Moderate");
        patient.AddChronicDisease("Diabetes", new DateTime(2020, 1, 1));
        patient.AddMedication("Insulin", "10 units", new DateTime(2024, 1, 1));
        patient.AddSurgery("Knee replacement", new DateTime(2022, 3, 10));

        // Assert
        patient.Allergies.Should().HaveCount(2);
        patient.ChronicDiseases.Should().HaveCount(1);
        patient.Medications.Should().HaveCount(1);
        patient.Surgeries.Should().HaveCount(1);
    }
}

