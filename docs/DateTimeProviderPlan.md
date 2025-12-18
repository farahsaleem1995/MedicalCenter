# DateTime Provider Implementation Plan

**Status**: Temporary plan - to be removed after implementation  
**Date**: 2025-12-18  
**Objective**: Introduce `IDateTimeProvider` service to provide unified time handling across the application, ensuring consistent UTC time usage and testability.

---

## Overview

This plan introduces a `IDateTimeProvider` service to replace direct usage of `DateTime.UtcNow` and `DateTime.Now` throughout the application. This provides:

1. **Unified Time Source**: Single source of truth for current time
2. **Testability**: Time can be mocked/controlled in tests
3. **Consistency**: Always returns UTC time
4. **Explicit Dependency**: Makes time dependencies explicit in code

### Benefits

- **Testability**: Can control time in unit and integration tests
- **Consistency**: All time operations use UTC from a single source
- **Explicit Dependencies**: Time dependencies are visible through dependency injection
- **Future Flexibility**: Can add time zone support, time adjustments, or other features if needed
- **Follows Best Practices**: Aligns with testing principles (make time explicit, avoid ambient context)

---

## Architecture Decisions

### 1. Interface Location

**Decision**: Place `IDateTimeProvider` in `src/MedicalCenter.Core/Services/`

**Rationale**:
- Core layer defines abstractions
- Services folder is appropriate for cross-cutting concerns
- Consistent with other service interfaces (`IIdentityService`, `ITokenProvider`, `IFileStorageService`)

### 2. Implementation Location

**Decision**: Implement in Infrastructure layer (`src/MedicalCenter.Infrastructure/Services/`)

**Rationale**:
- Infrastructure provides concrete implementations
- Can be swapped with different implementations if needed
- Follows existing pattern (other services implemented in Infrastructure)

**Alternative Consideration**: Could implement in WebApi layer, but Infrastructure is more appropriate as it's a cross-cutting concern that may be used by Infrastructure services.

### 3. Time Zone Strategy

**Decision**: Implementation returns UTC time, but abstraction is generic

**Rationale**:
- **Abstraction Independence**: Interface doesn't specify time zone (follows dependency inversion)
- **Implementation Flexibility**: Implementation can return UTC (current) or be extended later
- **Consistency**: All timestamps in database should be UTC
- **Simplicity**: No time zone conversion complexity in current implementation
- **Best Practice**: UTC is standard for server-side applications
- **Database Storage**: EF Core and SQL Server work well with UTC

**Future Enhancement**: If time zone support is needed, can extend interface with time zone-aware methods or create different implementations.

### 4. Interface Design

**Decision**: Simple interface with `Now` property (generic, no time zone specified)

**Rationale**:
- **Generic Abstraction**: Interface doesn't specify time zone (abstraction independence)
- **Simplicity**: Minimal interface, easy to understand
- **Single Responsibility**: Only provides current time
- **Testability**: Easy to mock in tests
- **Performance**: Property access is fast (no method call overhead)
- **Flexibility**: Implementation decides time zone (currently UTC, can be extended)

**Design**:
```csharp
public interface IDateTimeProvider
{
    DateTime Now { get; }
}
```

**Implementation Note**: The `DateTimeProvider` implementation will return UTC time (`DateTime.UtcNow`), but the interface doesn't specify this, allowing for future flexibility.

**Alternative Considered**: 
- `UtcNow` property - **Rejected**: Ties abstraction to UTC, violates abstraction independence
- Method `GetNow()` instead of property - **Rejected**: Property is more natural for time access

---

## Implementation Steps

### Step 1: Create IDateTimeProvider Interface

**File**: `src/MedicalCenter.Core/Services/IDateTimeProvider.cs`

**Design**:
```csharp
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
```

**Notes**:
- Simple property-based interface
- Generic abstraction (no time zone specified)
- Implementation will return UTC time
- XML documentation included
- Follows existing service interface patterns

### Step 2: Implement DateTimeProvider

**File**: `src/MedicalCenter.Infrastructure/Services/DateTimeProvider.cs`

**Design**:
```csharp
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
```

**Notes**:
- Simple implementation wrapping `DateTime.UtcNow`
- No state, thread-safe
- Can be registered as singleton (no instance state)

### Step 3: Register Service in Dependency Injection

