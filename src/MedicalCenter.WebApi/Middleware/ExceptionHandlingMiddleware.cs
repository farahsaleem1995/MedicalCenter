using System.Net;
using System.Text.Json;
using MedicalCenter.Core.Common;

namespace MedicalCenter.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Handles all exceptions and converts them to appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ArgumentException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = new ErrorDetail
                {
                    Code = ErrorCodes.InternalServerError,
                    Message = "An unexpected error occurred. Please contact support if the problem persists."
                }
            },
            InvalidOperationException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = new ErrorDetail
                {
                    Code = ErrorCodes.InternalServerError,
                    Message = "An unexpected error occurred. Please contact support if the problem persists."
                }
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = new ErrorDetail
                {
                    Code = ErrorCodes.InternalServerError,
                    Message = "An unexpected error occurred. Please contact support if the problem persists."
                }
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public ErrorDetail Error { get; set; } = null!;
    }

    private class ErrorDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

