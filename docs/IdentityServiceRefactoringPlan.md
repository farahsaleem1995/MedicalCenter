# IdentityService Refactoring Plan

**Status**: Temporary plan - to be removed after implementation  
**Date**: 2025-12-16  
**Last Updated**: 2025-12-18 (Updated for Core layer reorganization)  
**Objective**: Refactor `IIdentityService` to remove role-specific creation methods. Keep only generic `CreateUserAsync`. Move entity creation and transaction management to endpoints, following the `RegisterPatientEndpoint` pattern.

## Important: Namespace Updates

This plan has been updated to reflect the Core layer reorganization completed on 2025-12-18:

- **`MedicalCenter.Core.Common`** → Split into:
  - `MedicalCenter.Core.Abstractions` (BaseEntity, IAggregateRoot, IAuditableEntity, ValueObject)
  - `MedicalCenter.Core.Primitives` (Result, Error, ErrorCodes, Pagination)
  - `MedicalCenter.Core.SharedKernel` (User, UserRole, IRepository, IUnitOfWork, Attachment)
- **Aggregate namespaces**:
  - `MedicalCenter.Core.Aggregates.Doctors` (Doctor)
  - `MedicalCenter.Core.Aggregates.HealthcareStaff` (HealthcareStaff - renamed from HealthcareEntity)
  - `MedicalCenter.Core.Aggregates.Laboratories` (Laboratory)
  - `MedicalCenter.Core.Aggregates.ImagingCenters` (ImagingCenter)
- **Renamed entity**: `HealthcareEntity` → `HealthcareStaff`

All code examples in this plan use the updated namespaces.

---

## Current State

### Current Implementation
- `IIdentityService` has multiple role-specific methods:
  - `CreateDoctorAsync`
  - `CreateHealthcareStaffAsync` (renamed from `CreateHealthcareEntityAsync`)
  - `CreateLaboratoryAsync`
  - `CreateImagingCenterAsync`
- These methods handle both Identity user creation AND domain entity creation
- `CreateUserEndpoint` uses these methods
- `RegisterPatientEndpoint` handles Identity creation, then creates Patient entity separately

### Issues with Current Approach
1. **Inconsistent Pattern**: `RegisterPatientEndpoint` follows one pattern, `CreateUserEndpoint` follows another
2. **Service Layer Bloat**: `IIdentityService` contains too much logic (entity creation, transaction management)
3. **Violation of SRP**: Identity service should only handle Identity concerns, not domain entity creation
4. **Transaction Management**: Transactions are managed in service layer instead of endpoint layer
5. **Hard to Extend**: Adding new user types requires updating the service interface

---

## Target State

### Service Layer
- `IIdentityService` only has `CreateUserAsync` (generic method)
- Service layer only handles Identity user creation
- No domain entity creation in service layer
- No transaction management in service layer

### Endpoint Layer
- All endpoints handle their own entity creation
- All endpoints manage their own transactions
- Consistent pattern across all user creation endpoints
- Endpoints follow the same pattern as `RegisterPatientEndpoint`

### Pattern
```
1. Begin Transaction
2. Create Identity User (via IIdentityService.CreateUserAsync)
3. Create Domain Entity (via Repository)
4. Save Changes
5. Commit Transaction
```

---

## Implementation Steps

### Step 1: Update IIdentityService Interface

**File**: `src/MedicalCenter.Core/Services/IIdentityService.cs`

**Remove Methods**:
- `CreateDoctorAsync`
- `CreateHealthcareEntityAsync`
- `CreateLaboratoryAsync`
- `CreateImagingCenterAsync`

**Keep Method**:
- `CreateUserAsync` (generic, creates Identity user only)

**Updated Interface**:
```csharp
public interface IIdentityService
{
    /// <summary>
    /// Creates a new Identity user (ApplicationUser) with the specified email and password.
    /// This is a generic method that creates the base Identity user only.
    /// Domain entity creation should be handled by the calling endpoint.
    /// </summary>
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default);

    // ... other methods remain unchanged
}
```

**Notes**:
- Update XML documentation to clarify that domain entity creation is caller's responsibility
- Method only creates `ApplicationUser`, assigns role, returns user ID

---

### Step 2: Update IdentityService Implementation

