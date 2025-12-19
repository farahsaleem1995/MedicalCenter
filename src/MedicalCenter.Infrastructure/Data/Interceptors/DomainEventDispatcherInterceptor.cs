using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MedicalCenter.Core.SharedKernel.Events;

namespace MedicalCenter.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that dispatches domain events before SaveChanges completes.
/// Collects all domain events from entities implementing IHasDomainEvents,
/// dispatches them via MediatR, and clears them after dispatch.
/// </summary>
public class DomainEventDispatcherInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    private readonly IMediator _mediator = mediator;

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null)
        {
            // For synchronous SaveChanges, we'll dispatch asynchronously but wait
            // This is a compromise - ideally all SaveChanges should be async
            DispatchDomainEventsAsync(eventData.Context, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavedChanges(eventData, result);
    }

    private async Task DispatchDomainEventsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events before dispatching to prevent duplicate dispatch
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Dispatch all events via MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}