**File**: `src/MedicalCenter.Infrastructure/DependencyInjection.cs`

**Add**:
```csharp
// Register DateTimeProvider as singleton (stateless, thread-safe)
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

**Notes**:
- Singleton registration (stateless, thread-safe)
- Registered in Infrastructure layer (where implementation lives)
- Available throughout application via DI

### Step 4: Update AuditableEntityInterceptor

**File**: `src/MedicalCenter.Infrastructure/Data/Interceptors/AuditableEntityInterceptor.cs`

**Current Code**:
```csharp
var utcNow = DateTime.UtcNow;
```

**Updated Code**:
```csharp
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }
    
    // ... existing methods ...
    
    private void UpdateAuditableEntities(DbContext? context)
    {
        // ... existing code ...
        
        var utcNow = _dateTimeProvider.Now;
        
        // ... rest of method ...
    }
}
```

**Notes**:
- Inject `IDateTimeProvider` via constructor
- Replace `DateTime.UtcNow` with `_dateTimeProvider.UtcNow`
- Interceptor is registered in DI, so can receive dependencies

**Registration Update** (if needed):
```csharp
// In DependencyInjection.cs
services.AddSingleton<AuditableEntityInterceptor>();
// Then add to DbContext options
```

### Step 5: Update TokenProvider

**File**: `src/MedicalCenter.Infrastructure/Services/TokenProvider.cs`

**Current Code**:
```csharp
expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
```

**Updated Code**:
```csharp
public class TokenProvider : ITokenProvider
{
    private readonly IDateTimeProvider _dateTimeProvider;
    // ... existing fields ...
    
    public TokenProvider(
        IDateTimeProvider dateTimeProvider,
        // ... existing parameters ...)
    {
        _dateTimeProvider = dateTimeProvider;
        // ... existing assignments ...
    }
    
    // ... existing methods ...
    
    public string GenerateAccessToken(User user)
    {
        // ... existing code ...
        
        var token = new JwtSecurityToken(
            // ... existing parameters ...
            expires: _dateTimeProvider.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
            // ... rest of parameters ...
        );
        
        // ... rest of method ...
    }
}
```

**Notes**:
- Inject `IDateTimeProvider` via constructor
- Replace `DateTime.UtcNow` with `_dateTimeProvider.UtcNow`
- Service already uses DI, so straightforward update

### Step 6: Update Validators

**File**: `src/MedicalCenter.WebApi/Endpoints/Patients/Surgeries/CreateSurgeryEndpoint.Validator.cs`

**Current Code**:
```csharp
.LessThanOrEqualTo(DateTime.UtcNow)
```

**Updated Code**:
```csharp
public class CreateSurgeryEndpointValidator : Validator<CreateSurgeryRequest>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public CreateSurgeryEndpointValidator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        
        // ... existing rules ...
        
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Surgery date is required.")
            .LessThanOrEqualTo(_dateTimeProvider.Now)
            .WithMessage("Surgery date cannot be in the future.");
    }
}
```

**Notes**:
- FluentValidation validators support constructor injection
- Replace `DateTime.UtcNow` with `_dateTimeProvider.UtcNow`
- Check all validators for `DateTime.UtcNow` usage

**Files to Check**:
- All validator files in `src/MedicalCenter.WebApi/Endpoints/**/*.Validator.cs`
- Search for `DateTime.UtcNow` or `DateTime.Now` usage

### Step 7: Update Audit Trail Service (Future)

**File**: `src/MedicalCenter.Infrastructure/Services/AuditTrailService.cs` (when implementing CQRS plan)

**Note**: When implementing audit trail service, use `IDateTimeProvider` instead of `DateTime.UtcNow`:

```csharp
public class AuditTrailService : IAuditTrailService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    // ... existing fields ...
    
    public AuditTrailService(
        IDateTimeProvider dateTimeProvider,
        // ... existing parameters ...)
    {
        _dateTimeProvider = dateTimeProvider;
        // ... existing assignments ...
    }
    
    public void RecordCommandExecution(...)
    {
        var entry = new AuditTrailEntry
        {
            // ... existing properties ...
            ExecutedAt = _dateTimeProvider.Now
        };
        // ... rest of method ...
    }
}
```

### Step 8: Search and Replace All DateTime.UtcNow Usage

**Approach**: Systematic search and replace

**Command** (for reference):
```bash
# Find all usages
grep -r "DateTime.UtcNow" src/
grep -r "DateTime.Now" src/
```

**Files to Update** (based on search results):
1. ✅ `AuditableEntityInterceptor.cs` (Step 4)
2. ✅ `TokenProvider.cs` (Step 5)
3. ✅ Validators (Step 6)
4. ✅ Audit Trail Service (Step 7 - when implemented)
5. Any other services or infrastructure code

**Pattern to Follow**:
1. Add `IDateTimeProvider` to constructor
2. Store in private field
3. Replace `DateTime.UtcNow` with `_dateTimeProvider.Now`
4. Replace `DateTime.Now` with `_dateTimeProvider.Now` (implementation returns UTC)

**Important**: 
- **Do NOT** update test files - tests can use `DateTime.UtcNow` directly or mock `IDateTimeProvider`
- **Do NOT** update domain entities - they receive time as parameters, don't retrieve it
- **Focus on**: Infrastructure services, interceptors, validators, and application layer code

### Step 9: Update Documentation

**Files to Update**:
- `docs/Architecture.md` - Add section about time handling
- `docs/ImplementationPlan.md` - Document the pattern

**Content to Add**:

**Architecture.md**:
```markdown
### Time Handling

