# SystemAdmin Aggregate Root Implementation Plan

**Status**: Temporary plan - to be removed after implementation  
**Date**: 2025-12-16  
**Last Updated**: 2025-12-18 (Updated for Core layer reorganization and claims-based authorization)  
**Objective**: Model SystemAdmin as an aggregate root with organizational context, consistent with other user types (Doctor, HealthcareStaff, Laboratory, ImagingCenter). Implement database-only claims for privileged admin operations.

## Important: Namespace Updates

This plan has been updated to reflect the Core layer reorganization completed on 2025-12-18:

- **`MedicalCenter.Core.Common`** → Split into:
  - `MedicalCenter.Core.Abstractions` (BaseEntity, IAggregateRoot, IAuditableEntity, ValueObject)
  - `MedicalCenter.Core.Primitives` (Result, Error, ErrorCodes, Pagination)
  - `MedicalCenter.Core.SharedKernel` (User, UserRole, IRepository, IUnitOfWork, Attachment)
- **Aggregate namespaces**:
  - `MedicalCenter.Core.Aggregates.SystemAdmin` (SystemAdmin - should follow pattern: `SystemAdmin/SystemAdmin.cs`)
  - `MedicalCenter.Core.Aggregates.Doctors` (Doctor)
  - `MedicalCenter.Core.Aggregates.HealthcareStaff` (HealthcareStaff - renamed from HealthcareEntity)
  - `MedicalCenter.Core.Aggregates.Laboratories` (Laboratory)
  - `MedicalCenter.Core.Aggregates.ImagingCenters` (ImagingCenter)
- **Renamed entity**: `HealthcareEntity` → `HealthcareStaff`

All code examples in this plan use the updated namespaces.

---

## Current State

### Current Implementation
- SystemAdmin is **not** modeled as a domain aggregate
- SystemAdmin exists only as an `ApplicationUser` with `SystemAdmin` role in Identity
- `UserQueryService` uses a temporary `AdminUserWrapper` class to map `ApplicationUser` to `User`
- `IdentityConfiguration` does not include SystemAdmin relationship
- SystemAdminSeeder only creates Identity user, no domain entity

### Issues with Current Approach
1. **Inconsistent with other user types**: All other users (Doctor, HealthcareStaff, etc.) are aggregate roots
2. **No domain representation**: SystemAdmin doesn't exist as a domain concept
3. **Workaround pattern**: `AdminUserWrapper` is a technical workaround, not a domain model
4. **Query service complexity**: Special handling needed in `UserQueryService.MapToDomainUser`
5. **No IsActive management**: SystemAdmin cannot be deactivated/reactivated through domain model

---

## Target State

### Domain Model
- `SystemAdmin` aggregate root class in `src/MedicalCenter.Core/Aggregates/SystemAdmin/SystemAdmin.cs` (following the pattern of other aggregates like `Doctors/Doctor.cs`, `HealthcareStaff/HealthcareStaff.cs`)
- Inherits from `User` base class (like other user types)
- Implements `IAggregateRoot`
- Has `IsActive` property (inherited from `User`)
- Has `Id` property (inherited from `BaseEntity`)
- **Enhanced Organizational Properties** (instead of generic `EmployeeId`):
  - `CorporateId` (string, required, unique) - HR-assigned staff number within the organization
  - `Department` (string, required) - Organizational unit (e.g., "IT", "Medical Administration")
- Shares primary key with `ApplicationUser` (one-to-one relationship)

### Database Structure
- New `SystemAdmins` table with:
  - `Id` (Guid, PK, FK to AspNetUsers.Id)
  - `CorporateId` (string, required, unique) - Staff number within the organization
  - `Department` (string, required) - Organizational unit
- EF Core configuration in `SystemAdminConfiguration.cs`
- Relationship configured in `IdentityConfiguration.cs`

### Claims-Based Authorization (Database-Only)

**Important Distinction:**
- **Claims** describe WHO the user IS (identity attributes) - stored in `IdentityUserClaims` table
- **Permissions/Policies** describe WHAT the user CAN DO (capability) - derived from claims + roles

**Claims are NOT stored in JWT tokens** (to avoid token size issues). Instead:
- Claims are stored exclusively in `AspNetUserClaims` table
- Claims are verified at runtime via database lookup
- Claims are modeled as `(type, value)` tuples

**Identity Claim Types:**
| Claim Type | Description | Example Values |
|------------|-------------|----------------|
| `AdminTier` | Administrative privilege tier | "Super", "Standard" |
| `Department` | Department affiliation | "IT", "Medical Administration" |
| `Certification` | Professional certifications | "HIPAA", "PHI-Access" |

**Authorization Policies (derived from claims):**
| Policy | Requirement | Purpose |
|--------|-------------|---------|
| `CanManageAdmins` | AdminTier = "Super" | Create/update/delete SystemAdmin accounts |
| `CanViewAuditTrail` | SystemAdmin role OR any AdminTier claim | View audit trail entries |

