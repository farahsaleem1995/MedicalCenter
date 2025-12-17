using FluentAssertions;
using MedicalCenter.Core.Aggregates.Patient;
using Xunit;

namespace MedicalCenter.Core.Tests.ValueObjects;

public class BloodTypeTests
{
    [Fact]
    public void Create_CreatesBloodType_WithValidABOAndRh()
    {
        // Arrange & Act
        var bloodType = BloodType.Create(BloodABO.A, BloodRh.Positive);

        // Assert
        bloodType.ABO.Should().Be(BloodABO.A);
        bloodType.Rh.Should().Be(BloodRh.Positive);
    }

    [Theory]
    [InlineData(BloodABO.A, BloodRh.Positive, "A+")]
    [InlineData(BloodABO.B, BloodRh.Negative, "B-")]
    [InlineData(BloodABO.AB, BloodRh.Positive, "AB+")]
    [InlineData(BloodABO.O, BloodRh.Negative, "O-")]
    public void ToString_ReturnsCorrectFormat_ForAllBloodTypes(BloodABO abo, BloodRh rh, string expected)
    {
        // Arrange
        var bloodType = BloodType.Create(abo, rh);

        // Act
        var result = bloodType.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenBloodTypesAreEqual()
    {
        // Arrange
        var bloodType1 = BloodType.Create(BloodABO.A, BloodRh.Positive);
        var bloodType2 = BloodType.Create(BloodABO.A, BloodRh.Positive);

        // Act & Assert
        bloodType1.Should().Be(bloodType2);
        (bloodType1 == bloodType2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenABODiffers()
    {
        // Arrange
        var bloodType1 = BloodType.Create(BloodABO.A, BloodRh.Positive);
        var bloodType2 = BloodType.Create(BloodABO.B, BloodRh.Positive);

        // Act & Assert
        bloodType1.Should().NotBe(bloodType2);
        (bloodType1 != bloodType2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenRhDiffers()
    {
        // Arrange
        var bloodType1 = BloodType.Create(BloodABO.A, BloodRh.Positive);
        var bloodType2 = BloodType.Create(BloodABO.A, BloodRh.Negative);

        // Act & Assert
        bloodType1.Should().NotBe(bloodType2);
    }

    [Fact]
    public void GetHashCode_ReturnsSameHashCode_ForEqualBloodTypes()
    {
        // Arrange
        var bloodType1 = BloodType.Create(BloodABO.O, BloodRh.Negative);
        var bloodType2 = BloodType.Create(BloodABO.O, BloodRh.Negative);

        // Act & Assert
        bloodType1.GetHashCode().Should().Be(bloodType2.GetHashCode());
    }
}

