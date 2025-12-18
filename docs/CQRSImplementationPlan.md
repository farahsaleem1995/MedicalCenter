# CQRS Implementation Plan - Commands, Queries, Transactions, Audit Trail, and Performance Monitoring

**Status**: Temporary plan - to be removed after implementation  
**Date**: 2025-12-18  
**Objective**: Introduce CQRS concepts (Command/Query separation) with automatic transaction management, audit trail, and performance monitoring using FastEndpoints pipeline processors.

---

## Overview

This plan introduces Command Query Responsibility Segregation (CQRS) concepts to the Medical Center API by:

1. **Phase 1**: Adding `[Command]` and `[Query]` attributes to endpoints to explicitly mark their intent
2. **Phase 2**: Simplifying `IUnitOfWork` and implementing automatic transaction management using `TransactionScope`
3. **Phase 3**: Adding audit trail service to track all command executions
4. **Phase 4**: Adding performance monitoring for queries

### Benefits

- **Explicit Intent**: Clear distinction between read and write operations
- **Automatic Transaction Management**: Commands are transactional by default, reducing boilerplate
- **Audit Trail**: Automatic tracking of all state-changing operations
- **Performance Visibility**: Automatic performance monitoring for queries
- **Simplified Code**: Less manual transaction management in endpoints

---

## Architecture Decisions

### 1. Command vs Query Separation

**Decision**: Use attributes to mark endpoints as Commands or Queries

**Rationale**:
- FastEndpoints doesn't have built-in CQRS support
- Attributes provide declarative configuration
- Easy to apply to existing endpoints
- Can be discovered via reflection for pipeline configuration

**Implementation**:
- `[Command]` attribute for endpoints that modify state (POST, PUT, DELETE, PATCH)
- `[Query]` attribute for endpoints that only read data (GET)
- Attributes stored in metadata for pipeline processors to access

### 2. Transaction Management Strategy

**Decision**: Use `System.Transactions.TransactionScope` instead of EF Core transactions

**Rationale**:
- **Distributed Transaction Support**: `TransactionScope` supports distributed transactions (MSDTC) if needed
- **Automatic Rollback**: Exceptions automatically trigger rollback
- **Simpler API**: No need for explicit Begin/Commit/Rollback calls
- **Cross-Database Support**: Works across multiple databases if needed in future
- **Simplified IUnitOfWork**: Removes transaction management from UnitOfWork, focusing on persistence only

**Trade-offs**:
- **Performance**: Slight overhead compared to EF Core transactions (usually negligible)
- **MSDTC Dependency**: Distributed transactions require MSDTC (can be disabled for single-database scenarios)
- **Async Considerations**: Use `TransactionScopeAsyncFlowOption.Enabled` for async operations

### 3. Audit Trail Design

**Decision**: Create `IAuditTrailService` in Core layer, implement in Infrastructure with its own consistency boundaries

**Rationale**:
- **Separation of Concerns**: Audit trail is a cross-cutting infrastructure concern, not domain logic
- **Independent Consistency**: Audit trail has its own consistency boundary (saves directly, not via UoW)
- **Abstraction**: Core layer defines interface, Infrastructure provides implementation
- **Flexibility**: Can swap implementations (database, file, external service)
- **Testability**: Interface allows easy mocking in tests
- **Not a Domain Aggregate**: Audit trail is operational/infrastructure data, not a domain concept

**Audit Trail Data**:
- Command name/type
- User ID (from JWT claims)
- Timestamp
- Request data (serialized, with sensitive data filtering)
- Response status
- Execution duration
- IP address (if available)

**Consistency Boundary**:
- Audit trail saves directly to database (not through UnitOfWork)
- Independent of main transaction (audit trail persists even if main transaction rolls back)
- Similar to how Identity service manages its own persistence

### 4. Performance Monitoring

**Decision**: Simple timing-based performance monitoring for queries

**Rationale**:
- **Low Overhead**: Minimal performance impact
- **Actionable**: Logs slow queries for investigation
- **Non-Intrusive**: Doesn't affect query execution
- **Configurable**: Can set threshold for warning vs information

---

## Phase 1: Command and Query Attributes

### Objective

Add `[Command]` and `[Query]` attributes to mark endpoints and configure their behavior.

### Implementation Steps

#### Step 1.1: Create Command Attribute

**File**: `src/MedicalCenter.WebApi/Attributes/CommandAttribute.cs`

**Design**:
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public bool IsTransactional { get; set; } = true;  // Default: transactional
    public bool IsTraceable { get; set; } = true;      // Default: auditable
    
    public CommandAttribute() { }
}
```

**Notes**:
- Applied to endpoint classes
- Defaults to transactional and traceable
- Can be disabled: `[Command(IsTransactional = false, IsTraceable = false)]`
- Used by pipeline processors to configure behavior

#### Step 1.2: Create Query Attribute

**File**: `src/MedicalCenter.WebApi/Attributes/QueryAttribute.cs`

**Design**:
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class QueryAttribute : Attribute
{
    public QueryAttribute() { }
}
```

**Notes**:
- Applied to query endpoints (read-only)
- No configuration options needed (queries are never transactional or traceable)
- Used by performance monitoring processor

#### Step 1.3: Create Attribute Discovery Helper

**File**: `src/MedicalCenter.WebApi/Extensions/EndpointExtensions.cs`

**Purpose**: Helper methods to check if endpoint has Command/Query attribute

**Methods**:
- `IsCommandEndpoint(Type endpointType) -> bool`
- `IsQueryEndpoint(Type endpointType) -> bool`
- `GetCommandAttribute(Type endpointType) -> CommandAttribute?`
- `GetQueryAttribute(Type endpointType) -> QueryAttribute?`

**Notes**:
- Used by pipeline processors to determine endpoint type
- Caches reflection results for performance

#### Step 1.4: Apply Attributes to Existing Endpoints

**Files to Update**: All endpoint files

**Pattern**:
- **Commands**: `[Command]` on POST, PUT, DELETE, PATCH endpoints
- **Queries**: `[Query]` on GET endpoints

**Examples**:
```csharp
[Command]
public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    // ...
}

[Query]
public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    // ...
}
```

**Migration Strategy**:
- Apply attributes incrementally
- Start with a few endpoints to validate approach
- Use find/replace patterns where possible
- Verify no breaking changes

### Verification Checklist

- [ ] `CommandAttribute` created with `IsTransactional` and `IsTraceable` properties
- [ ] `QueryAttribute` created
- [ ] `EndpointExtensions` helper methods created
- [ ] Attributes applied to all endpoints
- [ ] Build successful
- [ ] No breaking changes

---

## Phase 2: Transaction Management with TransactionScope

### Objective

Simplify `IUnitOfWork` by removing transaction methods and implementing automatic transaction management using `TransactionScope` via FastEndpoints PreProcessor.