### IIdentityService Additions (Claims & Policy Verification)

New methods for database-only claims verification:

```csharp
// Claims verification (database lookup, not JWT)
Task<bool> HasClaimAsync(Guid userId, (string Type, string Value) claim, CancellationToken ct);
Task<bool> HasClaimTypeAsync(Guid userId, string claimType, CancellationToken ct);
Task<IReadOnlyCollection<(string Type, string Value)>> GetUserClaimsAsync(Guid userId, CancellationToken ct);
Task<Result> AddClaimAsync(Guid userId, (string Type, string Value) claim, CancellationToken ct);
Task<Result> RemoveClaimAsync(Guid userId, (string Type, string Value) claim, CancellationToken ct);

// Policy verification (evaluates claims + roles)
Task<bool> SatisfiesPolicyAsync(Guid userId, string policyName, CancellationToken ct);
```

### Query Service
- `UserQueryService` updated to query `SystemAdmin` directly
- Remove `AdminUserWrapper` class
- Unified query approach for all user types
- **Read-only access**: SystemAdmin can be queried but not modified through the service

### Seeding
- `SystemAdminSeeder` updated to create both `ApplicationUser` and `SystemAdmin` entity
- **Only way to create SystemAdmin**: SystemAdmin entities can only be created through seeding, not through API endpoints

### API Restrictions
- **No Create Endpoint**: SystemAdmin cannot be created via API (e.g., no `POST /admin/users` for SystemAdmin role)
- **No Update Endpoint**: SystemAdmin cannot be updated via API (e.g., no `PUT /admin/users/{userId}` for SystemAdmin)
- **No Delete Endpoint**: SystemAdmin cannot be deleted/deactivated via API (e.g., no `DELETE /admin/users/{userId}` for SystemAdmin)
- **Read-only**: SystemAdmin can only be queried/listed through admin endpoints

---

## Implementation Steps

### Step 1: Create SystemAdmin Aggregate Root

**File**: `src/MedicalCenter.Core/Aggregates/SystemAdmins/SystemAdmin.cs`

**Implementation**:
```csharp
using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Aggregates.SystemAdmins;

/// <summary>
/// System administrator aggregate root.
/// </summary>
public class SystemAdmin : User, IAggregateRoot
{
    /// <summary>
    /// Unique identifier within the organization (e.g., HR-assigned staff number).
    /// </summary>
    public string CorporateId { get; private set; } = string.Empty;
    
    /// <summary>
    /// The organizational unit or department this admin belongs to.
    /// </summary>
    public string Department { get; private set; } = string.Empty;

    private SystemAdmin() { } // EF Core

    private SystemAdmin(string fullName, string email, string corporateId, string department)
        : base(fullName, email, UserRole.SystemAdmin)
    {
        CorporateId = corporateId;
        Department = department;
    }

    public static SystemAdmin Create(
        string fullName, 
        string email, 
        string corporateId, 
        string department)
    {
        Guard.Against.NullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));
        Guard.Against.NullOrWhiteSpace(corporateId, nameof(corporateId));
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        
        return new SystemAdmin(fullName, email, corporateId, department);
    }

    public void UpdateCorporateId(string corporateId)
    {
        Guard.Against.NullOrWhiteSpace(corporateId, nameof(corporateId));
        CorporateId = corporateId;
    }
    
    public void UpdateDepartment(string department)
    {
        Guard.Against.NullOrWhiteSpace(department, nameof(department));
        Department = department;
    }
}
```

**Required Using Statements**:
```csharp
using MedicalCenter.Core.Abstractions; // For IAggregateRoot
using MedicalCenter.Core.SharedKernel; // For User, UserRole
```

**Notes**:
- **Enhanced organizational properties** instead of generic `EmployeeId`:
  - `CorporateId`: HR-assigned staff number (unique within organization)
  - `Department`: Organizational unit (e.g., "IT", "Medical Administration")
- Follows same pattern as other practitioner aggregates with role-specific properties
- CorporateId must be unique across all SystemAdmin instances
- **CRITICAL BUSINESS RULE**: SystemAdmin cannot be created, updated, or deleted through the API unless the requester has `CanManageAdmins` policy
  - `CanManageAdmins` policy requires `AdminTier = "Super"` claim (database lookup)
  - SystemAdmin entities can also be managed through database seeding
  - This is a security measure to prevent unauthorized admin account creation/modification
- `IAggregateRoot` is now in `MedicalCenter.Core.Abstractions` namespace
- `User` and `UserRole` are now in `MedicalCenter.Core.SharedKernel` namespace

---

### Step 2: Create EF Core Configuration

**File**: `src/MedicalCenter.Infrastructure/Data/Configurations/SystemAdminConfiguration.cs`

**Implementation**:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.SystemAdmins;

