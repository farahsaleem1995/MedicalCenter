# Domain Events Implementation Plan

**Status**: Temporary plan - to be removed after implementation  
**Date**: 2025-12-16  
**Last Updated**: 2025-12-18 (Updated for Core layer reorganization)  
**Objective**: Introduce domain events infrastructure using MediatR. Domain events allow aggregates to communicate changes across boundaries without direct coupling.

## Important: Namespace Updates

This plan has been updated to reflect the Core layer reorganization completed on 2025-12-18:

- **`MedicalCenter.Core.Common`** → Split into:
  - `MedicalCenter.Core.Abstractions` (BaseEntity, IAggregateRoot, IAuditableEntity, ValueObject)
  - `MedicalCenter.Core.Primitives` (Result, Error, ErrorCodes, Pagination)
  - `MedicalCenter.Core.SharedKernel` (User, UserRole, IRepository, IUnitOfWork, Attachment)
  - `MedicalCenter.Core.SharedKernel.Events` (Domain event infrastructure)
- **Domain Events Location**: All domain event base types are in `SharedKernel/Events/`:
  - `IDomainEvent` (marker interface)
  - `DomainEventBase` (abstract base class implementing `IDomainEvent`)
  - `IHasDomainEvents` (interface for aggregates)
  - `IDomainEventHandler<T>` (handler interface - note: uses `IDomainEventHandler` not `IEventHandler`)
  - `IEventDispatcher` (dispatcher interface)
- **Aggregate namespaces**:
  - `MedicalCenter.Core.Aggregates.Patients` (Patient - renamed from `Patient` singular)
  - `MedicalCenter.Core.Aggregates.MedicalRecords` (MedicalRecord - renamed from `MedicalRecord` singular)

**Note**: The domain events base infrastructure already exists in `SharedKernel/Events/`. This plan focuses on implementing the MediatR integration and using the existing infrastructure.

---

## Current State

### Current Implementation
- No domain events infrastructure
- Aggregates cannot communicate changes to other aggregates
- No event-driven architecture patterns
- Direct coupling between aggregates when coordination is needed

### Requirements
1. **Domain Event Infrastructure**: Already exists in `SharedKernel/Events/`:
   - `IDomainEvent` interface (marker interface)
   - `DomainEventBase` abstract base class (implements `IDomainEvent`)
   - `IHasDomainEvents` interface (for aggregates)
   - `IDomainEventHandler<T>` interface (for handlers - note: uses `IDomainEventHandler` not `IEventHandler`)
   - `IEventDispatcher` interface (for dispatching)
2. **MediatR Integration**: Infrastructure layer implements using MediatR
   - Custom MediatR NotificationHandler
   - EventDispatcher wraps domain events and maps them to MediatR notifications
   - Custom NotificationHandler dispatches to registered `IDomainEventHandler` implementations

---

## Target State

### Core Layer (SharedKernel/Events)
- `IDomainEvent` marker interface (already exists)
- `DomainEventBase` abstract base class (already exists - implements `IDomainEvent`)
- `IHasDomainEvents` interface with domain events collection (already exists)
- `IDomainEventHandler<TDomainEvent>` generic interface (already exists - note: uses `IDomainEventHandler` not `IEventHandler`)
- `IEventDispatcher` interface for dispatching events (already exists)

### Infrastructure Layer (Implementation)
- `MediatREventDispatcher` implementing `IEventDispatcher`
- `DomainEventNotification<TDomainEvent>` MediatR notification wrapper
- `DomainEventNotificationHandler` MediatR notification handler
- Integration with `UnitOfWork` to dispatch events after successful save

### Aggregate Integration
- Aggregates implement `IHasDomainEvents`
- Aggregates raise domain events during business operations
- Events are dispatched after transaction commit

---

## Implementation Steps

### Step 1: Verify Domain Event Infrastructure

**Files**: Already exist in `src/MedicalCenter.Core/SharedKernel/Events/`:
- `IDomainEvent.cs` - Marker interface
- `DomainEventBase.cs` - Abstract base class implementing `IDomainEvent`
- `IHasDomainEvents.cs` - Interface for aggregates
- `IDomainEventHandler.cs` - Handler interface
- `IEventDispatcher.cs` - Dispatcher interface