### Implementation Steps

#### Step 2.1: Update IUnitOfWork Interface

**File**: `src/MedicalCenter.Core/SharedKernel/IUnitOfWork.cs`

**Changes**:
- Remove: `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`
- Keep: `SaveChangesAsync` only

**Updated Interface**:
```csharp
namespace MedicalCenter.Core.SharedKernel;

/// <summary>
/// Unit of Work interface for managing database persistence.
/// Transaction management is handled by the application layer using TransactionScope.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Notes**:
- Simplified interface focuses on persistence only
- Transaction management moved to application layer (FastEndpoints pipeline)
- Breaking change - all endpoint code using transaction methods must be updated

#### Step 2.2: Update UnitOfWork Implementation

**File**: `src/MedicalCenter.Infrastructure/Repositories/UnitOfWork.cs`

**Changes**:
- Remove transaction-related fields and methods
- Keep only `SaveChangesAsync` implementation
- Remove `IDbContextTransaction` usage

**Updated Implementation**:
```csharp
// Remove: _transaction field
// Remove: BeginTransactionAsync, CommitTransactionAsync, RollbackTransactionAsync methods
// Keep: SaveChangesAsync (calls _context.SaveChangesAsync)
```

**Notes**:
- Much simpler implementation
- No transaction state to manage
- EF Core handles connection management

#### Step 2.3: Create Transaction PreProcessor

**File**: `src/MedicalCenter.WebApi/Processors/TransactionPreProcessor.cs`

**Implementation Approach**:
```csharp
using System.Transactions;
using FastEndpoints;

public class TransactionPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        // Check if endpoint has [Command] attribute
        var endpointType = ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType;
        if (endpointType == null) return;
        
        var commandAttr = endpointType.GetCustomAttribute<CommandAttribute>();
        if (commandAttr == null || !commandAttr.IsTransactional) return;
        
        // Create TransactionScope with async flow enabled
        var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(30) // Configurable
            },
            TransactionScopeAsyncFlowOption.Enabled
        );
        
        // Store in HttpContext.Items for cleanup in PostProcessor
        ctx.HttpContext.Items["TransactionScope"] = transactionScope;
        
        // CRITICAL: Register cleanup callback to ensure disposal even if PostProcessor isn't called
        // This handles cases where exceptions occur before endpoint execution or connection is aborted
        ctx.HttpContext.Response.OnCompleted(() =>
        {
            if (ctx.HttpContext.Items.TryGetValue("TransactionScope", out var scopeObj) 
                && scopeObj is TransactionScope scope)
            {
                try
                {
                    scope.Dispose();
                }
                catch
                {
                    // Ignore disposal errors - scope may already be disposed
                }
            }
            return Task.CompletedTask;
        });
    }
}
```

**Notes**:
- Only applies to Command endpoints with `IsTransactional = true`
- Uses `TransactionScopeAsyncFlowOption.Enabled` for async support
- Stores scope in `HttpContext.Items` for cleanup
- **CRITICAL**: Registers `OnCompleted` callback as safety net for disposal
- `OnCompleted` ensures disposal even if PostProcessor isn't called (e.g., connection abort, early exception)

#### Step 2.4: Create Transaction PostProcessor

**File**: `src/MedicalCenter.WebApi/Processors/TransactionPostProcessor.cs`

**Implementation Approach**:
```csharp
using System.Transactions;
using FastEndpoints;
using Microsoft.Extensions.Logging;

public class TransactionPostProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    private readonly ILogger<TransactionPostProcessor<TRequest, TResponse>> _logger;
    
    public TransactionPostProcessor(ILogger<TransactionPostProcessor<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    public async Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> ctx, CancellationToken ct)
    {
        if (!ctx.HttpContext.Items.TryGetValue("TransactionScope", out var scopeObj))
            return;
        
        if (scopeObj is not TransactionScope transactionScope)
            return;
        
        // Remove from Items to prevent double disposal via OnCompleted callback
        ctx.HttpContext.Items.Remove("TransactionScope");
        
        try
        {
            if (ctx.HttpContext.Response.StatusCode >= 200 && ctx.HttpContext.Response.StatusCode < 300)
            {
                // Success - complete transaction
                transactionScope.Complete();
            }
            // Failure - transaction will be rolled back automatically on Dispose()
        }
        catch (Exception ex)
        {
            // Log but don't throw - disposal must happen
            _logger.LogError(ex, "Error completing transaction scope");
        }
        finally
        {
            try
            {
                transactionScope.Dispose();
            }
            catch (Exception ex)
            {
                // Log disposal errors but don't throw
                // TransactionScope.Dispose() should not throw, but defensive programming
                _logger.LogWarning(ex, "Error disposing transaction scope");
            }
        }
    }
}
```

**Notes**:
- **Safe Disposal**: Uses try-finally to ensure disposal even if `Complete()` throws
- **Double Disposal Prevention**: Removes scope from `HttpContext.Items` to prevent disposal by `OnCompleted` callback
- **Error Handling**: Catches and logs exceptions during completion/disposal without throwing
- **Transaction Behavior**: 
  - If `Complete()` is called: Transaction commits
  - If `Complete()` is NOT called: Transaction rolls back automatically on `Dispose()`
  - If exception occurs: Transaction rolls back automatically

**Notes**:
- Completes transaction on success (2xx status codes)
- Automatically rolls back on exceptions or error status codes
- Always disposes scope in finally block
- Works with both success and failure scenarios

#### Step 2.5: Register Processors in FastEndpoints

**File**: `src/MedicalCenter.WebApi/Program.cs`

**Changes**:
```csharp
builder.Services
    .AddFastEndpoints()
    .AddPreProcessor<TransactionPreProcessor<object>>()  // Global pre-processor
    .AddPostProcessor<TransactionPostProcessor<object, object>>()  // Global post-processor
    .SwaggerDocument(o => { /* ... */ });
