using MedicalCenter.Core.Services;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IDateTimeProvider that returns current UTC time.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime Now => DateTime.UtcNow;
}

