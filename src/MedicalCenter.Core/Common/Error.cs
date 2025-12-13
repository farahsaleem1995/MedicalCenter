namespace MedicalCenter.Core.Common;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
public class Error
{
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }

    public static Error None => new(string.Empty, string.Empty);
    public static Error NotFound(string entityName) => new("NotFound", $"{entityName} not found.");
    public static Error Validation(string message) => new("Validation", message);
    public static Error Unauthorized(string message = "Unauthorized access.") => new("Unauthorized", message);
    public static Error Conflict(string message) => new("Conflict", message);
}