**File**: `src/MedicalCenter.Infrastructure/Services/IdentityService.cs`

**Remove Methods**:
- `CreateDoctorAsync`
- `CreateHealthcareStaffAsync` (renamed from `CreateHealthcareEntityAsync`)
- `CreateLaboratoryAsync`
- `CreateImagingCenterAsync`

**Update `CreateUserAsync`**:
- Ensure it only creates Identity user
- Does NOT create domain entities
- Does NOT manage transactions
- Returns user ID on success

**Verify Current Implementation**:
- Check that `CreateUserAsync` only handles Identity user creation
- Remove any domain entity creation logic if present
- Remove any transaction management if present

---

### Step 3: Update CreateUserEndpoint

**File**: `src/MedicalCenter.WebApi/Endpoints/Admin/CreateUserEndpoint.cs`

**Current Pattern** (to be replaced):
```csharp
Result<Guid> result = req.Role switch
{
    UserRole.Doctor => await identityService.CreateDoctorAsync(...),
    UserRole.HealthcareStaff => await identityService.CreateHealthcareStaffAsync(...),
    // etc.
};
```

**New Pattern** (following RegisterPatientEndpoint):
```csharp
await unitOfWork.BeginTransactionAsync(ct);

try
{
    // Step 1: Create Identity user
    var createUserResult = await identityService.CreateUserAsync(
        req.Email,
        req.Password,
        req.Role,
        ct);

    if (createUserResult.IsFailure)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        int statusCode = createUserResult.Error!.Code.ToStatusCode();
        ThrowError(createUserResult.Error.Message, statusCode);
        return;
    }

    Guid userId = createUserResult.Value;

    // Step 2: Create domain entity based on role
    User? user = req.Role switch
    {
        UserRole.Doctor => CreateDoctorWithId(req.FullName, req.Email, req.LicenseNumber!, req.Specialty!, userId),
        UserRole.HealthcareStaff => CreateHealthcareStaffWithId(req.FullName, req.Email, req.OrganizationName!, req.Department!, userId),
        UserRole.LabUser => CreateLaboratoryWithId(req.FullName, req.Email, req.LabName!, req.LicenseNumber!, userId),
        UserRole.ImagingUser => CreateImagingCenterWithId(req.FullName, req.Email, req.CenterName!, req.LicenseNumber!, userId),
        _ => throw new InvalidOperationException($"Unsupported role: {req.Role}")
    };

    // Step 3: Add entity to repository
    await userRepository.AddAsync(user, ct);
    await unitOfWork.SaveChangesAsync(ct);

    // Step 4: Commit transaction
    await unitOfWork.CommitTransactionAsync(ct);

    await Send.OkAsync(new CreateUserResponse { UserId = user.Id }, ct);
}
catch
{
    await unitOfWork.RollbackTransactionAsync(ct);
    throw;
}
```

**Add Helper Methods** (similar to `RegisterPatientEndpoint.CreatePatientWithId`):
```csharp
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;

private static Doctor CreateDoctorWithId(string fullName, string email, string licenseNumber, string specialty, Guid id)
{
    var doctor = Doctor.Create(fullName, email, licenseNumber, specialty);
    System.Reflection.PropertyInfo? idProperty = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));
    idProperty?.SetValue(doctor, id);
    return doctor;
}

private static HealthcareStaff CreateHealthcareStaffWithId(string fullName, string email, string organizationName, string department, Guid id)
{
    var staff = HealthcareStaff.Create(fullName, email, organizationName, department);
    System.Reflection.PropertyInfo? idProperty = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));
    idProperty?.SetValue(staff, id);
    return staff;
}

private static Laboratory CreateLaboratoryWithId(string fullName, string email, string labName, string licenseNumber, Guid id)
{
    var lab = Laboratory.Create(fullName, email, labName, licenseNumber);
    System.Reflection.PropertyInfo? idProperty = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));
    idProperty?.SetValue(lab, id);
    return lab;
}

private static ImagingCenter CreateImagingCenterWithId(string fullName, string email, string centerName, string licenseNumber, Guid id)
{
    var center = ImagingCenter.Create(fullName, email, centerName, licenseNumber);
    System.Reflection.PropertyInfo? idProperty = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));
    idProperty?.SetValue(center, id);
    return center;
}
```