namespace MedicalCenter.Infrastructure.Data.Configurations;

public class SystemAdminConfiguration : IEntityTypeConfiguration<SystemAdmin>
{
    public void Configure(EntityTypeBuilder<SystemAdmin> builder)
    {
        builder.ToTable("SystemAdmins");

        // Primary key is shared with ApplicationUser (one-to-one)
        builder.HasKey(sa => sa.Id);

        // Configure properties
        builder.Property(sa => sa.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sa => sa.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sa => sa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Enhanced organizational properties
        builder.Property(sa => sa.CorporateId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(sa => sa.Department)
            .IsRequired()
            .HasMaxLength(200);

        // Global query filter for soft delete
        builder.HasQueryFilter(sa => sa.IsActive);

        // Indexes
        builder.HasIndex(sa => sa.Email)
            .IsUnique();

        builder.HasIndex(sa => sa.CorporateId)
            .IsUnique();
            
        builder.HasIndex(sa => sa.Department);

        builder.HasIndex(sa => new { sa.Id, sa.IsActive });
    }
}
```

**Required Using Statement**:
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;
```

**Notes**:
- Follows same pattern as `DoctorConfiguration`, `HealthcareStaffConfiguration`, etc.
- Includes global query filter for soft delete
- Email and CorporateId indexes for uniqueness
- Department index for query performance

---

### Step 3: Update IdentityConfiguration

**File**: `src/MedicalCenter.Infrastructure/Data/Configurations/IdentityConfiguration.cs`

**Changes**:
- Add SystemAdmin relationship configuration (similar to Doctor, HealthcareEntity, etc.)

**Add after ImagingCenter configuration**:
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;

modelBuilder.Entity<ApplicationUser>()
    .HasOne(u => u.SystemAdmin)
    .WithOne()
    .HasForeignKey<SystemAdmin>(sa => sa.Id)
    .OnDelete(DeleteBehavior.Cascade);
```

---

### Step 4: Update ApplicationUser

**File**: `src/MedicalCenter.Infrastructure/Identity/ApplicationUser.cs`

**Changes**:
- Add navigation property for SystemAdmin

**Add navigation property** (with other user navigation properties):
```csharp
public SystemAdmin? SystemAdmin { get; set; }
```

**Add using statement**:
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;
```

---

### Step 5: Update SystemAdminSeeder (Seeding with Claims)

**File**: `src/MedicalCenter.Infrastructure/Data/Seeders/SystemAdminSeeder.cs`

**Changes**:
- Seed SystemAdmin entity in addition to ApplicationUser
- Use `HasData` for SystemAdmin entity
- **Seed SuperAdmin claim** for the initial admin (enables admin management capability)

**Add after ApplicationUser seeding** (after line 47):
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.Authorization;
using Microsoft.AspNetCore.Identity;

// Seed SystemAdmin domain entity with enhanced organizational properties
var systemAdmin = SystemAdmin.Create(
    "System Administrator",
    AdminEmail,
    "SYS-ADMIN-001",  // CorporateId (HR staff number)
    "IT"              // Department
);

// Set the ID to match the ApplicationUser
typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))?.SetValue(systemAdmin, adminId);
systemAdmin.GetType().GetProperty("CreatedAt")?.SetValue(systemAdmin, DateTime.UtcNow);

modelBuilder.Entity<SystemAdmin>().HasData(new
{
    Id = adminId,
    FullName = "System Administrator",
    Email = AdminEmail,
    CorporateId = "SYS-ADMIN-001",
    Department = "IT",
    Role = UserRole.SystemAdmin,
    IsActive = true,
    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
});

// Seed SuperAdmin claim (stored in database, NOT in JWT token)
// This claim grants the CanManageAdmins policy capability
modelBuilder.Entity<IdentityUserClaim<Guid>>().HasData(new IdentityUserClaim<Guid>
{
    Id = 1, // Unique ID for the claim
    UserId = adminId,
    ClaimType = IdentityClaimTypes.AdminTier,
    ClaimValue = IdentityClaimValues.AdminTier.Super
});
```

**Notes**:
- Use corporate identifiers (e.g., "SYS-ADMIN-001" for CorporateId)
- Department should match organizational structure
- **SuperAdmin claim** is seeded to enable admin management capability
- Claims are stored in `AspNetUserClaims` table, NOT in JWT tokens
- **Super admins can create other SystemAdmin accounts** via API (with proper policy check)
- Non-super admins can only be created through seeding or by super admins

**Add using statements**:
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.Authorization;
using Microsoft.AspNetCore.Identity;
```

---

### Step 6: Update UserQueryService

**File**: `src/MedicalCenter.Infrastructure/Services/UserQueryService.cs`

**Changes**:
1. Remove `AdminUserWrapper` class
2. Update `MapToDomainUser` method to include SystemAdmin

**Update MapToDomainUser method**:
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;