```

**Notes**:
- Global processors apply to all endpoints
- FastEndpoints will filter based on endpoint type
- Generic `<object>` allows matching any request/response type

#### Step 2.6: Update All Endpoints to Remove Manual Transaction Management

**Files to Update**: All endpoints that use `IUnitOfWork` transaction methods

**Pattern to Remove**:
```csharp
// OLD - Remove this pattern
await unitOfWork.BeginTransactionAsync(ct);
try
{
    // ... endpoint logic ...
    await unitOfWork.SaveChangesAsync(ct);
    await unitOfWork.CommitTransactionAsync(ct);
}
catch
{
    await unitOfWork.RollbackTransactionAsync(ct);
    throw;
}
```

**Pattern to Keep**:
```csharp
// NEW - Just save changes, transaction handled automatically
// ... endpoint logic ...
await unitOfWork.SaveChangesAsync(ct);
// Transaction committed automatically on success
```

**Files Affected**:
- `RegisterPatientEndpoint.cs`
- `CreateUserEndpoint.cs`
- `CreateRecordEndpoint.cs`
- `UpdateRecordEndpoint.cs`
- `DeleteRecordEndpoint.cs`
- All other command endpoints

**Migration Strategy**:
1. Update one endpoint at a time
2. Test thoroughly after each change
3. Use find/replace for common patterns
4. Verify transaction behavior (rollback on errors)

### Verification Checklist

- [ ] `IUnitOfWork` updated (transaction methods removed)
- [ ] `UnitOfWork` implementation updated
- [ ] `TransactionPreProcessor` created with `OnCompleted` callback for safe disposal
- [ ] `TransactionPostProcessor` created with proper error handling
- [ ] Processors registered in `Program.cs`
- [ ] All endpoints updated to remove manual transaction management
- [ ] Build successful
- [ ] Manual testing: Commands are transactional
- [ ] Manual testing: Transactions rollback on errors
- [ ] Manual testing: Queries are NOT transactional
- [ ] Manual testing: Commands with `IsTransactional = false` work correctly
- [ ] **Safety Testing**: Verify disposal on connection abort
- [ ] **Safety Testing**: Verify disposal on early exceptions
- [ ] **Safety Testing**: Verify no double disposal occurs

---

## Phase 3: Audit Trail Service

### Objective

Add audit trail service to automatically track all command executions.

### Implementation Steps

#### Step 3.1: Create IAuditTrailService Interface

**File**: `src/MedicalCenter.Core/Services/IAuditTrailService.cs`

**Design**:
```csharp
namespace MedicalCenter.Core.Services;

/// <summary>
/// Service for recording and querying audit trail entries for command executions.
/// Audit trail is a cross-cutting infrastructure concern with its own consistency boundaries.
/// </summary>
public interface IAuditTrailService
{
    /// <summary>
    /// Records an audit trail entry for a command execution.
    /// Saves directly to database (not through UnitOfWork) to maintain independent consistency boundary.
    /// </summary>
    /// <param name="commandName">Name/type of the command</param>
    /// <param name="userId">ID of the user executing the command</param>
    /// <param name="requestData">Serialized request data (with sensitive data filtered)</param>
    /// <param name="responseStatus">HTTP response status code</param>
    /// <param name="executionDuration">Duration of command execution</param>
    /// <param name="ipAddress">Client IP address (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordCommandExecutionAsync(
        string commandName,
        Guid? userId,
        string? requestData,
        int responseStatus,
        TimeSpan executionDuration,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit trail entries between two dates in descending order (newest first).
    /// Returns maximum number of entries as configured (default: 100).
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit trail entries, ordered by ExecutedAt descending</returns>
    Task<IReadOnlyList<AuditTrailEntryDto>> GetAuditTrailAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
```

**Notes**:
- Interface in Core layer (abstraction)
- `RecordCommandExecutionAsync` returns `Task` (fire-and-forget pattern, doesn't block)
- `GetAuditTrailAsync` returns DTOs (not domain entities - audit trail is infrastructure concern)
- All parameters optional where appropriate
- Can be extended with more metadata if needed

#### Step 3.2: Create AuditTrailEntry DTO

**File**: `src/MedicalCenter.Core/Services/AuditTrailEntryDto.cs`

**Design Decision**: Audit trail is an infrastructure/operational concern, not a domain aggregate

**Rationale**:
- Audit trail is cross-cutting infrastructure data
- Not part of domain model (no business logic)
- Simple DTO for data transfer
- Service manages its own persistence directly

**Design**:
```csharp
namespace MedicalCenter.Core.Services;

/// <summary>
/// Data transfer object for audit trail entries.
/// Audit trail is an infrastructure concern, not a domain aggregate.
/// </summary>
public record AuditTrailEntryDto
{
    public Guid Id { get; init; }
    public string CommandName { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public string? RequestData { get; init; }
    public int ResponseStatus { get; init; }
    public TimeSpan ExecutionDuration { get; init; }
    public string? IpAddress { get; init; }
    public DateTime ExecutedAt { get; init; }
}
```

**Notes**:
- Simple record (immutable DTO)
- Used for querying audit trail entries
- Not a domain entity or aggregate
- Infrastructure concern only

#### Step 3.3: Create AuditTrailEntry Entity (Infrastructure)

**File**: `src/MedicalCenter.Infrastructure/Data/AuditTrailEntry.cs`

**Design**: Simple entity class for EF Core (not a domain aggregate)

**Implementation**:
```csharp
namespace MedicalCenter.Infrastructure.Data;

/// <summary>
/// Entity class for audit trail entries.
/// This is an infrastructure concern, not a domain aggregate.
/// </summary>
public class AuditTrailEntry
{
    public Guid Id { get; set; }
    public string CommandName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? RequestData { get; set; }
    public int ResponseStatus { get; set; }
    public TimeSpan ExecutionDuration { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExecutedAt { get; set; }
}
```

**File**: `src/MedicalCenter.Infrastructure/Data/Configurations/AuditTrailEntryConfiguration.cs`

**Design**:
```csharp
namespace MedicalCenter.Infrastructure.Data.Configurations;

public class AuditTrailEntryConfiguration : IEntityTypeConfiguration<AuditTrailEntry>
{
    public void Configure(EntityTypeBuilder<AuditTrailEntry> builder)
    {
        builder.ToTable("AuditTrailEntries");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.CommandName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.RequestData)
            .HasMaxLength(10000); // Max 10KB
        
        builder.Property(e => e.ExecutionDuration)
            .IsRequired();
        
        builder.Property(e => e.ExecutedAt)
            .IsRequired();
        
        // Indexes for query performance
        builder.HasIndex(e => e.ExecutedAt)
            .IsDescending();
        
        builder.HasIndex(e => new { e.UserId, e.ExecutedAt })
            .IsDescending();
        
        builder.HasIndex(e => new { e.CommandName, e.ExecutedAt })
            .IsDescending();
    }
}
```

**Notes**:
- Table name: `AuditTrailEntries`
- Indexes: `ExecutedAt` (descending), `UserId + ExecutedAt`, `CommandName + ExecutedAt`
- No soft delete (audit trail should never be deleted)
- Simple entity class (not domain aggregate)

#### Step 3.4: Create AuditTrailOptions

**File**: `src/MedicalCenter.Infrastructure/Options/AuditTrailOptions.cs`

**Design**:
```csharp
namespace MedicalCenter.Infrastructure.Options;

/// <summary>
/// Configuration options for audit trail service.
/// </summary>
public class AuditTrailOptions
{
    public const string SectionName = "AuditTrail";
    