**Update Dependencies**:
- Add `IUnitOfWork` dependency (from `MedicalCenter.Core.SharedKernel`)
- Add `IRepository<User>` or specific repositories for each user type (from `MedicalCenter.Core.SharedKernel`)
- Remove dependency on role-specific methods

**Required Using Statements**:
```csharp
using MedicalCenter.Core.SharedKernel; // For IRepository<T>, IUnitOfWork
using MedicalCenter.Core.Abstractions; // For BaseEntity
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
```

**Notes**:
- Follow exact same pattern as `RegisterPatientEndpoint`
- Use reflection to set `Id` property (same approach as Patient)
- Handle all errors and rollback transaction on failure
- `IRepository<T>` and `IUnitOfWork` are now in `MedicalCenter.Core.SharedKernel` namespace

---

### Step 4: Determine Repository Strategy

**Option 1: Use IRepository<User>** (if User is an aggregate root)
- Check if `User` base class implements `IAggregateRoot`
- If yes, can use `IRepository<User>`

**Option 2: Use Specific Repositories**
- `IRepository<Doctor>`
- `IRepository<HealthcareStaff>`
- `IRepository<Laboratory>`
- `IRepository<ImagingCenter>`

**Option 3: Use IRepository<T> with Type Parameter**
- More complex, but type-safe

**Recommendation**: Option 2 (Specific Repositories) - Each aggregate has its own repository, type-safe, follows existing pattern.

**Update Endpoint Dependencies**:
```csharp
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;

public class CreateUserEndpoint(
    IIdentityService identityService,
    IRepository<Doctor> doctorRepository,
    IRepository<HealthcareStaff> healthcareStaffRepository,
    IRepository<Laboratory> laboratoryRepository,
    IRepository<ImagingCenter> imagingCenterRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<CreateUserRequest, CreateUserResponse>
```

**Update Entity Creation**:
```csharp
// Step 2: Create and add domain entity based on role
switch (req.Role)
{
    case UserRole.Doctor:
        var doctor = CreateDoctorWithId(req.FullName, req.Email, req.LicenseNumber!, req.Specialty!, userId);
        await doctorRepository.AddAsync(doctor, ct);
        break;
    case UserRole.HealthcareStaff:
        var healthcareStaff = CreateHealthcareStaffWithId(req.FullName, req.Email, req.OrganizationName!, req.Department!, userId);
        await healthcareStaffRepository.AddAsync(healthcareStaff, ct);
        break;
    case UserRole.LabUser:
        var laboratory = CreateLaboratoryWithId(req.FullName, req.Email, req.LabName!, req.LicenseNumber!, userId);
        await laboratoryRepository.AddAsync(laboratory, ct);
        break;
    case UserRole.ImagingUser:
        var imagingCenter = CreateImagingCenterWithId(req.FullName, req.Email, req.CenterName!, req.LicenseNumber!, userId);
        await imagingCenterRepository.AddAsync(imagingCenter, ct);
        break;
    default:
        await unitOfWork.RollbackTransactionAsync(ct);
        ThrowError($"Unsupported role: {req.Role}", 400);
        return;
}
```

---

### Step 5: Verify No Other Usages

**Search for Usages**:
- Search codebase for calls to removed methods
- Update any tests that use these methods
- Update any documentation that references these methods

**Files to Check**:
- Test files
- Documentation files
- Any other endpoints or services

---

### Step 6: Update Tests

**Files to Update**:
- Tests that mock `IIdentityService` with removed methods
- Tests that test `CreateUserEndpoint` (if any)
- Integration tests (if any)

**Test Updates**:
- Remove mocks for removed methods
- Update tests to only mock `CreateUserAsync`
- Update endpoint tests to verify new pattern

**Note**: Tests are only for the Core (domain) layer. Endpoint behavior is verified through manual testing.

---

### Step 7: Update Documentation

**Files to Update**:
- `docs/Features.md`: Update CreateUser endpoint documentation
- `docs/Architecture.md`: Update IdentityService description
- `docs/ImplementationPlan.md`: Document the refactoring

**Documentation Changes**:
- Clarify that `IIdentityService` only handles Identity user creation
- Document that endpoints are responsible for domain entity creation
- Update endpoint examples to show new pattern