private static User MapToDomainUser(ApplicationUser user)
{
    return (User?)user.Patient
        ?? (User?)user.Doctor
        ?? (User?)user.HealthcareStaff
        ?? (User?)user.Laboratory
        ?? (User?)user.ImagingCenter
        ?? (User?)user.SystemAdmin
        ?? throw new InvalidOperationException($"User {user.Id} has no associated domain entity.");
}
```

**Remove**:
- `AdminUserWrapper` class (lines 68-76)

**Update comment** (line 16):
```csharp
/// Query service for retrieving all user entities
/// (Patient, Doctor, HealthcareStaff, Laboratory, ImagingCenter, and SystemAdmin).
```

---

### Step 7: Update UserQueryableExtensions

**File**: `src/MedicalCenter.Infrastructure/Extensions/UserQueryableExtensions.cs`

**Changes**:
- Remove special handling for SystemAdmin (no longer needed)
- SystemAdmin will be handled like other aggregates

**Review and update**:
- Remove comments about "SystemAdmin users (no domain entity relationship) are always active"
- SystemAdmin now has IsActive property and will be filtered by global query filter

---

### Step 8: Update DependencyInjection

**File**: `src/MedicalCenter.Infrastructure/DependencyInjection.cs`

**Changes**:
- Ensure SystemAdminConfiguration is applied in DbContext

**Verify**:
- `MedicalCenterDbContext.OnModelCreating` should apply all configurations
- SystemAdminConfiguration should be included in configuration application

**Check DbContext**:
```csharp
// In MedicalCenterDbContext.OnModelCreating:
modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedicalCenterDbContext).Assembly);
```

This should automatically pick up `SystemAdminConfiguration` if it's in the same assembly.

---

### Step 8a: Create Identity Claim Types (Database-Only Claims)

**File**: `src/MedicalCenter.Core/Authorization/IdentityClaimTypes.cs`

**Implementation**:
```csharp
namespace MedicalCenter.Core.Authorization;

/// <summary>
/// Defines claim types that describe user identity attributes.
/// Claims answer "WHO is this user?" not "WHAT can they do?"
/// 
/// IMPORTANT: Claims are stored ONLY in the database (AspNetUserClaims table),
/// NOT in JWT tokens. This avoids token size issues since claims are unlimited.
/// </summary>
public static class IdentityClaimTypes
{
    /// <summary>
    /// Administrative tier within the organization.
    /// Values: "Super", "Standard"
    /// </summary>
    public const string AdminTier = "MedicalCenter.AdminTier";
    
    /// <summary>
    /// Department the user belongs to (identity attribute).
    /// Values: e.g., "IT", "Medical Administration", "HR"
    /// </summary>
    public const string Department = "MedicalCenter.Department";
    
    /// <summary>
    /// Professional certifications held by the user.
    /// Values: e.g., "HIPAA", "PHI-Access"
    /// </summary>
    public const string Certification = "MedicalCenter.Certification";
}

/// <summary>
/// Well-known claim values for type safety.
/// </summary>
public static class IdentityClaimValues
{
    public static class AdminTier
    {
        /// <summary>
        /// Super admin - can manage other SystemAdmin accounts.
        /// </summary>
        public const string Super = "Super";
        
        /// <summary>
        /// Standard admin - cannot manage other SystemAdmin accounts.
        /// </summary>
        public const string Standard = "Standard";
    }
}
```

**Notes**:
- Claims describe WHO the user is (identity), not WHAT they can do (permissions)
- Claims are stored in `AspNetUserClaims` table only - **NEVER in JWT tokens**
- This avoids token size bloat since claims can be unlimited
- Claims are verified at runtime via database lookup

---

### Step 8b: Create Authorization Policy Constants

**File**: `src/MedicalCenter.Core/Authorization/ClaimBasedPolicies.cs`

**Implementation**:
```csharp
namespace MedicalCenter.Core.Authorization;

/// <summary>
/// Defines authorization policies that determine what actions users CAN DO.
/// Policies answer "CAN this user do X?" based on their claims and roles.
/// 
/// Policies are evaluated via IIdentityService.SatisfiesPolicyAsync() which
/// performs a database lookup (not JWT claim inspection).
/// </summary>
public static class ClaimBasedPolicies
{
    /// <summary>
    /// Policy: Can manage (create/update/delete) SystemAdmin accounts.
    /// Requirement: AdminTier claim with value "Super"
    /// </summary>
    public const string CanManageAdmins = "CanManageAdmins";
    
    /// <summary>
    /// Policy: Can view audit trail entries.
    /// Requirement: SystemAdmin role OR any AdminTier claim
    /// </summary>
    public const string CanViewAuditTrail = "CanViewAuditTrail";
    
