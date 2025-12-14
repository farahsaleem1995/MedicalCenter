using FluentAssertions;
using MedicalCenter.Core.Common;
using Xunit;

namespace MedicalCenter.Core.Tests.Entities;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity
    {
        public TestEntity() { }
        public TestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Constructor_GeneratesNewGuid_WhenNoIdProvided()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_UsesProvidedId_WhenIdProvided()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var entity = new TestEntity(expectedId);

        // Assert
        entity.Id.Should().Be(expectedId);
    }
}

