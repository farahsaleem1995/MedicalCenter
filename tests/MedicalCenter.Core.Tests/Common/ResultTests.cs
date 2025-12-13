using FluentAssertions;
using MedicalCenter.Core.Common;
using Xunit;

namespace MedicalCenter.Core.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsResultWithValue_WhenCreated()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ReturnsResultWithError_WhenCreated()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithCodeAndMessage_CreatesErrorCorrectly()
    {
        // Arrange
        var code = "TestCode";
        var message = "Test message";

        // Act
        var result = Result<string>.Failure(code, message);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(code);
        result.Error.Message.Should().Be(message);
    }

    [Fact]
    public void NonGenericSuccess_ReturnsSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void NonGenericFailure_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}