    /// <summary>
    /// Maximum number of entries to return when querying audit trail.
    /// Default: 100
    /// </summary>
    public int MaxQueryResults { get; set; } = 100;
}
```

**Configuration in appsettings.json**:
```json
{
  "AuditTrail": {
    "MaxQueryResults": 100
  }
}
```

#### Step 3.5: Implement AuditTrailService

**File**: `src/MedicalCenter.Infrastructure/Services/AuditTrailService.cs`

**Implementation Approach**:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IAuditTrailService.
/// Saves directly to database (not through UnitOfWork) to maintain independent consistency boundary.
/// </summary>
public class AuditTrailService : IAuditTrailService
{
    private readonly MedicalCenterDbContext _context;
    private readonly AuditTrailOptions _options;
    private readonly ILogger<AuditTrailService> _logger;
    
    public AuditTrailService(
        MedicalCenterDbContext context,
        IOptions<AuditTrailOptions> options,
        ILogger<AuditTrailService> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }
    
    public async Task RecordCommandExecutionAsync(
        string commandName,
        Guid? userId,
        string? requestData,
        int responseStatus,
        TimeSpan executionDuration,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = new AuditTrailEntry
            {
                Id = Guid.NewGuid(),
                CommandName = commandName,
                UserId = userId,
                RequestData = requestData,
                ResponseStatus = responseStatus,
                ExecutionDuration = executionDuration,
                IpAddress = ipAddress,
                ExecutedAt = DateTime.UtcNow
            };
            
            // Save directly to database (not through UnitOfWork)
            // This maintains independent consistency boundary
            _context.Set<AuditTrailEntry>().Add(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            _logger.LogError(ex, "Failed to record audit trail entry for command {CommandName}", commandName);
        }
    }
    
    public async Task<IReadOnlyList<AuditTrailEntryDto>> GetAuditTrailAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var entries = await _context.Set<AuditTrailEntry>()
            .Where(e => e.ExecutedAt >= startDate && e.ExecutedAt <= endDate)
            .OrderByDescending(e => e.ExecutedAt)
            .Take(_options.MaxQueryResults)
            .Select(e => new AuditTrailEntryDto
            {
                Id = e.Id,
                CommandName = e.CommandName,
                UserId = e.UserId,
                RequestData = e.RequestData,
                ResponseStatus = e.ResponseStatus,
                ExecutionDuration = e.ExecutionDuration,
                IpAddress = e.IpAddress,
                ExecutedAt = e.ExecutedAt
            })
            .ToListAsync(cancellationToken);
        
        return entries.AsReadOnly();
    }
}
```

**Query Method Details**:
- **Date Range**: `startDate` and `endDate` are inclusive
- **Ordering**: Results ordered by `ExecutedAt` descending (newest first)
- **Limit**: Maximum `MaxQueryResults` entries (default: 100, configurable via Options)
- **Performance**: Uses indexed query on `ExecutedAt` for efficient date range filtering
- **Return Type**: Returns `IReadOnlyList<AuditTrailEntryDto>` (DTOs, not entities)

**Key Design Decisions**:
- **Direct Database Access**: Uses `DbContext.SaveChangesAsync()` directly, NOT through `UnitOfWork`
- **Independent Consistency Boundary**: Audit trail has its own transaction scope, independent of main transaction
- **Not a Domain Aggregate**: Audit trail is infrastructure/operational concern, not domain model
- **Fire-and-forget Pattern**: `RecordCommandExecutionAsync` doesn't block request (PostProcessor doesn't await)
- **Error Handling**: Logs errors but doesn't throw (audit trail failure shouldn't fail the request)
- **Query Method**: `GetAuditTrailAsync` returns entries between dates, ordered by `ExecutedAt` descending, limited by `MaxQueryResults` (default: 100, configurable)
- **Options Pattern**: Uses `AuditTrailOptions` for configuration (`MaxQueryResults`)

#### Step 3.5a: (Enhancement) Implement Real Fire-and-Forget with Background Queue

**Objective**: Implement true fire-and-forget pattern using `BoundedChannel` and a hosted service for better performance and decoupling.

**Rationale**:
- **True Fire-and-Forget**: Request processing doesn't wait for audit trail persistence
- **Better Performance**: Decouples audit trail from request pipeline
- **Handles Bursts**: Queue can buffer entries during high load
- **Similar to Logging**: Follows the same pattern as structured logging (async background processing)
- **Future Batching**: Foundation for batching multiple entries in a single database write

**Architecture**:
1. **Queue Service**: Uses `System.Threading.Channels.BoundedChannel` to queue audit entries
2. **AuditTrailService**: Synchronously enqueues entries (like loggers - non-blocking)
3. **Hosted Service**: Background service that processes queue entries
4. **Scope Management**: Each entry processed in its own scope with fresh DbContext

**Implementation Steps**:

##### Step 3.5a.1: Create AuditTrailQueue Service

**Note**: We use `AuditTrailEntry` directly as the queue item (no separate DTO needed).

**File**: `src/MedicalCenter.Infrastructure/Services/AuditTrailQueue.cs`

```csharp
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Queue service for audit trail entries using BoundedChannel.
/// Provides thread-safe, bounded queue for audit trail processing.
/// </summary>
public interface IAuditTrailQueue
{
    /// <summary>
    /// Enqueues an audit trail entry for background processing.
    /// Non-blocking operation (returns false if queue is full).
    /// </summary>
    bool TryEnqueue(AuditTrailEntry entry);
    
    /// <summary>
    /// Asynchronously reads an audit trail entry from the queue.
    /// </summary>
    ValueTask<AuditTrailEntry?> DequeueAsync(CancellationToken cancellationToken = default);
}

public class AuditTrailQueue : IAuditTrailQueue
{
    private readonly Channel<AuditTrailEntry> _channel;
    private readonly ILogger<AuditTrailQueue> _logger;
    
    public AuditTrailQueue(IOptions<AuditTrailOptions> options, ILogger<AuditTrailQueue> logger)
    {
        _logger = logger;
        
        // Create bounded channel with configurable capacity (default: 1000)
        var capacity = options.Value.QueueCapacity;
        var channelOptions = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite, // Drop oldest when full (prevent blocking)
            SingleReader = true, // Single background service reader
            SingleWriter = false // Multiple writers (concurrent requests)
        };
        
        _channel = Channel.CreateBounded<AuditTrailEntry>(channelOptions);
    }
    
    public bool TryEnqueue(AuditTrailEntry entry)
    {
        try
        {
            return _channel.Writer.TryWrite(entry);
        }
        catch (Exception ex)
        {
            // Log but don't throw - audit trail failure shouldn't fail the request
            _logger.LogWarning(ex, "Failed to enqueue audit trail entry for command {CommandName}", entry.CommandName);
            return false;
        }
    }
    
    public async ValueTask<AuditTrailEntry?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out var item))
                {
                    return item;
                }
            }
            
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
```