**Pattern**: Use `IDateTimeProvider` for all time access

**Rationale**:
- Ensures consistent UTC time across application
- Makes time dependencies explicit and testable
- Follows testing best practices (avoid ambient context)

**Usage**:
- Inject `IDateTimeProvider` where time is needed
- Use `_dateTimeProvider.Now` instead of `DateTime.UtcNow` or `DateTime.Now`
- Implementation returns UTC time (but abstraction doesn't specify this)

**Implementation**:
- Interface: `MedicalCenter.Core.Services.IDateTimeProvider`
- Implementation: `MedicalCenter.Infrastructure.Services.DateTimeProvider`
- Registered as singleton in DI
```

---

## Migration Strategy

### Phase 1: Add Infrastructure (Non-Breaking)

1. Create `IDateTimeProvider` interface
2. Create `DateTimeProvider` implementation
3. Register in DI
4. **No breaking changes** - existing code continues to work

### Phase 2: Update Existing Code (Incremental)

1. Update `AuditableEntityInterceptor` (high impact - affects all entities)
2. Update `TokenProvider` (security-related)
3. Update validators (one at a time)
4. Update any other services found in search

### Phase 3: Enforce Pattern (Ongoing)

1. Add code review checklist item: "No direct DateTime.UtcNow usage"
2. Consider adding analyzer rule (optional, future enhancement)
3. Update team documentation

---

## Testing Strategy

### Unit Tests

**Scope**: Test `DateTimeProvider` implementation

**Test File**: `tests/MedicalCenter.Core.Tests/Services/DateTimeProviderTests.cs` (if needed)

**Note**: Implementation is trivial (wraps `DateTime.UtcNow`), may not need dedicated tests. However, can test that it returns UTC time:

```csharp
[Fact]
public void Now_ReturnsUtcTime()
{
    // Arrange
    var provider = new DateTimeProvider();
    
    // Act
    var result = provider.Now;
    
    // Assert
    result.Kind.Should().Be(DateTimeKind.Utc);
    result.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}
```

### Integration Tests

**Scope**: Test services that use `IDateTimeProvider`

**Approach**: Mock `IDateTimeProvider` in integration tests to control time

**Example**:
```csharp
// In integration test setup
var mockDateTimeProvider = new Mock<IDateTimeProvider>();
mockDateTimeProvider.Setup(x => x.Now).Returns(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));

