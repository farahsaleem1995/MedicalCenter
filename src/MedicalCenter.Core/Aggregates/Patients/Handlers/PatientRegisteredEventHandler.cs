using MediatR;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.Patients.Events;

namespace MedicalCenter.Core.Aggregates.Patients.Handlers;

/// <summary>
/// Handler for PatientRegisteredEvent domain event.
/// This is an example handler showing how to implement INotificationHandler for domain events.
/// </summary>
public class PatientRegisteredEventHandler(ILogger<PatientRegisteredEventHandler> logger)
    : INotificationHandler<PatientRegisteredEvent>
{
    private readonly ILogger<PatientRegisteredEventHandler> _logger = logger;

    public Task Handle(PatientRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Patient registered: {PatientId}, Name: {FullName}, Email: {Email}, NationalId: {NationalId}",
            notification.PatientId,
            notification.FullName,
            notification.Email,
            notification.NationalId);

        // Here you could:
        // - Send welcome email
        // - Create audit log entry
        // - Update search indexes
        // - Trigger other side effects

        return Task.CompletedTask;
    }
}