**Notes**:
- Uses `BoundedChannel` for thread-safe queue
- Uses `AuditTrailEntry` directly as queue item (no separate DTO needed)
- `FullMode = DropWrite` prevents blocking (drops oldest entry if queue is full)
- `SingleReader = true` for background service
- `SingleWriter = false` for concurrent request handling
- Non-blocking `TryEnqueue` (like logger pattern)

##### Step 3.5a.2: Update AuditTrailOptions

**File**: `src/MedicalCenter.Infrastructure/Options/AuditTrailOptions.cs`

**Add**:
```csharp
/// <summary>
/// Capacity of the audit trail queue.
/// Default: 1000
/// </summary>
public int QueueCapacity { get; set; } = 1000;

/// <summary>
/// Batch size for processing audit trail entries.
/// Default: 1 (process one at a time, can be increased for batching in future)
/// </summary>
public int BatchSize { get; set; } = 1;
```

##### Step 3.5a.3: Update AuditTrailService to Use Queue

**File**: `src/MedicalCenter.Infrastructure/Services/AuditTrailService.cs`

**Updated Implementation**:
```csharp
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IAuditTrailService.
/// Enqueues entries for background processing (true fire-and-forget).
/// </summary>
public class AuditTrailService : IAuditTrailService
{
    private readonly IAuditTrailQueue _queue;
    private readonly ILogger<AuditTrailService> _logger;
    
    public AuditTrailService(
        IAuditTrailQueue queue,
        ILogger<AuditTrailService> logger)
    {
        _queue = queue;
        _logger = logger;
    }
    
    /// <summary>
    /// Enqueues an audit trail entry for background processing.
    /// Non-blocking, synchronous operation (like logger pattern).
    /// </summary>
    public void RecordCommandExecution(
        string commandName,
        Guid? userId,
        string? requestData,
        int responseStatus,
        TimeSpan executionDuration,
        string? ipAddress = null)
    {
        var entry = new AuditTrailEntry
        {
            Id = Guid.NewGuid(),
            CommandName = commandName,
            UserId = userId,
            RequestData = requestData,
            ResponseStatus = responseStatus,
            ExecutionDuration = executionDuration,
            IpAddress = ipAddress,
            ExecutedAt = DateTime.UtcNow
        };
        
        if (!_queue.TryEnqueue(entry))
        {
            // Queue is full - log warning but don't fail request
            _logger.LogWarning(
                "Audit trail queue is full, dropping entry for command {CommandName}",
                commandName);
        }
    }
    
    // Keep async method for backward compatibility (calls sync method)
    public Task RecordCommandExecutionAsync(
        string commandName,
        Guid? userId,
        string? requestData,
        int responseStatus,
        TimeSpan executionDuration,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        RecordCommandExecution(
            commandName,
            userId,
            requestData,
            responseStatus,
            executionDuration,
            ipAddress);
        
        return Task.CompletedTask;
    }
    
    // GetAuditTrailAsync remains the same (uses DbContext directly for queries)
    // ... (same as before)
}
```

**Notes**:
- **Synchronous `RecordCommandExecution`**: Like logger pattern (non-blocking enqueue)
- **Async wrapper**: Maintains interface compatibility
- **Non-blocking**: `TryEnqueue` never blocks the request thread
- **Queue full handling**: Logs warning, doesn't fail request

##### Step 3.5a.4: Update IAuditTrailService Interface

**File**: `src/MedicalCenter.Core/Services/IAuditTrailService.cs`

**Add synchronous method**:
```csharp
/// <summary>
/// Records an audit trail entry for a command execution (synchronous, non-blocking).
/// Enqueues entry for background processing - similar to logger pattern.
/// </summary>
void RecordCommandExecution(
    string commandName,
    Guid? userId,
    string? requestData,
    int responseStatus,
    TimeSpan executionDuration,
    string? ipAddress = null);
```

**Notes**:
- Synchronous method for true fire-and-forget (like `ILogger.Log`)
- Async method remains for backward compatibility

##### Step 3.5a.5: Create AuditTrailBackgroundService

**File**: `src/MedicalCenter.Infrastructure/Services/AuditTrailBackgroundService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Background service that processes audit trail entries from the queue.
/// Creates a scope for each entry to resolve DbContext independently.
/// </summary>
public class AuditTrailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuditTrailQueue _queue;
    private readonly AuditTrailOptions _options;
    private readonly ILogger<AuditTrailBackgroundService> _logger;
    
    public AuditTrailBackgroundService(
        IServiceProvider serviceProvider,
        IAuditTrailQueue queue,
        IOptions<AuditTrailOptions> options,
        ILogger<AuditTrailBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
        _options = options.Value;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit trail background service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Dequeue entry (waits if queue is empty)
                var item = await _queue.DequeueAsync(stoppingToken);
                
                if (item == null)
                    continue;
                
                // Process entry in its own scope
                await ProcessEntryAsync(item, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audit trail entry");
                // Continue processing - don't let one error stop the service
            }
        }
        
        _logger.LogInformation("Audit trail background service stopped");
    }
    
    private async Task ProcessEntryAsync(AuditTrailEntry entry, CancellationToken cancellationToken)
    {
        // Create scope for this entry (independent DbContext)
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        
        try
        {
            // Entry is already created, just add and save
            context.Set<AuditTrailEntry>().Add(entry);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to save audit trail entry for command {CommandName}",
                entry.CommandName);
            // Don't throw - continue processing other entries
        }
    }
}
```

**Notes**:
- **Hosted Service**: Runs in background, independent of request lifecycle
- **Scope per Entry**: Each entry processed in its own scope with fresh DbContext
- **Error Handling**: Logs errors but continues processing (resilient)
- **Cancellation**: Respects cancellation token for graceful shutdown

##### Step 3.5a.6: Update Dependency Injection

**File**: `src/MedicalCenter.Infrastructure/DependencyInjection.cs`

**Add**:
```csharp
// Register queue service (singleton - shared across requests)
services.AddSingleton<IAuditTrailQueue, AuditTrailQueue>();

// Register audit trail service (scoped - but only enqueues, doesn't use DbContext)
services.AddScoped<IAuditTrailService, AuditTrailService>();

// Register background service
services.AddHostedService<AuditTrailBackgroundService>();
```

