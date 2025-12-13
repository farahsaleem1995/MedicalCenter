using FluentAssertions;
using MedicalCenter.Core.Common;
using Xunit;

namespace MedicalCenter.Core.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Constructor_SetsCodeAndMessage()
    {
        // Arrange
        var code = "TestCode";
        var message = "Test message";

        // Act
        var error = new Error(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void None_ReturnsEmptyError()
    {
        // Act
        var error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Message.Should().BeEmpty();
    }

    [Fact]
    public void NotFound_CreatesErrorWithCorrectCodeAndMessage()
    {
        // Arrange
        var entityName = "Patient";

        // Act
        var error = Error.NotFound(entityName);

        // Assert
        error.Code.Should().Be("NotFound");
        error.Message.Should().Be($"{entityName} not found.");
    }

    [Fact]
    public void Validation_CreatesErrorWithValidationCode()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var error = Error.Validation(message);

        // Assert
        error.Code.Should().Be("Validation");
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Unauthorized_CreatesErrorWithUnauthorizedCode()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        error.Code.Should().Be("Unauthorized");
        error.Message.Should().Be("Unauthorized access.");
    }

    [Fact]
    public void Unauthorized_WithCustomMessage_UsesCustomMessage()
    {
        // Arrange
        var customMessage = "Custom unauthorized message";

        // Act
        var error = Error.Unauthorized(customMessage);

        // Assert
        error.Code.Should().Be("Unauthorized");
        error.Message.Should().Be(customMessage);
    }

    [Fact]
    public void Conflict_CreatesErrorWithConflictCode()
    {
        // Arrange
        var message = "Resource conflict";

        // Act
        var error = Error.Conflict(message);

        // Assert
        error.Code.Should().Be("Conflict");
        error.Message.Should().Be(message);
    }
}

