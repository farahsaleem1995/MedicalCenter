namespace MedicalCenter.Core.Services;

/// <summary>
/// Provides unified time access across the application.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTime Now { get; }
}