    /// <summary>
    /// Policy: Can access PHI (Protected Health Information).
    /// Requirement: Certification claim "HIPAA" or "PHI-Access"
    /// </summary>
    public const string CanAccessPHI = "CanAccessPHI";
}
```

---

### Step 8c: Add Claims Methods to IIdentityService

**File**: `src/MedicalCenter.Core/Services/IIdentityService.cs`

**Add the following methods**:
```csharp
#region Claims Verification (Database-Only)

/// <summary>
/// Checks if a user has a specific claim (type-value pair) in the database.
/// Claims are stored exclusively in IdentityUserClaims table, never in JWT tokens.
/// </summary>
/// <param name="userId">User ID to check</param>
/// <param name="claim">Claim as (type, value) tuple</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>True if user has the exact claim type-value pair</returns>
Task<bool> HasClaimAsync(
    Guid userId,
    (string Type, string Value) claim,
    CancellationToken cancellationToken = default);

/// <summary>
/// Checks if a user has any claim of the specified type (regardless of value).
/// </summary>
Task<bool> HasClaimTypeAsync(
    Guid userId,
    string claimType,
    CancellationToken cancellationToken = default);

/// <summary>
/// Gets all claims for a user from the database.
/// Returns claims as (type, value) tuples.
/// </summary>
Task<IReadOnlyCollection<(string Type, string Value)>> GetUserClaimsAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

/// <summary>
/// Adds a claim to a user in the database.
/// </summary>
Task<Result> AddClaimAsync(
    Guid userId,
    (string Type, string Value) claim,
    CancellationToken cancellationToken = default);

/// <summary>
/// Removes a claim from a user in the database.
/// </summary>
Task<Result> RemoveClaimAsync(
    Guid userId,
    (string Type, string Value) claim,
    CancellationToken cancellationToken = default);

#endregion

#region Policy Verification

/// <summary>
/// Checks if a user satisfies a named authorization policy.
/// Policies are evaluated by checking user's claims and roles against policy requirements.
/// This performs a DATABASE lookup, not JWT token inspection.
/// </summary>
/// <param name="userId">User ID to check</param>
/// <param name="policyName">Name of the policy (from ClaimBasedPolicies)</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>True if user satisfies all policy requirements</returns>
Task<bool> SatisfiesPolicyAsync(
    Guid userId,
    string policyName,
    CancellationToken cancellationToken = default);

#endregion
```

---

### Step 8d: Implement Claims Methods in IdentityService

**File**: `src/MedicalCenter.Infrastructure/Services/IdentityService.cs`

**Add implementation for the new methods**:
```csharp
using System.Security.Claims;
using MedicalCenter.Core.Authorization;

#region Claims Verification (Database-Only)

public async Task<bool> HasClaimAsync(
    Guid userId,
    (string Type, string Value) claim,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) return false;
    
    var userClaims = await _userManager.GetClaimsAsync(user);
    return userClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value);
}

public async Task<bool> HasClaimTypeAsync(
    Guid userId,
    string claimType,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) return false;
    
    var userClaims = await _userManager.GetClaimsAsync(user);
    return userClaims.Any(c => c.Type == claimType);
}

public async Task<IReadOnlyCollection<(string Type, string Value)>> GetUserClaimsAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) return Array.Empty<(string, string)>();
    
    var userClaims = await _userManager.GetClaimsAsync(user);
    return userClaims.Select(c => (c.Type, c.Value)).ToList().AsReadOnly();
}

public async Task<Result> AddClaimAsync(
    Guid userId,
    (string Type, string Value) claim,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) return Result.Failure(Error.NotFound("User"));
    
    var existingClaims = await _userManager.GetClaimsAsync(user);
    if (existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
        return Result.Failure(Error.Conflict("Claim already exists for this user."));
    
    var result = await _userManager.AddClaimAsync(user, new Claim(claim.Type, claim.Value));
    if (!result.Succeeded)
        return Result.Failure(Error.Validation(string.Join("; ", result.Errors.Select(e => e.Description))));
    
    return Result.Success();
}

public async Task<Result> RemoveClaimAsync(
    Guid userId,
    (string Type, string Value) claim,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) return Result.Failure(Error.NotFound("User"));
    
    var result = await _userManager.RemoveClaimAsync(user, new Claim(claim.Type, claim.Value));
    if (!result.Succeeded)
        return Result.Failure(Error.Validation(string.Join("; ", result.Errors.Select(e => e.Description))));
    
    return Result.Success();
}

#endregion

#region Policy Verification

public async Task<bool> SatisfiesPolicyAsync(
    Guid userId,
    string policyName,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) return false;
    
    var userClaims = await _userManager.GetClaimsAsync(user);
    var userRoles = await _userManager.GetRolesAsync(user);
    
    return policyName switch
    {
        ClaimBasedPolicies.CanManageAdmins => 
            userClaims.Any(c => c.Type == IdentityClaimTypes.AdminTier 
                             && c.Value == IdentityClaimValues.AdminTier.Super),
        
        ClaimBasedPolicies.CanViewAuditTrail => 
            userRoles.Contains(nameof(UserRole.SystemAdmin)) 
            || userClaims.Any(c => c.Type == IdentityClaimTypes.AdminTier),
        
        ClaimBasedPolicies.CanAccessPHI => 
            userClaims.Any(c => c.Type == IdentityClaimTypes.Certification 
                             && (c.Value == "HIPAA" || c.Value == "PHI-Access")),
        
        _ => false // Unknown policy = deny
    };
}