services.AddSingleton<IDateTimeProvider>(mockDateTimeProvider.Object);
```

**Benefits**:
- Can test time-dependent logic (token expiration, audit timestamps)
- Can test edge cases (midnight, year boundaries)
- Tests are deterministic (not dependent on actual current time)

### Domain Entity Tests

**Note**: Domain entities should receive time as parameters, not retrieve it directly. Tests can use `DateTime.UtcNow` directly for test data.

**Example** (acceptable in tests):
```csharp
var surgery = Surgery.Create(
    patientId, 
    "Appendectomy", 
    DateTime.UtcNow.AddDays(-100), // Test data - OK to use DateTime.UtcNow
    "Dr. Smith"
);
```

---

## Verification Checklist

- [ ] `IDateTimeProvider` interface created in Core layer
- [ ] `DateTimeProvider` implementation created in Infrastructure layer
- [ ] Service registered in DI (singleton)
- [ ] `AuditableEntityInterceptor` updated to use `IDateTimeProvider`
- [ ] `TokenProvider` updated to use `IDateTimeProvider`
- [ ] All validators updated to use `IDateTimeProvider`
- [ ] All other services updated (search for `DateTime.UtcNow` usage)
- [ ] No direct `DateTime.UtcNow` or `DateTime.Now` usage in production code (except tests)
- [ ] Build successful
- [ ] All tests pass
- [ ] Documentation updated

---

## Code Review Checklist

When reviewing code, ensure:
- ✅ No direct `DateTime.UtcNow` usage (use `IDateTimeProvider`)
- ✅ No direct `DateTime.Now` usage (use `IDateTimeProvider.Now`)
- ✅ `IDateTimeProvider` is injected via constructor
- ✅ Time is passed as parameters to domain methods (not retrieved in domain layer)

---

## Future Enhancements

### 1. Time Zone Support (If Needed)

**Enhancement**: Add time zone-aware methods

**Design**:
```csharp
public interface IDateTimeProvider
{
    DateTime Now { get; }
    
    // Future: Time zone support
    DateTime NowInTimeZone(string timeZoneId);
    DateTime ConvertToTimeZone(DateTime time, string timeZoneId);
}
```

**Note**: Only add if business requirements demand it. UTC is sufficient for most applications.

### 2. Time Adjustment for Testing

**Enhancement**: Allow time adjustment in test scenarios

**Design**:
```csharp
public interface IDateTimeProvider
{
    DateTime Now { get; }
    
    // Future: For testing scenarios
    void SetTime(DateTime fixedTime); // Only in test implementation
}
```

**Note**: Use mocking instead - cleaner separation of concerns.

### 3. Analyzer Rule (Optional)

**Enhancement**: Create Roslyn analyzer to detect direct `DateTime.UtcNow` usage

**Benefit**: Enforces pattern at compile time

**Note**: Low priority - code reviews can catch violations.

---

## Evaluation and Recommendations

### Strengths of This Approach

1. **Testability**: Time can be controlled in tests
2. **Consistency**: Single source of truth for time
3. **Explicit Dependencies**: Time dependencies are visible
4. **Simple Interface**: Easy to understand and use
5. **Follows Best Practices**: Aligns with testing principles

### Concerns and Mitigation

#### 1. Overhead of Dependency Injection

**Concern**: Additional DI registration and constructor parameters

**Evaluation**:
- Minimal overhead (singleton, no instance creation cost)
- Benefits (testability, consistency) outweigh costs
- Common pattern in enterprise applications

**Recommendation**: ✅ Proceed - overhead is negligible

#### 2. Migration Effort

**Concern**: Need to update all existing `DateTime.UtcNow` usage

**Evaluation**:
- Can be done incrementally
- Non-breaking changes (add interface, update gradually)
- Search and replace is straightforward

**Recommendation**: ✅ Proceed - incremental migration is manageable

#### 3. Team Adoption

**Concern**: Developers might forget to use `IDateTimeProvider`

**Evaluation**:
- Code reviews can catch violations
- Documentation and examples help
- Optional analyzer rule can enforce pattern

**Recommendation**: 
- ✅ Proceed with implementation
- Add to code review checklist
- Consider analyzer rule if violations persist

---

## Dependencies

### NuGet Packages

- **None** - Uses built-in .NET types only

### Database Changes

- **None** - No database schema changes

---

## Post-Implementation

After successful implementation:
1. Remove this plan document (`docs/DateTimeProviderPlan.md`)
2. Update main documentation files (`Architecture.md`, `ImplementationPlan.md`)
3. Add to code review checklist
4. Commit changes with appropriate message
5. Verify all tests pass
6. Monitor for any direct `DateTime.UtcNow` usage in new code

---

## References

- **Testing Principles**: "Unit Testing Principles, Practices, and Patterns" by Vladimir Khorikov
  - Chapter on handling time dependencies
  - Principle: Make time explicit, avoid ambient context
- **Clean Architecture**: Dependency injection for cross-cutting concerns
- **DDD**: Domain entities receive time as parameters, don't retrieve it

---

**End of Plan**