**Verify Implementation**:
- `IDomainEvent` has `OccurredOn` property (DateTime)
- `DomainEventBase` implements `IDomainEvent` with default `OccurredOn = DateTime.UtcNow`
- `IHasDomainEvents` has methods: `AddDomainEvent`, `RemoveDomainEvent`, `ClearDomainEvents`
- `IDomainEventHandler<T>` where `T : IDomainEvent` (note: uses `IDomainEventHandler` not `IEventHandler`)
- `IEventDispatcher` has `DispatchAsync` methods for single and multiple events

**Notes**:
- Domain event infrastructure already exists
- All domain events should implement `IDomainEvent` (or inherit from `DomainEventBase`)
- Use `IDomainEventHandler<T>` for handlers (not `IEventHandler<T>`)

---

### Step 2: Verify IHasDomainEvents Interface

**File**: `src/MedicalCenter.Core/SharedKernel/Events/IHasDomainEvents.cs` (already exists)

**Current Implementation**:
```csharp
namespace MedicalCenter.Core.SharedKernel.Events;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    
    void AddDomainEvent(IDomainEvent domainEvent);
    
    void RemoveDomainEvent(IDomainEvent domainEvent);
    
    void ClearDomainEvents();
}
```

**Notes**:
- Already exists in `SharedKernel/Events/`
- Uses `IDomainEvent` (not `DomainEvent`)
- Provides methods to manage domain events collection
- Events are collected during aggregate operations
- Cleared after successful dispatch

---

### Step 3: Verify IDomainEventHandler Interface

**File**: `src/MedicalCenter.Core/SharedKernel/Events/IDomainEventHandler.cs` (already exists)

**Current Implementation**:
```csharp
namespace MedicalCenter.Core.SharedKernel.Events;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
}
```

**Notes**:
- Already exists in `SharedKernel/Events/`
- Uses `IDomainEventHandler<T>` (not `IEventHandler<T>`)
- Generic interface for type-safe event handling
- Contravariant (`in`) to allow flexibility
- Async to support I/O operations
- Constraint: `T : IDomainEvent` (not `DomainEvent`)

---

### Step 4: Verify IEventDispatcher Interface

**File**: `src/MedicalCenter.Core/SharedKernel/Events/IEventDispatcher.cs` (already exists)

**Current Implementation**:
```csharp
namespace MedicalCenter.Core.SharedKernel.Events;

public interface IEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
```

**Notes**:
- Already exists in `SharedKernel/Events/`
- Uses `IDomainEvent` (not `DomainEvent`)
- Simple interface for dispatching events
- Supports both single and batch dispatch
- Async operations

---

### Step 5: Add MediatR NuGet Package

**Command**:
```bash
dotnet add src/MedicalCenter.Infrastructure/MedicalCenter.Infrastructure.csproj package MediatR
```

**Notes**:
- MediatR provides in-process messaging
- Used for dispatching domain events

---

### Step 6: Create DomainEventNotification Wrapper

**File**: `src/MedicalCenter.Infrastructure/Events/DomainEventNotification.cs`

**Implementation**:
```csharp
using MediatR;
using MedicalCenter.Core.SharedKernel.Events;

namespace MedicalCenter.Infrastructure.Events;

/// <summary>
/// MediatR notification wrapper for domain events.
/// This allows domain events to be dispatched through MediatR's notification pipeline.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event</typeparam>
public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
```

**Required Using Statement**:
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEvent
```

**Notes**:
- Wraps domain events as MediatR notifications
- Generic to maintain type safety
- Simple wrapper pattern

---

### Step 7: Create MediatREventDispatcher

**File**: `src/MedicalCenter.Infrastructure/Events/MediatREventDispatcher.cs`

**Implementation**:
```csharp
using MediatR;
using MedicalCenter.Core.SharedKernel.Events;
using MedicalCenter.Infrastructure.Events;

namespace MedicalCenter.Infrastructure.Events;

/// <summary>
/// MediatR-based implementation of IEventDispatcher.
/// Wraps domain events in MediatR notifications and publishes them.
/// </summary>
public class MediatREventDispatcher : IEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatREventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await DispatchAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Create MediatR notification wrapper
            var notification = CreateNotification(domainEvent);
            
            // Publish through MediatR
            await _mediator.Publish(notification, cancellationToken);
        }
    }

    private static INotification CreateNotification(IDomainEvent domainEvent)
    {
        // Use reflection to create the generic DomainEventNotification<T>
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }
}
```

**Required Using Statements**:
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEvent, IEventDispatcher
```