#endregion
```

---

### Step 9: Create EF Core Migration

**Command**:
```bash
dotnet ef migrations add AddSystemAdminAggregate --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
```

**Expected Migration**:
- Creates `SystemAdmins` table
- Adds foreign key relationship to `AspNetUsers`
- Seeds SystemAdmin entity data
- Adds indexes

**Verify Migration**:
- Check that migration includes SystemAdmin seeding
- Verify foreign key relationship is correct
- Ensure indexes are created

---

### Step 10: Update Tests

**Files to Update**:
- `tests/MedicalCenter.Core.Tests/Entities/UserCreationRulesTests.cs` (if exists)
- Any tests that reference `AdminUserWrapper`
- Add tests for SystemAdmin aggregate (domain layer only)

**Create New Test File**:
`tests/MedicalCenter.Core.Tests/Aggregates/SystemAdmin/SystemAdminTests.cs`

**Note**: Tests are only for the Core (domain) layer. API endpoint restrictions are enforced at the application layer but not unit tested.

**Test Cases**:
```csharp
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.SharedKernel;
using FluentAssertions;

[Fact]
public void Creates_SystemAdmin_WithValidInput()
{
    // Arrange & Act
    var admin = SystemAdmin.Create(
        "Admin User", 
        "admin@example.com", 
        "SYS-001", 
        "IT");

    // Assert
    admin.Should().NotBeNull();
    admin.FullName.Should().Be("Admin User");
    admin.Email.Should().Be("admin@example.com");
    admin.CorporateId.Should().Be("SYS-001");
    admin.Department.Should().Be("IT");
    admin.Role.Should().Be(UserRole.SystemAdmin);
    admin.IsActive.Should().BeTrue();
}

[Fact]
public void Throws_WhenCreating_WithNullOrWhiteSpaceFullName()
{
    // Act & Assert
    var act = () => SystemAdmin.Create("", "admin@example.com", "SYS-001", "IT");
    act.Should().Throw<ArgumentException>();
}

[Fact]
public void Throws_WhenCreating_WithNullOrWhiteSpaceEmail()
{
    // Act & Assert
    var act = () => SystemAdmin.Create("Admin User", "", "SYS-001", "IT");
    act.Should().Throw<ArgumentException>();
}

[Fact]
public void Throws_WhenCreating_WithNullOrWhiteSpaceCorporateId()
{
    // Act & Assert
    var act = () => SystemAdmin.Create("Admin User", "admin@example.com", "", "IT");
    act.Should().Throw<ArgumentException>();
}

[Fact]
public void Throws_WhenCreating_WithNullOrWhiteSpaceDepartment()
{
    // Act & Assert
    var act = () => SystemAdmin.Create("Admin User", "admin@example.com", "SYS-001", "");
    act.Should().Throw<ArgumentException>();
}

[Fact]
public void Can_UpdateCorporateId()
{
    // Arrange
    var admin = SystemAdmin.Create("Admin User", "admin@example.com", "SYS-001", "IT");

    // Act
    admin.UpdateCorporateId("SYS-002");

    // Assert
    admin.CorporateId.Should().Be("SYS-002");
}

[Fact]
public void Can_UpdateDepartment()
{
    // Arrange
    var admin = SystemAdmin.Create("Admin User", "admin@example.com", "SYS-001", "IT");

    // Act
    admin.UpdateDepartment("Medical Administration");

    // Assert
    admin.Department.Should().Be("Medical Administration");
}


[Fact]
public void Can_Deactivate_SystemAdmin()
{
    // Arrange
    var admin = SystemAdmin.Create("Admin User", "admin@example.com", "SYS-001", "IT");

    // Act
    admin.Deactivate();

    // Assert
    admin.IsActive.Should().BeFalse();
}

