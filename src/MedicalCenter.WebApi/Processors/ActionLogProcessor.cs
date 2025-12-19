using System.Security.Claims;
using System.Text.Json;
using FastEndpoints;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.WebApi.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MedicalCenter.WebApi.Processors;

/// <summary>
/// Global post-processor that records action log entries for endpoints marked with [ActionLog] attribute.
/// Only records if the request was successful (2xx status codes).
/// </summary>
public class ActionLogProcessor : IGlobalPostProcessor
{
    public Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        try
        {
            // Check if endpoint has [ActionLog] attribute
            string? description = GetActionLogDescription(context.HttpContext);
            if (string.IsNullOrWhiteSpace(description))
            {
                return Task.CompletedTask; // Endpoint not marked for logging
            }
            
            // Only record if request was successful (2xx status codes)
            int statusCode = context.HttpContext.Response.StatusCode;
            if (statusCode is < 200 or >= 300)
            {
                return Task.CompletedTask; // Don't log failed requests
            }
            
            // Resolve service from context
            IActionLogService actionLogService = context.HttpContext.RequestServices.GetRequiredService<IActionLogService>();
            
            // Extract metadata
            string actionName = ExtractActionName(context.HttpContext);
            Guid? userId = ExtractUserId(context.HttpContext);
            string? payload = SerializeRequest(context.Request, filterSensitive: true);
            
            // Create and record action log entry
            ActionLogEntry entry = ActionLogEntry.Create(
                actionName,
                description,
                userId,
                payload);
            
            actionLogService.Record(entry);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request - action log is non-critical
            try
            {
                ILogger<ActionLogProcessor> logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<ActionLogProcessor>>();
                logger.LogError(ex, "Failed to record action log entry");
            }
            catch
            {
                // If we can't even get a logger, silently fail
            }
        }
        
        return Task.CompletedTask;
    }
    
    private static string? GetActionLogDescription(HttpContext httpContext)
    {
        Endpoint? endpoint = httpContext.GetEndpoint();
        if (endpoint == null)
        {
            return null;
        }
        
        EndpointDefinition? endpointDef = endpoint.Metadata.GetMetadata<EndpointDefinition>();
        if (endpointDef?.EndpointType == null)
        {
            return null;
        }
        
        ActionLogAttribute? attribute = endpointDef.EndpointType
            .GetCustomAttributes(typeof(ActionLogAttribute), inherit: true)
            .FirstOrDefault() as ActionLogAttribute;
        
        return attribute?.Description;
    }
    
    private static string ExtractActionName(HttpContext httpContext)
    {
        // Get endpoint type from metadata
        Endpoint? endpoint = httpContext.GetEndpoint();
        if (endpoint != null)
        {
            EndpointDefinition? endpointDef = endpoint.Metadata.GetMetadata<EndpointDefinition>();
            if (endpointDef != null && endpointDef.EndpointType != null)
            {
                string className = endpointDef.EndpointType.Name;
                // Remove "Endpoint" suffix if present
                if (className.EndsWith("Endpoint", StringComparison.OrdinalIgnoreCase))
                {
                    return className[..^8]; // Remove last 8 characters ("Endpoint")
                }
                return className;
            }
        }
        
        // Fallback to route path if endpoint type not available
        return httpContext.Request.Path.Value ?? "Unknown";
    }
    
    private static Guid? ExtractUserId(HttpContext httpContext)
    {
        string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }
        
        return Guid.TryParse(userIdClaim, out Guid userId) ? userId : null;
    }
    
    private static string? SerializeRequest(object? request, bool filterSensitive)
    {
        if (request == null)
        {
            return null;
        }
        
        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(request, options);
            
            // Filter sensitive data
            if (filterSensitive)
            {
                json = FilterSensitiveData(json);
            }
            
            // Truncate if too long (max 10KB)
            const int maxLength = 10000;
            if (json.Length > maxLength)
            {
                json = json[..maxLength] + "... [truncated]";
            }
            
            return json;
        }
        catch
        {
            return null;
        }
    }
    
    private static string FilterSensitiveData(string json)
    {
        // List of sensitive field patterns to filter
        string[] sensitivePatterns = new[]
        {
            "\"password\"",
            "\"passwordHash\"",
            "\"token\"",
            "\"refreshToken\"",
            "\"creditCard\"",
            "\"ssn\"",
            "\"nationalId\"",
            "\"currentPassword\"",
            "\"newPassword\""
        };
        
        string result = json;
        foreach (string pattern in sensitivePatterns)
        {
            // Simple regex replacement - replace value with [REDACTED]
            // This is a basic implementation - could be enhanced with proper JSON parsing
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(
                $"{pattern}\\s*:\\s*\"[^\"]*\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = regex.Replace(result, $"{pattern}: \"[REDACTED]\"");
        }
        
        return result;
    }
}