**Notes**:
- Implements `IEventDispatcher`
- Uses MediatR's `IMediator.Publish` to dispatch notifications
- Uses reflection to create generic notification wrappers
- Handles both single and batch dispatch

---

### Step 8: Create DomainEventNotificationHandler

**File**: `src/MedicalCenter.Infrastructure/Events/DomainEventNotificationHandler.cs`

**Implementation**:
```csharp
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MedicalCenter.Core.SharedKernel.Events;
using MedicalCenter.Infrastructure.Events;

namespace MedicalCenter.Infrastructure.Events;

/// <summary>
/// MediatR notification handler that dispatches domain events to registered IDomainEventHandler implementations.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event</typeparam>
public class DomainEventNotificationHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventNotificationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(DomainEventNotification<TDomainEvent> notification, CancellationToken cancellationToken)
    {
        // Get all registered handlers for this domain event type
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();

        // Dispatch to all handlers
        var tasks = handlers.Select(handler => handler.HandleAsync(notification.DomainEvent, cancellationToken));
        await Task.WhenAll(tasks);
    }
}
```

**Required Using Statements**:
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEvent, IDomainEventHandler<T>
```

**Notes**:
- MediatR notification handler
- Resolves all `IDomainEventHandler<TDomainEvent>` implementations from DI container (note: uses `IDomainEventHandler` not `IEventHandler`)
- Dispatches to all registered handlers (supports multiple handlers per event)
- Uses `Task.WhenAll` for parallel execution

---

### Step 9: Update BaseEntity to Support Domain Events

**File**: `src/MedicalCenter.Core/Abstractions/BaseEntity.cs`

**Changes**:
- Make `BaseEntity` implement `IHasDomainEvents` (if appropriate)
- OR: Create a base class that aggregates can inherit from

**Option 1: Update BaseEntity** (if all entities should support events):
```csharp
using MedicalCenter.Core.SharedKernel.Events;

