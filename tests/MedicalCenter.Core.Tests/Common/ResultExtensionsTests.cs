using FluentAssertions;
using MedicalCenter.Core.Primitives;
using Xunit;

namespace MedicalCenter.Core.Tests.Common;

public class ResultExtensionsTests
{
    [Fact]
    public void ToResult_ConvertsValueToSuccessResult()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = value.ToResult();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ToResult_ConvertsErrorToFailureResult()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");

        // Act
        var result = error.ToResult<string>();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Map_TransformsValue_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result<int>.Success(5);
        Func<int, string> transform = x => x.ToString();

        // Act
        var mappedResult = result.Map(transform);

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be("5");
    }

    [Fact]
    public void Map_PreservesError_WhenResultIsFailure()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);
        Func<int, string> transform = x => x.ToString();

        // Act
        var mappedResult = result.Map(transform);

        // Assert
        mappedResult.IsFailure.Should().BeTrue();
        mappedResult.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_ChainsResults_WhenFirstResultIsSuccess()
    {
        // Arrange
        var result = Result<int>.Success(5);
        Func<int, Result<string>> bindFunc = x => Result<string>.Success(x.ToString());

        // Act
        var boundResult = result.Bind(bindFunc);

        // Assert
        boundResult.IsSuccess.Should().BeTrue();
        boundResult.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_PreservesError_WhenFirstResultIsFailure()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);
        Func<int, Result<string>> bindFunc = x => Result<string>.Success(x.ToString());

        // Act
        var boundResult = result.Bind(bindFunc);

        // Assert
        boundResult.IsFailure.Should().BeTrue();
        boundResult.Error.Should().Be(error);
    }
}