[Fact]
public void Can_Activate_SystemAdmin()
{
    // Arrange
    var admin = SystemAdmin.Create("Admin User", "admin@example.com", "SYS-001", "IT");
    admin.Deactivate();

    // Act
    admin.Activate();

    // Assert
    admin.IsActive.Should().BeTrue();
}
```

**Note**: Tests are only for the Core (domain) layer. API endpoint restrictions and claims verification are enforced at the application layer but not tested as unit tests.

---

### Step 11: Verify API Endpoint Restrictions (with Claims-Based Policy Check)

**Files to Review**:
- `src/MedicalCenter.WebApi/Endpoints/Admin/CreateUserEndpoint.cs`
- `src/MedicalCenter.WebApi/Endpoints/Admin/UpdateUserEndpoint.cs` (if exists)
- `src/MedicalCenter.WebApi/Endpoints/Admin/DeleteUserEndpoint.cs` (if exists)

**Required Changes**:

1. **CreateUserEndpoint.cs**: For SystemAdmin creation, check `CanManageAdmins` policy
   ```csharp
   using MedicalCenter.Core.Authorization;
   
   // For SystemAdmin role, verify requester has CanManageAdmins policy
   if (req.Role == UserRole.SystemAdmin)
   {
       var currentUserId = User.GetUserId(); // From JWT (only contains UserId, Role)
       var canManageAdmins = await identityService.SatisfiesPolicyAsync(
           currentUserId, 
           ClaimBasedPolicies.CanManageAdmins, 
           ct);
       
       if (!canManageAdmins)
       {
           ThrowError("Only Super Administrators can create SystemAdmin accounts.", 403);
           return;
       }
   }
   ```

2. **UpdateUserEndpoint.cs**: Add policy check for SystemAdmin updates
   ```csharp
   // Business rule: SystemAdmin can only be updated by Super Admins
   if (user.Role == UserRole.SystemAdmin)
   {
       var currentUserId = User.GetUserId();
       var canManageAdmins = await identityService.SatisfiesPolicyAsync(
           currentUserId, 
           ClaimBasedPolicies.CanManageAdmins, 
           ct);
       
       if (!canManageAdmins)
       {
           ThrowError("Only Super Administrators can update SystemAdmin accounts.", 403);
           return;
       }
   }
   ```

3. **DeleteUserEndpoint.cs**: Add policy check for SystemAdmin deletion
   ```csharp
   // Business rule: SystemAdmin can only be deleted by Super Admins
   if (user.Role == UserRole.SystemAdmin)
   {
       var currentUserId = User.GetUserId();
       var canManageAdmins = await identityService.SatisfiesPolicyAsync(
           currentUserId, 
           ClaimBasedPolicies.CanManageAdmins, 
           ct);
       
       if (!canManageAdmins)
       {
           ThrowError("Only Super Administrators can delete SystemAdmin accounts.", 403);
           return;
       }
   }
   ```

**Key Points**:
- Claims are verified via **database lookup** (`SatisfiesPolicyAsync`), NOT from JWT token
- JWT token only contains `UserId`, `Email`, `Role` - minimal payload
- `CanManageAdmins` policy requires `AdminTier = "Super"` claim in database
- Super Admins (with claim) can manage other SystemAdmin accounts via API
- Initial Super Admin is created via seeding with the claim

---

### Step 12: Update Documentation

**Files to Update**:
- `docs/Architecture.md`: Add SystemAdmin to aggregates list
- `docs/ImplementationPlan.md`: Update user hierarchy section
- `README.md`: Update if user types are listed

**Architecture.md Changes**:
- Add SystemAdmin to aggregates list:
  ```markdown
  - `Doctor`, `HealthcareStaff`, `Laboratory`, `ImagingCenter`, `SystemAdmin`: Practitioner aggregate roots
  ```

**ImplementationPlan.md Changes**:
- Update user hierarchy:
  ```
  User (abstract base)
  ├── Patient
  ├── Doctor
  ├── HealthcareStaff
  ├── Laboratory
  ├── ImagingCenter
  └── SystemAdmin
  ```

---

### Step 13: Verification Checklist

**Domain Model:**
- [ ] SystemAdmin aggregate root created with enhanced properties:
  - [ ] `CorporateId` (string, unique)
  - [ ] `Department` (string)
- [ ] EF Core configuration created and applied
- [ ] IdentityConfiguration updated with SystemAdmin relationship
- [ ] ApplicationUser navigation property added

**Claims & Authorization Infrastructure:**
- [ ] `IdentityClaimTypes` class created in `Core/Authorization/`
- [ ] `IdentityClaimValues` class created with well-known values
- [ ] `ClaimBasedPolicies` class created with policy names
- [ ] `IIdentityService` updated with claims verification methods:
  - [ ] `HasClaimAsync(userId, (type, value))`
  - [ ] `HasClaimTypeAsync(userId, claimType)`
  - [ ] `GetUserClaimsAsync(userId)`
  - [ ] `AddClaimAsync(userId, (type, value))`
  - [ ] `RemoveClaimAsync(userId, (type, value))`
  - [ ] `SatisfiesPolicyAsync(userId, policyName)`
- [ ] `IdentityService` implementations added

**Seeding:**
- [ ] SystemAdminSeeder updated to seed domain entity with new properties
- [ ] SuperAdmin claim (`AdminTier = "Super"`) seeded for initial admin

**Query Service:**
- [ ] UserQueryService updated (AdminUserWrapper removed)
- [ ] UserQueryableExtensions updated (special handling removed)

**API Endpoint Restrictions (Claims-Based):**
- [ ] CreateUserEndpoint checks `CanManageAdmins` policy for SystemAdmin role
- [ ] UpdateUserEndpoint checks `CanManageAdmins` policy for SystemAdmin
- [ ] DeleteUserEndpoint checks `CanManageAdmins` policy for SystemAdmin

**Database:**
- [ ] Migration created and verified
- [ ] Database updated successfully

**Testing:**
- [ ] Domain tests created and passing (with new properties)
- [ ] All existing tests still pass

**Documentation:**
- [ ] Documentation updated
- [ ] Build successful

**Manual Testing:**
- [ ] SystemAdmin can be queried via UserQueryService
- [ ] SystemAdmin appears in admin user list endpoint
- [ ] **Claims Verification**: Super Admin (with claim) can create/update/delete SystemAdmin via API
- [ ] **Claims Verification**: Non-super Admin gets 403 when trying to manage SystemAdmin
- [ ] **Claims Verification**: Policy check uses database lookup, not JWT token

---

## Database Schema Changes

### New Table: SystemAdmins

```sql
CREATE TABLE [SystemAdmins] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [FullName] nvarchar(200) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [CorporateId] nvarchar(100) NOT NULL,
    [Department] nvarchar(200) NOT NULL,
    [Role] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [FK_SystemAdmins_AspNetUsers_Id] 
        FOREIGN KEY ([Id]) REFERENCES [AspNetUsers] ([Id]) 
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_SystemAdmins_Email] ON [SystemAdmins] ([Email]);
CREATE UNIQUE INDEX [IX_SystemAdmins_CorporateId] ON [SystemAdmins] ([CorporateId]);
CREATE INDEX [IX_SystemAdmins_Department] ON [SystemAdmins] ([Department]);
CREATE INDEX [IX_SystemAdmins_Id_IsActive] ON [SystemAdmins] ([Id], [IsActive]);
```

### Claims Data (AspNetUserClaims)

Claims are stored in the existing `AspNetUserClaims` table (ASP.NET Core Identity built-in):

```sql
-- Example: Super Admin claim for initial admin
INSERT INTO [AspNetUserClaims] ([UserId], [ClaimType], [ClaimValue])
VALUES (@AdminId, 'MedicalCenter.AdminTier', 'Super');
```

**Important**: Claims are stored ONLY in the database, NOT in JWT tokens. This avoids token size issues since claims can be unlimited.

---

## Testing Strategy

**Note**: Following project conventions, tests are only for the Core (domain) layer. Infrastructure and API layers are not unit tested.

### Unit Tests (Domain Layer Only)
- Test SystemAdmin creation with valid/invalid inputs
- Test EmployeeId validation (required, not null/whitespace)
- Test UpdateEmployeeId method
- Test IsActive management (Activate/Deactivate)
- Test UpdateFullName method (inherited from User)

**API Endpoint Restrictions**: The business rule that SystemAdmin cannot be created/updated/deleted via API is enforced at the application layer (endpoints and validators) but is not unit tested. This is verified through manual testing and code review.

---

## Rollback Plan

If issues arise during implementation:

1. **Migration Rollback**: `dotnet ef database update <previous-migration>`
2. **Code Revert**: Revert changes in reverse order of implementation steps
3. **Database Cleanup**: Manually drop SystemAdmins table if needed

---

## Notes

- **No Breaking Changes**: This is a modeling change, not a functional change
- **Backward Compatible**: Existing SystemAdmin Identity user will be migrated
- **Unified Query**: All user types now follow the same query pattern
- **Consistent Pattern**: SystemAdmin follows the same aggregate root pattern as other users
- **Enhanced Organizational Model**: Properties capture corporate context (CorporateId, Department)
- **Claims-Based Authorization**:
  - Claims describe WHO (identity attributes), Policies describe WHAT (capabilities)
  - Claims stored in database only (AspNetUserClaims), NOT in JWT tokens
  - This avoids token size bloat since claims can be unlimited
  - Claims verified at runtime via `IIdentityService.SatisfiesPolicyAsync()` (database lookup)
  - `AdminTier = "Super"` claim enables `CanManageAdmins` policy
- **CRITICAL BUSINESS RULE**: SystemAdmin management requires `CanManageAdmins` policy
  - Super Admins (with `AdminTier = "Super"` claim) can manage other SystemAdmin accounts
  - Non-super Admins cannot create/update/delete SystemAdmin accounts (403 Forbidden)
  - Initial Super Admin is created via database seeding with the claim
  - This is a security measure with defense-in-depth (endpoint + service + claims)

---

## Post-Implementation

After successful implementation:
1. Remove this plan document (`docs/SystemAdminModelingPlan.md`)
2. Update main documentation files
3. Commit changes with appropriate message
4. Verify all tests pass
5. Perform manual testing

---

**End of Plan**