public abstract class BaseEntity : IHasDomainEvents
{
    public Guid Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Option 2: Create AggregateBase** (if only aggregates should support events):
```csharp
// New file: src/MedicalCenter.Core/Abstractions/AggregateBase.cs
using MedicalCenter.Core.Abstractions; // For BaseEntity, IAggregateRoot
using MedicalCenter.Core.SharedKernel.Events; // For IHasDomainEvents, IDomainEvent

public abstract class AggregateBase : BaseEntity, IHasDomainEvents, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Recommendation**: Option 2 (AggregateBase) - only aggregates should raise domain events, not all entities.

**Update Aggregates**:
- Change `Patient : User, IAggregateRoot` to `Patient : User, IAggregateRoot, IHasDomainEvents`
- Or if using AggregateBase: `Patient : User, AggregateBase`
- Add domain events collection and methods to each aggregate
- Use `IDomainEvent` (not `DomainEvent`) in collections
- Required using: `using MedicalCenter.Core.SharedKernel.Events;`

---

### Step 10: Integrate with UnitOfWork

**File**: `src/MedicalCenter.Infrastructure/Repositories/UnitOfWork.cs`

**Changes**:
- Inject `IEventDispatcher`
- After successful `SaveChangesAsync`, collect all domain events from tracked entities
- Dispatch events
- Clear events from aggregates

**Update SaveChangesAsync**:
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IHasDomainEvents, IDomainEvent

public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // Collect domain events from all tracked aggregates
    var domainEvents = _context.ChangeTracker.Entries<IHasDomainEvents>()
        .SelectMany(entry => entry.Entity.DomainEvents)
        .ToList();

    // Save changes first
    var result = await _context.SaveChangesAsync(cancellationToken);

    // Dispatch domain events after successful save
    if (domainEvents.Any())
    {
        await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        // Clear events from aggregates
        foreach (var entry in _context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }

    return result;
}
```

**Update Constructor**:
```csharp
private readonly IEventDispatcher _eventDispatcher;

public UnitOfWork(MedicalCenterDbContext context, IEventDispatcher eventDispatcher)
{
    _context = context;
    _eventDispatcher = eventDispatcher;
}
```

**Notes**:
- Events are dispatched AFTER successful save (transaction committed)
- If save fails, events are not dispatched
- Events are cleared after dispatch to prevent duplicate processing

---

### Step 11: Register Services in DependencyInjection

**File**: `src/MedicalCenter.Infrastructure/DependencyInjection.cs`

**Add**:
```csharp
using MediatR;
using MedicalCenter.Core.SharedKernel.Events; // For IEventDispatcher
using MedicalCenter.Infrastructure.Events;

// Add MediatR
services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(MediatREventDispatcher).Assembly);
});

// Register event dispatcher
services.AddScoped<IEventDispatcher, MediatREventDispatcher>();

// Register domain event notification handlers
services.AddScoped(typeof(INotificationHandler<>), typeof(DomainEventNotificationHandler<>));
```

**Notes**:
- MediatR scans for handlers in the Infrastructure assembly
- `IEventDispatcher` registered as scoped (same lifetime as DbContext)
- Notification handlers registered via MediatR's assembly scanning

---

### Step 12: Create Example Domain Event

**File**: `src/MedicalCenter.Core/Aggregates/Patients/Events/PatientRegisteredEvent.cs`

**Implementation** (Example):
```csharp
using MedicalCenter.Core.SharedKernel.Events;

namespace MedicalCenter.Core.Aggregates.Patients.Events;

/// <summary>
/// Domain event raised when a patient is registered.
/// </summary>
public class PatientRegisteredEvent : DomainEventBase
{
    public Guid PatientId { get; }
    public string Email { get; }
    public string FullName { get; }

    public PatientRegisteredEvent(Guid patientId, string email, string fullName)
    {
        PatientId = patientId;
        Email = email;
        FullName = fullName;
    }
}
```

**Required Using Statement**:
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For DomainEventBase (or IDomainEvent)
```

**Notes**:
- Inherit from `DomainEventBase` (which implements `IDomainEvent`)
- Or implement `IDomainEvent` directly if needed
- Located in `Aggregates/Patients/Events/` (not `Patient/Events/`)

**Update Patient Aggregate** (Example):
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IHasDomainEvents, IDomainEvent
using MedicalCenter.Core.Aggregates.Patients.Events; // For PatientRegisteredEvent

public static Patient Create(string fullName, string email, string nationalId, DateTime dateOfBirth)
{
    Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
    Guard.Against.NullOrWhiteSpace(email, nameof(email));
    Guard.Against.NullOrWhiteSpace(nationalId, nameof(nationalId));
    Guard.Against.OutOfRange(dateOfBirth, nameof(dateOfBirth), DateTime.MinValue, DateTime.UtcNow);

    var patient = new Patient(fullName, email, nationalId, dateOfBirth);
    
    // Raise domain event
    patient.AddDomainEvent(new PatientRegisteredEvent(patient.Id, patient.Email, patient.FullName));
    
    return patient;
}
```

**Notes**:
- Domain events are raised in aggregate methods
- Events are added to the collection, not dispatched immediately
- Dispatch happens after transaction commit

---

### Step 13: Create Example Event Handler

**File**: `src/MedicalCenter.Infrastructure/Handlers/PatientRegisteredEventHandler.cs`

**Implementation** (Example):
```csharp
using MedicalCenter.Core.Aggregates.Patients.Events;
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEventHandler<T>
using Microsoft.Extensions.Logging;

namespace MedicalCenter.Infrastructure.Handlers;

/// <summary>
/// Handles PatientRegisteredEvent.
/// Example: Send welcome email, create audit log, etc.
/// </summary>
public class PatientRegisteredEventHandler : IDomainEventHandler<PatientRegisteredEvent>
{
    private readonly ILogger<PatientRegisteredEventHandler> _logger;

    public PatientRegisteredEventHandler(ILogger<PatientRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(PatientRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Patient registered: {PatientId}, Email: {Email}, FullName: {FullName}",
            domainEvent.PatientId,
            domainEvent.Email,
            domainEvent.FullName);

        // Example: Send welcome email, create audit log, etc.
        // This is where side effects happen
        
        await Task.CompletedTask;
    }
}
```

**Register Handler**:
```csharp
// In DependencyInjection.cs
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEventHandler<T>

services.AddScoped<IDomainEventHandler<PatientRegisteredEvent>, PatientRegisteredEventHandler>();
```

**Notes**:
- Use `IDomainEventHandler<T>` (not `IEventHandler<T>`)
- Handler is in Infrastructure layer
- Event is from `MedicalCenter.Core.Aggregates.Patients.Events` namespace

**Notes**:
- Handlers are registered in DI container
- Multiple handlers can be registered for the same event
- Handlers can perform side effects (emails, logging, etc.)

---

### Step 14: Update Aggregates to Implement IHasDomainEvents

**Files to Update**:
- `src/MedicalCenter.Core/Aggregates/Patients/Patient.cs`
- `src/MedicalCenter.Core/Aggregates/MedicalRecords/MedicalRecord.cs`
- Other aggregates as needed

**Pattern for Each Aggregate**:
```csharp
using MedicalCenter.Core.Abstractions; // For IAggregateRoot
using MedicalCenter.Core.SharedKernel; // For User
using MedicalCenter.Core.SharedKernel.Events; // For IHasDomainEvents, IDomainEvent

public class Patient : User, IAggregateRoot, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // ... rest of aggregate implementation
}
```

**Alternative**: If using `AggregateBase`:
```csharp
using MedicalCenter.Core.Abstractions; // For AggregateBase
using MedicalCenter.Core.SharedKernel; // For User

public class Patient : User, AggregateBase
{
    // Domain events handled by AggregateBase
    // ... rest of aggregate implementation
}
```

**Notes**:
- Use `IDomainEvent` (not `DomainEvent`) in collections
- `IHasDomainEvents` is from `MedicalCenter.Core.SharedKernel.Events`
- Aggregates are in plural folders: `Patients/`, `MedicalRecords/`

---

### Step 15: Update Tests

**Files to Update**:
- Existing aggregate tests may need updates if aggregates now implement `IHasDomainEvents`

**New Test File** (Optional - for domain event infrastructure):
`tests/MedicalCenter.Core.Tests/SharedKernel/Events/DomainEventTests.cs`

**Test Cases** (if needed):
```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEvent, DomainEventBase
using FluentAssertions;

[Fact]
public void DomainEvent_HasOccurredOnTimestamp()
{
    // Arrange & Act
    var domainEvent = new TestDomainEvent();

    // Assert
    domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}

// Note: IDomainEvent doesn't have Id property - only OccurredOn
// If you need unique IDs, add them to your specific event classes

[Fact]
public void Aggregate_CanAddDomainEvent()
{
    // Arrange
    var aggregate = new TestAggregate();
    var domainEvent = new TestDomainEvent();

    // Act
    aggregate.AddDomainEvent(domainEvent);

    // Assert
    aggregate.DomainEvents.Should().Contain(domainEvent);
}

[Fact]
public void Aggregate_CanClearDomainEvents()
{
    // Arrange
    var aggregate = new TestAggregate();
    aggregate.AddDomainEvent(new TestDomainEvent());

    // Act
    aggregate.ClearDomainEvents();

    // Assert
    aggregate.DomainEvents.Should().BeEmpty();
}
```

**Note**: Tests are only for the Core (domain) layer. Infrastructure and event handler behavior is verified through manual testing.

---

### Step 16: Update Documentation

**Files to Update**:
- `docs/Architecture.md`: Add domain events section
- `docs/ImplementationPlan.md`: Document domain events infrastructure
- `README.md`: Update if architecture overview is documented

**Architecture.md Changes**:
- Add "Domain Events" section explaining:
  - Purpose of domain events
  - How they're raised and dispatched
  - MediatR integration
  - Event handler pattern

---

### Step 17: Verification Checklist

- [ ] Domain event infrastructure verified (already exists in `SharedKernel/Events/`)
  - [ ] `IDomainEvent` interface exists
  - [ ] `DomainEventBase` abstract class exists
  - [ ] `IHasDomainEvents` interface exists
  - [ ] `IDomainEventHandler<T>` interface exists (note: uses `IDomainEventHandler` not `IEventHandler`)
  - [ ] `IEventDispatcher` interface exists
- [ ] MediatR NuGet package added
- [ ] `DomainEventNotification<T>` wrapper created
- [ ] `MediatREventDispatcher` implemented
- [ ] `DomainEventNotificationHandler<T>` implemented
- [ ] Aggregates updated to implement `IHasDomainEvents` (or use `AggregateBase`)
- [ ] `UnitOfWork` updated to dispatch events after save
- [ ] Services registered in DependencyInjection
- [ ] Example domain event created
- [ ] Example event handler created
- [ ] Domain tests updated (if needed)
- [ ] All existing tests still pass
- [ ] Documentation updated
- [ ] Build successful
- [ ] Manual testing: Domain events are raised and dispatched
- [ ] Manual testing: Event handlers are invoked
- [ ] Manual testing: Events are cleared after dispatch

---

## Architecture Overview

### Flow Diagram

```
Aggregate Operation
    ↓
Raise Domain Event (AddDomainEvent)
    ↓
Save Changes (UnitOfWork.SaveChangesAsync)
    ↓
Collect Domain Events from Tracked Entities
    ↓
Commit Transaction
    ↓
Dispatch Events (IEventDispatcher.DispatchAsync)
    ↓
Wrap in MediatR Notification (DomainEventNotification<T>)
    ↓
Publish via MediatR (IMediator.Publish)
    ↓
DomainEventNotificationHandler<T> Receives Notification
    ↓
Resolve IEventHandler<T> Implementations from DI
    ↓
Invoke All Handlers (Task.WhenAll)
    ↓
Clear Events from Aggregates
```

---

## Design Decisions

### Why MediatR?
- **In-Process Messaging**: MediatR provides clean in-process messaging
- **Decoupling**: Handlers are decoupled from dispatcher
- **Multiple Handlers**: Supports multiple handlers per event
- **Pipeline Behaviors**: Can add cross-cutting concerns (logging, validation, etc.)

### Why Wrap in Notification?
- **MediatR Pattern**: MediatR uses `INotification` for publish/subscribe
- **Type Safety**: Generic wrapper maintains type information
- **Flexibility**: Can add MediatR pipeline behaviors if needed

### Why Dispatch After Save?
- **Transaction Safety**: Events only dispatched if transaction succeeds
- **Consistency**: Ensures domain state is persisted before side effects
- **Idempotency**: If save fails, events are not dispatched

### Why Clear Events After Dispatch?
- **Prevent Duplicates**: Prevents events from being dispatched multiple times
- **Memory Management**: Clears events from memory after processing
- **Clean State**: Aggregates return to clean state after dispatch

---

## Example Usage

### Raising a Domain Event

```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IHasDomainEvents
using MedicalCenter.Core.Aggregates.Patients.Events; // For PatientRegisteredEvent

// In Patient aggregate
public static Patient Create(string fullName, string email, string nationalId, DateTime dateOfBirth)
{
    var patient = new Patient(fullName, email, nationalId, dateOfBirth);
    
    // Raise domain event
    patient.AddDomainEvent(new PatientRegisteredEvent(
        patient.Id,
        patient.Email,
        patient.FullName));
    
    return patient;
}
```

### Handling a Domain Event

```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEventHandler<T>

// In Infrastructure layer
public class PatientRegisteredEventHandler : IDomainEventHandler<PatientRegisteredEvent>
{
    public async Task HandleAsync(PatientRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        // Perform side effects: send email, create audit log, etc.
        await SendWelcomeEmail(domainEvent.Email);
        await CreateAuditLog(domainEvent);
    }
}
```

### Registering Handlers

```csharp
using MedicalCenter.Core.SharedKernel.Events; // For IDomainEventHandler<T>

// In DependencyInjection.cs
services.AddScoped<IDomainEventHandler<PatientRegisteredEvent>, PatientRegisteredEventHandler>();
services.AddScoped<IDomainEventHandler<PatientRegisteredEvent>, AuditLogHandler>(); // Multiple handlers
```

---

## Notes

- **Separation of Concerns**: Domain events are domain concepts, MediatR is infrastructure
- **Type Safety**: Generic interfaces maintain type safety throughout the pipeline
- **Flexibility**: Can add multiple handlers per event
- **Testability**: Handlers can be tested independently
- **Performance**: Events dispatched asynchronously, handlers run in parallel
- **Error Handling**: Consider adding error handling strategy for handler failures

---

## Future Enhancements

1. **Event Store**: Store domain events for event sourcing
2. **Outbox Pattern**: Store events in database for reliable delivery
3. **Pipeline Behaviors**: Add MediatR pipeline behaviors for logging, validation
4. **Event Versioning**: Support event versioning for schema evolution
5. **Event Replay**: Support replaying events for debugging/recovery

---

## Post-Implementation

After successful implementation:
1. Remove this plan document (`docs/DomainEventsPlan.md`)
2. Update main documentation files
3. Commit changes with appropriate message
4. Verify all tests pass
5. Perform manual testing
6. Create example domain events for existing aggregates (if needed)

---

**End of Plan**