**Notes**:
- Queue is singleton (shared across all requests)
- AuditTrailService is scoped (but only enqueues, doesn't need DbContext)
- Background service is registered as hosted service

##### Step 3.5a.8: Update PostProcessor to Use Synchronous Method

**File**: `src/MedicalCenter.WebApi/Processors/AuditTrailPostProcessor.cs`

**Update**:
```csharp
// Record audit trail (synchronous, non-blocking - like logger)
_auditTrailService.RecordCommandExecution(
    commandName,
    userId,
    requestData,
    responseStatus,
    executionDuration,
    ipAddress);
```

**Notes**:
- Uses synchronous method (non-blocking enqueue)
- No `await` needed - true fire-and-forget
- Similar to logger pattern

**Future Enhancement: Batching Entries**

**File**: `src/MedicalCenter.Infrastructure/Services/AuditTrailBackgroundService.cs`

**Batching Implementation** (when `BatchSize > 1`):
```csharp
private async Task ProcessBatchAsync(CancellationToken cancellationToken)
{
    var batch = new List<AuditTrailEntry>();
    
    // Collect batch of entries
    for (int i = 0; i < _options.BatchSize; i++)
    {
        var entry = await _queue.DequeueAsync(cancellationToken);
        if (entry == null) break;
        batch.Add(entry);
    }
    
    if (batch.Count == 0) return;
    
    // Process batch in single scope
    using var scope = _serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
    
    try
    {
        // Entries are already AuditTrailEntry instances, just add and save
        context.Set<AuditTrailEntry>().AddRange(batch);
        await context.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to save batch of {Count} audit trail entries", batch.Count);
    }
}
```

**Benefits of Batching**:
- **Better Performance**: Single database write for multiple entries
- **Reduced Overhead**: Less scope creation and DbContext instantiation
- **Configurable**: Can adjust batch size based on load

**Configuration**:
```json
{
  "AuditTrail": {
    "MaxQueryResults": 100,
    "QueueCapacity": 1000,
    "BatchSize": 10  // Process 10 entries per batch
  }
}
```

**Benefits of This Approach**:
1. ✅ **True Fire-and-Forget**: Request doesn't wait for audit trail persistence
2. ✅ **Non-Blocking**: Enqueue operation never blocks (like logger pattern)
3. ✅ **Decoupled**: Audit trail processing independent of request lifecycle
4. ✅ **Resilient**: Queue buffers entries during high load
5. ✅ **Scalable**: Can handle bursts without impacting request performance
6. ✅ **Future-Proof**: Foundation for batching and other optimizations
7. ✅ **Similar to Logging**: Follows established pattern (async background processing)

**Trade-offs**:
- **Complexity**: More moving parts (queue, background service)
- **Memory**: Queue consumes memory (bounded to prevent unbounded growth)
- **Potential Loss**: If queue is full, entries may be dropped (logged but not persisted)
- **Startup Dependency**: Background service must start before processing begins

**Recommendation**: ✅ **Implement this enhancement** - The benefits (true fire-and-forget, better performance, decoupling) outweigh the added complexity. This is a production-ready pattern similar to how structured logging works.

#### Step 3.6: Create Audit Trail PostProcessor

**File**: `src/MedicalCenter.WebApi/Processors/AuditTrailPostProcessor.cs`

**Implementation Approach**:
```csharp
public class AuditTrailPostProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly ILogger<AuditTrailPostProcessor<TRequest, TResponse>> _logger;
    
    public async Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> ctx, CancellationToken ct)
    {
        // Check if endpoint has [Command] attribute with IsTraceable = true
        var endpointType = ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType;
        if (endpointType == null) return;
        
        var commandAttr = endpointType.GetCustomAttribute<CommandAttribute>();
        if (commandAttr == null || !commandAttr.IsTraceable) return;
        
        // Extract metadata
        var commandName = endpointType.Name;
        var userId = ExtractUserId(ctx.HttpContext); // From JWT claims
        var requestData = SerializeRequest(ctx.Request, filterSensitive: true);
        var responseStatus = ctx.HttpContext.Response.StatusCode;
        var executionDuration = CalculateDuration(ctx.HttpContext);
        var ipAddress = ExtractIpAddress(ctx.HttpContext);
        
        // Record audit trail (fire-and-forget)
        // Option A: Direct save (async, not awaited)
        // _ = _auditTrailService.RecordCommandExecutionAsync(...);
        
        // Option B: Queue-based (synchronous, non-blocking enqueue - like logger pattern)
        _auditTrailService.RecordCommandExecution(
            commandName,
            userId,
            requestData,
            responseStatus,
            executionDuration,
            ipAddress);
    }
}
```

**Notes**:
- Only processes Command endpoints with `IsTraceable = true`
- Extracts user ID from JWT claims
- Serializes request (filters sensitive data like passwords)
- Calculates execution duration from HttpContext.Items
- **Queue-Based (Recommended)**: Synchronous enqueue (non-blocking, like logger pattern)
- **Direct Save**: Async method not awaited (fire-and-forget)
- Audit trail saves independently (not affected by main transaction rollback)

#### Step 3.7: Add Request Data Serialization Helper

**File**: `src/MedicalCenter.WebApi/Helpers/AuditTrailHelper.cs`

**Purpose**: Serialize request data while filtering sensitive fields

**Sensitive Fields to Filter**:
- `Password`, `PasswordHash`
- `Token`, `RefreshToken`
- `CreditCard`, `SSN`, `NationalId`
- Any field marked with `[Sensitive]` attribute

**Implementation**:
- Use `System.Text.Json` for serialization
- Filter sensitive properties before serialization
- Truncate large payloads (max 10KB per entry)

#### Step 3.6: Register Services

**File**: `src/MedicalCenter.Infrastructure/DependencyInjection.cs`

**Add**:
```csharp
using MedicalCenter.Infrastructure.Options;

// Configure audit trail options
services.Configure<AuditTrailOptions>(configuration.GetSection(AuditTrailOptions.SectionName));

// Register audit trail service
services.AddScoped<IAuditTrailService, AuditTrailService>();
```

**File**: `src/MedicalCenter.WebApi/Program.cs`

**Add**:
```csharp
.AddPostProcessor<AuditTrailPostProcessor<object, object>>()
```

**Notes**:
- Options pattern for configuration
- Service registered as scoped (same lifetime as DbContext)

#### Step 3.9: Create EF Core Migration

**Command**:
```bash
dotnet ef migrations add AddAuditTrailEntries --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
```

**Notes**:
- Creates `AuditTrailEntries` table
- Adds indexes for query performance
- No soft delete (audit trail should never be deleted)

### Verification Checklist

- [ ] `IAuditTrailService` interface created with `RecordCommandExecutionAsync` and `GetAuditTrailAsync` methods
- [ ] `AuditTrailEntryDto` record created in Core layer
- [ ] `AuditTrailEntry` entity class created in Infrastructure layer (not domain aggregate)
- [ ] `AuditTrailOptions` class created with `MaxQueryResults` property
- [ ] EF Core configuration created with proper indexes
- [ ] `AuditTrailService` implementation created (saves directly, not through UoW)
- [ ] **OR** (Enhancement): Queue-based implementation with background service
  - [ ] `IAuditTrailQueue` interface and implementation created (uses `AuditTrailEntry` directly)
  - [ ] `AuditTrailBackgroundService` hosted service created
  - [ ] `AuditTrailService` updated to use queue (synchronous enqueue)
  - [ ] Services registered in DI (queue singleton, background service)
- [ ] `GetAuditTrailAsync` method implemented (returns max 100 entries, descending order)
- [ ] `AuditTrailPostProcessor` created
- [ ] Request serialization helper created (with sensitive data filtering)
- [ ] Options configured in `appsettings.json`
- [ ] Services registered in DI
- [ ] PostProcessor registered in FastEndpoints
- [ ] Migration created and applied
- [ ] Build successful
- [ ] Manual testing: Commands are audited
- [ ] Manual testing: Queries are NOT audited
- [ ] Manual testing: Commands with `IsTraceable = false` are NOT audited
- [ ] Manual testing: Sensitive data is filtered from audit trail
- [ ] Manual testing: `GetAuditTrailAsync` returns entries in descending order
- [ ] Manual testing: `GetAuditTrailAsync` respects `MaxQueryResults` limit
- [ ] Manual testing: Audit trail persists even if main transaction rolls back

---

## Phase 4: Query Performance Monitoring

### Objective

Add simple performance monitoring for query endpoints to log execution time.

### Implementation Steps

#### Step 4.1: Create Performance Monitoring PreProcessor

**File**: `src/MedicalCenter.WebApi/Processors/QueryPerformancePreProcessor.cs`

**Implementation Approach**:
```csharp
public class QueryPerformancePreProcessor<TRequest> : IPreProcessor<TRequest>
{
    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        // Check if endpoint has [Query] attribute
        var endpointType = ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType;
        if (endpointType == null) return;
        
        var queryAttr = endpointType.GetCustomAttribute<QueryAttribute>();
        if (queryAttr == null) return;
        
        // Record start time
        ctx.HttpContext.Items["QueryStartTime"] = DateTime.UtcNow;
    }
}
```

**Notes**:
- Only applies to Query endpoints
- Records start time in HttpContext.Items
- Minimal overhead

#### Step 4.2: Create Performance Monitoring PostProcessor

**File**: `src/MedicalCenter.WebApi/Processors/QueryPerformancePostProcessor.cs`

**Implementation Approach**:
```csharp
public class QueryPerformancePostProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    private readonly ILogger<QueryPerformancePostProcessor<TRequest, TResponse>> _logger;
    private readonly IConfiguration _configuration;
    
    private static readonly TimeSpan DefaultWarningThreshold = TimeSpan.FromSeconds(1);
    
    public async Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> ctx, CancellationToken ct)
    {
        // Check if endpoint has [Query] attribute
        var endpointType = ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType;
        if (endpointType == null) return;
        
        var queryAttr = endpointType.GetCustomAttribute<QueryAttribute>();
        if (queryAttr == null) return;
        
        // Calculate duration
        if (!ctx.HttpContext.Items.TryGetValue("QueryStartTime", out var startTimeObj))
            return;
        
        if (startTimeObj is not DateTime startTime)
            return;
        
        var duration = DateTime.UtcNow - startTime;
        var queryName = endpointType.Name;
        var warningThreshold = _configuration.GetValue<TimeSpan?>("Performance:QueryWarningThreshold") 
            ?? DefaultWarningThreshold;
        
        // Log based on duration
        if (duration >= warningThreshold)
        {
            _logger.LogWarning(
                "Slow query detected: {QueryName} took {Duration}ms",
                queryName,
                duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "Query executed: {QueryName} took {Duration}ms",
                queryName,
                duration.TotalMilliseconds);
        }
    }
}
```

**Notes**:
- Logs Information for normal queries
- Logs Warning for slow queries (configurable threshold)
- Includes query name and duration
- Can be extended with more metrics (memory, database queries, etc.)

#### Step 4.3: Add Configuration

**File**: `appsettings.json`

**Add**:
```json
{
  "Performance": {
    "QueryWarningThreshold": "00:00:01"  // 1 second
  }
}
```

**Notes**:
- Configurable threshold for warnings
- Default: 1 second
- Can be adjusted per environment

#### Step 4.4: Register Processors

**File**: `src/MedicalCenter.WebApi/Program.cs`

**Add**:
```csharp
.AddPreProcessor<QueryPerformancePreProcessor<object>>()
.AddPostProcessor<QueryPerformancePostProcessor<object, object>>()
```

### Verification Checklist

- [ ] `QueryPerformancePreProcessor` created
- [ ] `QueryPerformancePostProcessor` created
- [ ] Configuration added to `appsettings.json`
- [ ] Processors registered in FastEndpoints
- [ ] Build successful
- [ ] Manual testing: Query performance is logged
- [ ] Manual testing: Slow queries log as Warning
- [ ] Manual testing: Fast queries log as Information
- [ ] Manual testing: Commands are NOT monitored

---

## Evaluation and Suggestions

### Strengths of This Approach

1. **Separation of Concerns**: Each phase addresses a distinct concern
2. **Incremental Implementation**: Can be implemented phase by phase
3. **Non-Breaking**: Attributes are additive, existing code continues to work
4. **Flexible**: Can disable features per endpoint via attribute properties
5. **Standard Patterns**: Uses well-known patterns (TransactionScope, PostProcessor)

### Concerns and Recommendations

#### 1. TransactionScope Disposal Safety ⚠️

**Concern**: Ensuring TransactionScope is always disposed, even if PostProcessor isn't called

**Evaluation**: 
- **Risk**: If PostProcessor isn't called (connection abort, early exception), TransactionScope could leak
- **Solution**: Use `HttpContext.Response.OnCompleted` callback as safety net
- **Additional Safety**: Remove scope from Items in PostProcessor to prevent double disposal

**Recommendation**: ✅ Use `OnCompleted` callback + proper error handling in PostProcessor

#### 2. TransactionScope Performance

**Concern**: `TransactionScope` has slight overhead compared to EF Core transactions

**Evaluation**: 
- Overhead is usually negligible (< 1ms)
- Benefits (distributed transaction support, automatic rollback) outweigh costs
- Can be optimized later if needed

**Recommendation**: ✅ Proceed with TransactionScope

#### 2. Audit Trail Performance and Consistency

**Concern**: Audit trail recording could slow down requests or be affected by transaction rollbacks

**Evaluation**:
- **Option A: Direct Save** (Initial Implementation)
  - Saves directly to database (not through UoW), independent consistency boundary
  - Fire-and-forget pattern (doesn't await in PostProcessor)
  - Simple implementation
  - **Drawback**: Still creates DbContext and performs database write during request (even if not awaited)

- **Option B: Queue-Based Background Processing** (Recommended Enhancement)
  - **True Fire-and-Forget**: Request only enqueues entry (non-blocking, like logger pattern)
  - **Better Performance**: Database writes happen in background, don't impact request processing
  - **Decoupled**: Audit trail processing completely independent of request lifecycle
  - **Resilient**: Queue buffers entries during high load
  - **Scalable**: Can handle bursts without impacting request performance
  - **Future-Proof**: Foundation for batching entries
  - **Similar to Logging**: Follows established pattern (async background processing)

**Recommendation**: 
- ✅ **Start with Option B (Queue-Based)** - True fire-and-forget provides better performance and decoupling
- If simpler initial implementation needed, start with Option A, then migrate to Option B
- Query method limited to 100 entries (configurable) to prevent large result sets
- Monitor queue capacity and adjust based on load

#### 3. Sensitive Data Filtering

**Concern**: Need comprehensive sensitive data filtering

**Evaluation**:
- Current design uses attribute-based filtering
- Could miss some sensitive fields
- Need comprehensive list of sensitive field names

**Recommendation**:
- Create comprehensive list of sensitive field patterns
- Consider using `[Sensitive]` attribute on DTOs
- Review and update filtering logic regularly

#### 4. IUnitOfWork Breaking Change

**Concern**: Removing transaction methods is a breaking change

**Evaluation**:
- All endpoints using transactions must be updated
- Significant refactoring effort
- But simplifies codebase long-term

**Recommendation**:
- ✅ Proceed with breaking change
- Update all endpoints in Phase 2
- Document migration guide
- Consider feature flag to ease migration

#### 5. Error Handling in Processors

**Concern**: Processors should not fail requests

**Evaluation**:
- Current design catches exceptions in processors
- Logs errors but doesn't throw
- Good practice for cross-cutting concerns

**Recommendation**: ✅ Current approach is correct

### Additional Suggestions

#### 1. Add Command/Query Validation

**Suggestion**: Validate that all endpoints have either `[Command]` or `[Query]` attribute

**Implementation**: Add validation in `Program.cs` startup:
```csharp
// Validate all endpoints have Command or Query attribute
var endpointTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t.IsSubclassOf(typeof(EndpointBase)));
    
foreach (var endpointType in endpointTypes)
{
    var hasCommand = endpointType.GetCustomAttribute<CommandAttribute>() != null;
    var hasQuery = endpointType.GetCustomAttribute<QueryAttribute>() != null;
    
    if (!hasCommand && !hasQuery)
    {
        throw new InvalidOperationException(
            $"Endpoint {endpointType.Name} must have either [Command] or [Query] attribute");
    }
}
```

**Benefit**: Ensures all endpoints are properly categorized

#### 2. Add Metrics/Telemetry

**Suggestion**: Consider adding application insights or similar for production monitoring

**Implementation**: 
- Add telemetry to performance monitoring
- Track command/query execution times
- Monitor audit trail write performance

**Benefit**: Better observability in production

#### 3. Audit Trail Query Endpoint

**Suggestion**: Add admin endpoint to query audit trail

**Implementation**:
- `GET /admin/audit-trail?startDate={date}&endDate={date}` endpoint
- Uses `IAuditTrailService.GetAuditTrailAsync` method
- Returns maximum 100 entries (configurable via Options)
- Ordered by `ExecutedAt` descending (newest first)
- Admin-only access (requires SystemAdmin role)

**Benefit**: Compliance and debugging

**Note**: Query method already implemented in service, just needs endpoint wrapper

#### 4. Transaction Timeout Configuration

**Suggestion**: Make transaction timeout configurable

**Implementation**:
- Add to `appsettings.json`
- Use in `TransactionPreProcessor`
- Default: 30 seconds

**Benefit**: Flexibility for different environments

#### 5. Command/Query Documentation

**Suggestion**: Update Swagger documentation to show Command/Query classification

**Implementation**:
- Add tags or descriptions in Swagger
- Group by Command/Query in UI

**Benefit**: Better API documentation

---

## Testing Strategy

### Unit Tests

**Scope**: Core layer interfaces only

**Files to Test**:
- `IAuditTrailService` interface (contract tests)
- `AuditTrailEntryDto` record (simple DTO, no business logic to test)

**Note**: Following project conventions, only Core layer is unit tested. Audit trail is infrastructure concern, not domain model.

### Integration Tests

**Scope**: End-to-end testing of pipeline processors

**Test Scenarios**:
1. Command with transaction: Verify transaction commits on success
2. Command with transaction: Verify transaction rolls back on error
3. Command with audit trail: Verify audit entry is created
4. Query performance: Verify performance is logged
5. Command with `IsTransactional = false`: Verify no transaction
6. Command with `IsTraceable = false`: Verify no audit trail

**Note**: Integration tests verify behavior at the edge (endpoints).

### Manual Testing Checklist

- [ ] All commands are transactional by default
- [ ] Transactions rollback on errors
- [ ] Commands with `IsTransactional = false` work correctly
- [ ] All commands are audited by default
- [ ] Commands with `IsTraceable = false` are not audited
- [ ] Sensitive data is filtered from audit trail
- [ ] Query performance is logged
- [ ] Slow queries log as Warning
- [ ] No performance degradation

---

## Migration Strategy

### Phase-by-Phase Migration

1. **Phase 1**: Add attributes (non-breaking, can be done incrementally)
2. **Phase 2**: Update IUnitOfWork and endpoints (breaking change, requires all endpoints updated)
3. **Phase 3**: Add audit trail (non-breaking, additive)
4. **Phase 4**: Add performance monitoring (non-breaking, additive)

### Rollback Plan

- **Phase 1**: Remove attributes (no functional impact)
- **Phase 2**: Revert IUnitOfWork changes, restore transaction methods
- **Phase 3**: Remove audit trail service and processor
- **Phase 4**: Remove performance monitoring processors

### Risk Mitigation

- Implement phases incrementally
- Test thoroughly after each phase
- Keep old code commented during migration
- Use feature flags if needed
- Monitor performance after each phase

---

## Dependencies

### NuGet Packages

- **System.Transactions** (for TransactionScope) - Already included in .NET
- No additional packages required

### Database Changes

- **Phase 3**: New `AuditTrailEntries` table (infrastructure concern, not domain model)
  - Indexes on `ExecutedAt`, `UserId + ExecutedAt`, `CommandName + ExecutedAt` for query performance

---

## Post-Implementation

After successful implementation:
1. Remove this plan document (`docs/CQRSImplementationPlan.md`)
2. Update main documentation files (`Architecture.md`, `Features.md`, `ImplementationPlan.md`)
3. Commit changes with appropriate message
4. Verify all tests pass
5. Perform manual testing
6. Monitor performance in production

---

**End of Plan**

