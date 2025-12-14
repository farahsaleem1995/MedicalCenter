using MedicalCenter.Core.Common;

namespace MedicalCenter.WebApi.Extensions;

/// <summary>
/// Extension methods for mapping Result pattern to FastEndpoints responses.
/// Adapted from Ardalis Clean Architecture pattern for FastEndpoints usage.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps an error code to an HTTP status code.
    /// </summary>
    public static int ToStatusCode(this string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.NotFound => 404,
            ErrorCodes.Validation => 400,
            ErrorCodes.Conflict => 409,
            ErrorCodes.Unauthorized => 401,
            ErrorCodes.Forbidden => 403,
            _ => 500
        };
    }
}
