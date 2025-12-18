using FluentAssertions;
using MedicalCenter.Core.Abstractions;
using Xunit;

namespace MedicalCenter.Core.Tests.ValueObjects;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string Property1 { get; }
        public int Property2 { get; }

        public TestValueObject(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Property1;
            yield return Property2;
        }
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenAllPropertiesAreEqual()
    {
        // Arrange
        var valueObject1 = new TestValueObject("test", 42);
        var valueObject2 = new TestValueObject("test", 42);

        // Act & Assert
        valueObject1.Equals(valueObject2).Should().BeTrue();
        (valueObject1 == valueObject2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenPropertiesDiffer()
    {
        // Arrange
        var valueObject1 = new TestValueObject("test", 42);
        var valueObject2 = new TestValueObject("test", 43);

        // Act & Assert
        valueObject1.Equals(valueObject2).Should().BeFalse();
        (valueObject1 != valueObject2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenComparedWithNull()
    {
        // Arrange
        var valueObject = new TestValueObject("test", 42);

        // Act & Assert
        valueObject.Equals(null).Should().BeFalse();
        (valueObject == null).Should().BeFalse();
        (valueObject != null).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ReturnsSameValue_ForEqualObjects()
    {
        // Arrange
        var valueObject1 = new TestValueObject("test", 42);
        var valueObject2 = new TestValueObject("test", 42);

        // Act
        var hashCode1 = valueObject1.GetHashCode();
        var hashCode2 = valueObject2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }
}