---

### Step 8: Verification Checklist

- [ ] `IIdentityService` interface updated (removed role-specific methods)
- [ ] `IdentityService` implementation updated (removed role-specific methods)
- [ ] `CreateUserEndpoint` updated to follow `RegisterPatientEndpoint` pattern
- [ ] Transaction management moved to endpoint
- [ ] Entity creation moved to endpoint
- [ ] Helper methods added for entity creation with ID
- [ ] Repositories injected correctly
- [ ] All usages of removed methods updated
- [ ] Tests updated (if any)
- [ ] Documentation updated
- [ ] Build successful
- [ ] All existing tests still pass
- [ ] Manual testing: CreateUser endpoint works for all user types
- [ ] Manual testing: Transaction rollback works on errors
- [ ] Manual testing: Pattern is consistent with RegisterPatientEndpoint

---

## Code Comparison

### Before (Inconsistent Pattern)

**RegisterPatientEndpoint**:
```csharp
// Handles transaction, creates Identity, creates Patient entity
await unitOfWork.BeginTransactionAsync(ct);
var createUserResult = await identityService.CreateUserAsync(...);
var patient = CreatePatientWithId(...);
await patientRepository.AddAsync(patient, ct);
await unitOfWork.SaveChangesAsync(ct);
await unitOfWork.CommitTransactionAsync(ct);
```

**CreateUserEndpoint**:
```csharp
// Delegates to service method that handles everything
var result = await identityService.CreateDoctorAsync(...);
// Service handles transaction, Identity, and entity creation
```

### After (Consistent Pattern)

**RegisterPatientEndpoint** (unchanged):
```csharp
// Handles transaction, creates Identity, creates Patient entity
await unitOfWork.BeginTransactionAsync(ct);
var createUserResult = await identityService.CreateUserAsync(...);
var patient = CreatePatientWithId(...);
await patientRepository.AddAsync(patient, ct);
await unitOfWork.SaveChangesAsync(ct);
await unitOfWork.CommitTransactionAsync(ct);
```

**CreateUserEndpoint** (updated to match):
```csharp
// Handles transaction, creates Identity, creates domain entity
await unitOfWork.BeginTransactionAsync(ct);
var createUserResult = await identityService.CreateUserAsync(...);
var user = CreateUserWithId(...); // Based on role
await userRepository.AddAsync(user, ct);
await unitOfWork.SaveChangesAsync(ct);
await unitOfWork.CommitTransactionAsync(ct);
```

---

## Benefits

1. **Consistency**: All endpoints follow the same pattern
2. **Separation of Concerns**: Identity service only handles Identity, endpoints handle domain logic
3. **Single Responsibility**: Each layer has clear responsibilities
4. **Easier to Extend**: Adding new user types only requires endpoint changes
5. **Better Testability**: Endpoints can be tested independently
6. **Transaction Control**: Endpoints have full control over transaction boundaries

---

## Migration Strategy

### Step-by-Step Migration

1. **Update Interface First**: Remove methods from `IIdentityService` interface
2. **Update Implementation**: Remove methods from `IdentityService` implementation
3. **Update Endpoint**: Refactor `CreateUserEndpoint` to new pattern
4. **Test**: Verify endpoint works correctly
5. **Clean Up**: Remove any unused code, update documentation

### Backward Compatibility

- **Breaking Change**: This is a breaking change for any code using the removed methods
- **Scope**: Only affects `CreateUserEndpoint` and any tests
- **Impact**: Low - only one endpoint uses these methods

---

## Notes

- **Pattern Consistency**: All user creation endpoints now follow the same pattern
- **Service Layer Simplification**: `IIdentityService` becomes simpler and focused
- **Endpoint Responsibility**: Endpoints are responsible for orchestrating the creation process
- **Transaction Management**: Transactions are managed at the endpoint level (application layer)
- **Domain Entity Creation**: Domain entities are created in the endpoint, not the service

---

## Post-Implementation

After successful implementation:
1. Remove this plan document (`docs/IdentityServiceRefactoringPlan.md`)
2. Update main documentation files
3. Commit changes with appropriate message
4. Verify all tests pass
5. Perform manual testing

---

**End of Plan**
