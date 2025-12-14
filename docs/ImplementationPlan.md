# Medical Center System - Implementation Plan

## Overview

This document outlines the high-level implementation plan for the Medical Center Automation System, following Domain-Driven Design principles and modern .NET best practices.

## Implementation Status

- ✅ **Phase 1**: Solution Scaffolding & Git Setup
- ✅ **Phase 2**: Core Foundation & Base Classes
- ✅ **Phase 3**: Infrastructure Foundation
- ✅ **Phase 4**: Identity System Foundation
- ✅ **Phase 5**: Patient Aggregate & Medical Attributes
- 🔄 **Phase 7**: Query Services & Provider Lookups (Partially Complete - UserQueryService implemented)
- 🔄 **Phase 10**: Admin Features (Partially Complete - User management endpoints implemented)
- ⏳ **Phase 6**: Medical Records & Encounters
- ⏳ **Phase 8**: Action Logging & Audit Trail
- ⏳ **Phase 9**: Complete Provider Endpoints
- ⏳ **Phase 11**: Patient Self-Service Features
- ⏳ **Phase 12**: Testing & Quality Assurance

### Completed Features

- ✅ Core foundation with base classes and common abstractions
- ✅ Infrastructure with EF Core, Identity, and repositories
- ✅ Authentication system (JWT, refresh tokens)
- ✅ Patient aggregate with medical attributes (Allergies, ChronicDiseases, Medications, Surgeries)
- ✅ Blood type management (Value Object)
- ✅ Medical attributes CRUD endpoints
- ✅ Pagination infrastructure (`PaginatedList<T>`, `PaginationMetadata`)
- ✅ User query service with pagination support
- ✅ Provider entities (Doctor, HealthcareEntity, Laboratory, ImagingCenter) with shared primary key
- ✅ Identity service for user management
- ✅ Admin user management endpoints (CRUD, change password)
- ✅ FluentValidation for all endpoints
- ✅ Swagger/OpenAPI documentation (FastEndpoints.Swagger)
- ✅ Security enhancements (RequirePatient policy, JWT role mapping)
- ✅ 154 domain unit tests passing

### In Progress

- 🔄 Query services for provider lookups (UserQueryService implemented, additional services planned)
- 🔄 Admin features (user management complete, additional admin features planned)

### Test Coverage

- **154 tests passing** (domain unit tests)
- Tests follow classical school approach (behavior-focused)
- AAA pattern (Arrange, Act, Assert)

**Coding Patterns Reference**: While the architecture defined in this plan is specific to our Medical Center system, the coding patterns, conventions, and implementation details should follow the established patterns from the [Ardalis Clean Architecture template](https://github.com/ardalis/CleanArchitecture). This template provides proven patterns for:
- Project organization and structure
- Base classes and common abstractions
- Repository and Specification implementations
- Domain event handling
- Testing approaches
- Code organization conventions

The overall architecture (three-layer: Core, Infrastructure, WebApi) remains as defined in this plan, but developers should reference the Clean Architecture template for specific coding patterns and conventions when implementing features.

---

## 1. High-Level Architecture

### 1.1 Three-Layer Architecture

The system follows a clean three-layer architecture:

```
┌─────────────────────────────────────┐
│         Web API Layer               │
│  (FastEndpoints, Validation,        │
│   Authorization, Result Pattern)    │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│         Core Layer                  │
│  (Domain Models, Aggregates,        │
│   Domain Services, Specifications,   │
│   Repository Interfaces)             │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│      Infrastructure Layer           │
│  (EF Core, Identity, Specifications │
│   Implementation, Audit Interceptors)│
└─────────────────────────────────────┘
```

### 1.2 Solution Structure

```
MedicalCenter/
├── src/
│   ├── MedicalCenter.Core/              # Domain layer
│   ├── MedicalCenter.Infrastructure/    # Data access & external services
│   └── MedicalCenter.WebApi/            # API endpoints & presentation
├── tests/
│   └── MedicalCenter.Core.Tests/        # Domain unit tests
└── docs/                                 # Documentation
```

**Note on Shared Projects**: Initially considered a `MedicalCenter.Shared` project, but decided against it to maintain clean architecture principles. DTOs belong in the WebApi layer (they're presentation concerns), domain constants/enums belong in Core, and API-specific constants belong in WebApi. This avoids unnecessary coupling between layers.

---

## 2. Core Layer (Domain)

### 2.1 Aggregates

The domain is organized around the following aggregates:

#### **Patient Aggregate**
- Root entity: `Patient` (inherits from `User`)
- Contains medical attributes (Allergies, ChronicDiseases, Medications, Surgeries, BloodType)
- Owns `Encounter` collection (as references, not owned entities)
- Enforces invariants for medical attribute updates
- Domain methods for managing medical attributes
- **Why it's an aggregate**: Has its own consistency boundary, owns medical attributes, enforces business rules

#### **MedicalRecord Aggregate**
- Root entity: `MedicalRecord`
- Contains medical content, attachments, creator information
- Business rules:
  - Only creator can modify
  - Attachments are immutable
- Automatically creates `Encounter` when created
- **Why it's an aggregate**: Has its own consistency boundary, enforces modification rules

#### **Encounter Aggregate**
- Root entity: `Encounter`
- Represents real-world interactions between Patient and Provider
- Created automatically when medical records are added
- Contains: PatientId, ProviderId, ProviderType, EncounterType, Timestamp
- **Why it's an aggregate**: Represents a distinct business concept with its own lifecycle

#### **ActionLog Aggregate**
- Root entity: `ActionLog`
- Tracks data access and changes
- Visible to patients
- Audit trail for compliance
- **Why it's an aggregate**: Has its own consistency boundary for audit purposes

### 2.1.1 User Entities (Not Aggregates)

The other user types (`Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`) are **domain entities**, not aggregates:

- They inherit from `User` for identity and authentication purposes
- They are **referenced** by other aggregates (Encounters, MedicalRecords) but don't own them
- They don't have their own consistency boundaries or enforce complex business rules
- They are primarily **reference data** - their role is to identify providers in encounters and records
- They can be queried and displayed, but the core business logic revolves around Patients, Records, and Encounters

**Repository consideration**: Since provider entities (`Doctor`, `HealthcareEntity`, etc.) are not aggregates, they are not accessed through the repository pattern. When provider data is needed, it's queried through query service interfaces (e.g., `IProviderQueryService`) defined in Core, with implementations in Infrastructure. Provider data can also be included in specifications that query aggregates (e.g., a specification that loads Encounters with their associated Provider information).

### 2.2 Domain Services

- `EncounterCreationService`: Handles automatic encounter creation when records are added
- `MedicalAttributeValidationService`: Validates medical attribute updates
- `ActionLogService`: Records actions for audit purposes

### 2.3 Value Objects

- `BloodType`: Immutable value object (Group + Rhesus)
- Other value objects as needed for domain concepts

### 2.4 Specifications

Using Ardalis.Specification pattern for:
- Complex query logic
- Reusable filtering criteria
- Encapsulation of business rules in queries

### 2.5 Repository Pattern

**Single Generic Repository Interface**: `IRepository<T>` where `T` is constrained to aggregate roots only.

**Key Principles**:
- **One interface for all aggregates**: Generic `IRepository<TAggregate>` where `TAggregate : IAggregateRoot`
- **Specification Pattern Integration**: All queries use specifications (`ISpecification<T>`)
- **Aggregate roots only**: Repository works exclusively with aggregate roots, not regular entities
- **Collection-like interface**: Provides methods like `GetByIdAsync`, `ListAsync`, `FirstOrDefaultAsync`, `CountAsync`, `Add`, `Update`, `Delete`
- **Returns aggregates, not DTOs**: All methods return domain aggregates

**Interface Structure** (using Ardalis.Specification):
```csharp
public interface IRepository<T> where T : IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

**Why Generic Repository?**:
- **DRY Principle**: Avoids code duplication across multiple repository implementations
- **Consistency**: Same interface and behavior for all aggregates
- **Specification Pattern**: All queries encapsulated in specifications, making them reusable and testable
- **Type Safety**: Generic constraint ensures only aggregate roots can be used
- **Framework Integration**: Ardalis.Specification provides EF Core implementation out of the box

**Provider Entities**: Since `Doctor`, `HealthcareEntity`, `Laboratory`, and `ImagingCenter` are not aggregates, they are not accessed through the repository. They are queried through query service interfaces (defined in Core) with implementations in Infrastructure.

### 2.6 Query Service Interfaces

**For Non-Aggregate Entities and Complex Queries**: Following the [Ardalis Clean Architecture template](https://github.com/ardalis/CleanArchitecture) principle that queries don't need the repository pattern, we define query service interfaces in the Core layer.

**Key Principles**:
- **Interfaces in Core**: Query service interfaces are defined in Core layer (abstractions)
- **Read-only operations**: These services are for queries only, not mutations
- **Returns DTOs or domain entities**: Can return DTOs optimized for specific use cases or domain entities
- **No infrastructure dependencies**: Interfaces are framework-agnostic

**Example Interfaces**:
```csharp
// In Core layer
public interface IProviderQueryService
{
    Task<IReadOnlyList<DoctorDto>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetDoctorByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorDto>> GetDoctorsBySpecialtyAsync(string specialty, CancellationToken cancellationToken = default);
}

public interface IUserQueryService
{
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}
```

**Why Query Services Instead of Repository?**:
- **Queries are read-only**: No need for aggregate consistency guarantees
- **Flexibility**: Can optimize queries, use raw SQL, or stored procedures as needed
- **Performance**: Not constrained by repository abstraction
- **DDD Compliance**: Repository pattern reserved for aggregates where it provides value

**Note**: DTOs returned by query services can be defined in Core (if they represent domain concepts) or in WebApi (if they're presentation-specific). Follow the Clean Architecture template's guidance on DTO placement.

---

## 3. Infrastructure Layer

### 3.1 Data Access

- **EF Core DbContext**: `MedicalCenterDbContext`
- **Entity Mappings**: 
  - Table-per-hierarchy for User inheritance (using views)
  - Owned entities for value objects
  - Aggregate root configurations
- **Migrations**: Code-first approach

### 3.2 Identity

- **ASP.NET Core Identity** integration
- Custom user store for role-specific user types
- SQL Views for mapping Users + role-specific tables to domain aggregates
- JWT token generation and validation

### 3.3 Specification Implementations

- EF Core implementations of specifications using `Ardalis.Specification.EntityFrameworkCore`

### 3.4 Audit Interceptors

- **AuditableEntityInterceptor**: EF Core interceptor that automatically sets audit properties
  - Only affects entities implementing `IAuditableEntity` interface
  - Sets `CreatedAt` on entity creation (EntityState.Added)
  - Sets `UpdatedAt` on entity modification (EntityState.Modified)
  - Non-auditable entities (like `ActionLog`) are not affected
- **Action Logging Interceptors**: Separate interceptors for tracking user actions (for ActionLog aggregate)
- Keeps audit logic separate from domain entities - entities opt-in via interface

### 3.5 Repository Implementation

- **Single Generic Implementation**: `Repository<T>` using EF Core and `Ardalis.Specification.EntityFrameworkCore`
- **Base Class**: Inherits from `Ardalis.Specification.EF.RepositoryBase<T>` or implements `IRepository<T>` directly
- **Specification Evaluator**: Uses `SpecificationEvaluator` to apply specifications to EF Core queries
- **DbContext Integration**: Works with `MedicalCenterDbContext` through `DbSet<T>`
- **Transaction Support**: Can participate in EF Core transactions managed by the DbContext

**Implementation Approach**:
- Use `Ardalis.Specification.EntityFrameworkCore` package which provides `RepositoryBase<T>`
- Extend or wrap the base implementation if custom behavior is needed
- All query operations go through specifications, ensuring business rules are encapsulated
- Add/Update/Delete operations work directly with aggregates

**Example Usage**:
```csharp
// In application code or domain services
var spec = new PatientByIdSpecification(patientId);
var patient = await _repository.FirstOrDefaultAsync(spec);

var activePatientsSpec = new ActivePatientsSpecification();
var activePatients = await _repository.ListAsync(activePatientsSpec);
```

### 3.6 Query Service Implementations

**Implementations in Infrastructure**: Query service interfaces (defined in Core) are implemented in the Infrastructure layer using EF Core DbContext.

**Key Principles**:
- **Implementations in Infrastructure**: All query service implementations live in Infrastructure layer
- **DbContext access**: Use `MedicalCenterDbContext` directly for queries
- **No Core dependencies on Infrastructure**: Core defines interfaces, Infrastructure implements them
- **Flexible query approaches**: Can use LINQ, specifications, raw SQL, or stored procedures

**Implementation Pattern**:
```csharp
// In Infrastructure layer
public class ProviderQueryService : IProviderQueryService
{
    private readonly MedicalCenterDbContext _context;
    
    public ProviderQueryService(MedicalCenterDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyList<DoctorDto>> GetActiveDoctorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Doctor>()
            .Where(d => d.IsActive)
            .Select(d => new DoctorDto 
            { 
                Id = d.Id,
                FullName = d.FullName,
                Specialty = d.Specialty,
                LicenseNumber = d.LicenseNumber
            })
            .ToListAsync(cancellationToken);
    }
    
    public async Task<DoctorDto?> GetDoctorByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Doctor>()
            .Where(d => d.Id == id)
            .Select(d => new DoctorDto { ... })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

**Using Specifications with Query Services** (Optional):
```csharp
// Specification in Core
public class ActiveDoctorsBySpecialtySpecification : Specification<Doctor>
{
    public ActiveDoctorsBySpecialtySpecification(string specialty)
    {
        Query.Where(d => d.IsActive && d.Specialty == specialty);
    }
}

// In Infrastructure query service implementation
public async Task<IReadOnlyList<DoctorDto>> GetDoctorsBySpecialtyAsync(
    string specialty, 
    CancellationToken cancellationToken = default)
{
    var spec = new ActiveDoctorsBySpecialtySpecification(specialty);
    return await _context.Set<Doctor>()
        .WithSpecification(spec)
        .Select(d => new DoctorDto { ... })
        .ToListAsync(cancellationToken);
}
```

**When to Use Query Services**:
- **Non-aggregate entities**: For querying `Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`
- **Complex queries**: For queries that don't fit well into repository pattern
- **Reporting/Read-only operations**: For queries that return DTOs optimized for specific views
- **Performance-critical queries**: When you need direct control over SQL generation

**Dependency Injection**: Register query service implementations in the DI container (typically in Infrastructure's service registration or WebApi's Program.cs).

---

## 4. Web API Layer

### 4.1 Endpoint Organization

Following the folder structure from documentation:

```
Endpoints/
├── Admin/
│   ├── Users/
│   ├── Records/
│   └── Encounters/
├── Patients/
│   ├── Self/
│   └── Registration/
├── Doctors/
│   ├── Records/
│   └── Encounters/
├── Healthcare/
│   ├── Records/
│   └── Encounters/
├── Labs/
│   ├── Records/
│   └── Encounters/
└── Imaging/
    ├── Records/
    └── Encounters/
```

### 4.2 FastEndpoints Configuration

- Endpoint classes inherit from `Endpoint<TRequest, TResponse>`
- Request validation using FluentValidation
- Authorization policies for RBAC
- Result pattern for error handling
- Action logging middleware

### 4.3 Authentication & Authorization

- JWT-based authentication
- **Role-Based Access Control (RBAC)**: Primary authorization mechanism using ASP.NET Core Identity roles
- **Claims-Based Authorization**: Fine-grained access control using JWT claims
- **Authorization Policies**: Named policies combining roles and claims for endpoint protection
- **Resource-Based Authorization**: Additional checks for resource ownership (e.g., patients accessing their own data)

See [Authorization Architecture](#authorization-architecture) section for detailed implementation.

### 4.4 Validation

- FluentValidation validators for each endpoint request
- Input validation at API boundaries
- Domain validation in aggregates

### 4.5 Result Pattern

- Custom `Result<T>` type for operation outcomes
- Consistent error handling across endpoints
- No exceptions for expected business errors

### 4.6 Mapping

- AutoMapper profiles for DTO ↔ Domain entity mapping
- Separate request/response DTOs
- No domain entities exposed directly

---

## 5. Identity System

### 5.1 User Hierarchy

```
User (abstract base)
├── Patient
├── Doctor
├── HealthcareEntity
├── Laboratory
└── ImagingCenter
```

### 5.2 Identity Services

- `IIdentityService`: User registration, password management, role-based operations
- `ITokenProvider`: JWT generation and validation

### 5.3 Database Design

- **Users Table**: Base identity table (EF Core Identity)
- **Role-Specific Tables**: 
  - `PatientDetails`
  - `DoctorDetails`
  - `HealthcareDetails`
  - `LabDetails`
  - `ImagingDetails`
- **SQL Views**: Join Users + role-specific tables for domain aggregates

---

## Authorization Architecture

### Overview

The system uses a **hybrid authorization approach** combining:
1. **Role-Based Access Control (RBAC)** - Primary mechanism using ASP.NET Core Identity roles
2. **Claims-Based Authorization** - Fine-grained permissions via JWT claims
3. **Authorization Policies** - Named policies combining roles and claims
4. **Resource-Based Authorization** - Runtime checks for resource ownership

### 1. Roles (RBAC)

Roles are stored in ASP.NET Core Identity (`AspNetRoles`, `AspNetUserRoles`) and assigned during user creation:

| Role | Description | Use Case |
|------|-------------|----------|
| `SystemAdmin` | System administrators | Full system access, user management |
| `Patient` | Patients receiving care | Access to own medical records, self-registration |
| `Doctor` | Medical doctors | Create records, view patient data, modify medical attributes |
| `HealthcareStaff` | Hospital/clinic staff | Create records, view patient data, modify medical attributes |
| `LabUser` | Laboratory technicians | Create lab records, view related patient data |
| `ImagingUser` | Imaging technicians | Create imaging records, view related patient data |

### 2. JWT Claims Structure

JWT tokens contain the following claims:

**Standard Claims:**
- `sub` (Subject) - User ID (Guid)
- `email` - User email address
- `name` - User full name
- `role` - User role (from Identity roles)

**Custom Claims:**
- `userId` - User ID (Guid, for convenience)
- `userRole` - User role enum value (for type-safe checks)
- `specialty` - Doctor specialty (for doctors only)
- `organizationId` - Organization ID (for healthcare entities, labs, imaging centers)

**Example JWT Claims:**
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "doctor@example.com",
  "name": "Dr. John Smith",
  "role": "Doctor",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userRole": "3",
  "specialty": "Cardiology"
}
```

### 3. Authorization Policies

Authorization policies are defined in Infrastructure layer and registered in `Program.cs`:

#### 3.1 Role-Based Policies

```csharp
// Basic role policies
options.AddPolicy("RequirePatient", policy => 
    policy.RequireRole(UserRole.Patient.ToString()));

options.AddPolicy("RequireDoctor", policy => 
    policy.RequireRole(UserRole.Doctor.ToString()));

options.AddPolicy("RequireAdmin", policy => 
    policy.RequireRole(UserRole.SystemAdmin.ToString()));

// Composite role policies
options.AddPolicy("RequireProvider", policy => 
    policy.RequireRole(
        UserRole.Doctor.ToString(),
        UserRole.HealthcareStaff.ToString(),
        UserRole.LabUser.ToString(),
        UserRole.ImagingUser.ToString()));

options.AddPolicy("RequirePatientOrProvider", policy => 
    policy.RequireRole(
        UserRole.Patient.ToString(),
        UserRole.Doctor.ToString(),
        UserRole.HealthcareStaff.ToString(),
        UserRole.LabUser.ToString(),
        UserRole.ImagingUser.ToString()));
```

#### 3.2 Claims-Based Policies

```csharp
// Fine-grained permission policies
options.AddPolicy("CanModifyMedicalAttributes", policy =>
    policy.RequireClaim("role", "Doctor", "HealthcareStaff")
          .RequireAssertion(context => 
              context.User.HasClaim("permission", "modify_medical_attributes")));

options.AddPolicy("CanCreateRecords", policy =>
    policy.RequireClaim("role", "Doctor", "HealthcareStaff", "LabUser", "ImagingUser"));

options.AddPolicy("CanViewAllPatients", policy =>
    policy.RequireClaim("role", "Doctor", "HealthcareStaff", "SystemAdmin"));
```

#### 3.3 Resource-Based Policies

Resource-based authorization is handled at the endpoint level using custom authorization handlers or inline checks:

```csharp
// Example: Patient accessing own data
public override async Task HandleAsync(GetPatientRecordsRequest req, CancellationToken ct)
{
    var currentUserId = User.FindFirstValue("userId");
    if (req.PatientId.ToString() != currentUserId && !User.IsInRole("Doctor"))
    {
        await SendUnauthorizedAsync(ct);
        return;
    }
    // ... rest of handler
}
```

### 4. Policy-to-Endpoint Mapping

| Endpoint | Policy | Additional Checks |
|----------|--------|-------------------|
| `POST /patients` | None (public) | N/A (self-registration) |
| `GET /patients/self/*` | `RequirePatient` | Verify `userId` claim matches resource |
| `PUT /patients/self/medical-attributes` | `RequirePatient` | Verify ownership + business rules |
| `POST /doctors/records` | `RequireDoctor` | N/A |
| `GET /doctors/records` | `RequireDoctor` | Optional: filter by own records |
| `POST /healthcare/records` | `RequireProvider` | Verify role is HealthcareStaff |
| `POST /labs/records` | `RequireProvider` | Verify role is LabUser |
| `POST /imaging/records` | `RequireProvider` | Verify role is ImagingUser |
| `GET /admin/users` | `RequireAdmin` | N/A |
| `POST /admin/users` | `RequireAdmin` | N/A |

### 5. FastEndpoints Integration

FastEndpoints uses policies via the `Policies()` method:

```csharp
public class GetPatientRecordsEndpoint : Endpoint<GetPatientRecordsRequest>
{
    public override void Configure()
    {
        Get("/patients/self/records");
        Policies("RequirePatient");
        // OR use claims directly
        // Claims("role", "Patient");
    }
    
    public override async Task HandleAsync(GetPatientRecordsRequest req, CancellationToken ct)
    {
        // Resource-based check
        var currentUserId = Guid.Parse(User.FindFirstValue("userId")!);
        if (req.PatientId != currentUserId)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        // ... rest of handler
    }
}
```

### 6. Authorization Implementation Details

#### 6.1 Token Generation

JWT tokens are generated in `TokenProvider` (Infrastructure) with claims populated from:
- Identity user roles → `role` claim
- Domain user entity → custom claims (specialty, organizationId, etc.)

#### 6.2 Policy Registration

Policies are registered in `DependencyInjection.AddInfrastructure()` or a dedicated `AddAuthorizationPolicies()` extension method in Infrastructure layer.

#### 6.3 Resource Ownership Verification

Resource-based authorization patterns:
- **Patient Resources**: Verify `userId` claim matches resource owner
- **Provider Resources**: Verify role has access + optional organization/entity checks
- **Admin Resources**: Verify `SystemAdmin` role

### 7. Security Considerations

- **Principle of Least Privilege**: Users only get minimum required permissions
- **Defense in Depth**: Multiple layers (policies + resource checks)
- **Token Expiration**: JWT tokens have reasonable expiration times
- **Refresh Tokens**: Secure refresh token mechanism for token renewal
- **Audit Logging**: All authorization decisions logged for security auditing

### 8. Testing Authorization

Authorization tests should verify:
- ✅ Policies correctly allow/deny access based on roles
- ✅ Claims are correctly included in JWT tokens
- ✅ Resource-based checks prevent unauthorized access
- ✅ Endpoints return 401/403 for unauthorized requests
- ✅ Endpoints return 200 for authorized requests

---

## 6. Medical Attributes Domain Model

### 6.1 Patient Medical Attributes

Medical attributes are part of the `Patient` aggregate:

- **Allergies**: Collection of `Allergy` entities
- **ChronicDiseases**: Collection of `ChronicDisease` entities
- **Medications**: Collection of `Medication` entities
- **Surgeries**: Collection of `Surgery` entities
- **BloodType**: Value object (optional)

### 6.2 Domain Rules

- Only authorized providers can modify medical attributes
- Patients cannot modify blood type or chronic conditions
- All updates must have audit metadata (RecordedBy, DiagnosedBy, etc.)
- No automatic inference from uploaded records
- Medical records can suggest updates, but clinician approval required

---

## 7. Key Architectural Decisions

### 7.0 Architectural Decision Explanations

#### 7.0.1 Why No Shared Project?

**Decision**: Removed `MedicalCenter.Shared` project from the solution structure.

**Rationale**:
- **DTOs belong in WebApi**: Request/Response DTOs are presentation concerns and should live in the WebApi layer. They're not shared across layers - Core doesn't need to know about DTOs.
- **Domain constants in Core**: If constants represent domain concepts (e.g., `UserRole` enum, `EncounterType` enum), they belong in the Core layer as part of the domain model.
- **API constants in WebApi**: API-specific constants (e.g., route names, policy names) belong in the WebApi layer.
- **Avoid coupling**: Shared projects create coupling between layers, violating clean architecture principles. Each layer should only depend on layers below it.
- **YAGNI principle**: We don't need a shared project until we have a concrete need that can't be satisfied by placing items in the appropriate layer.

**Alternative considered**: If we later need truly shared utilities (e.g., extension methods used by both Infrastructure and WebApi), we can introduce a shared project, but it should be minimal and well-justified.

#### 7.0.2 User Roles: Aggregates vs Entities

**Decision**: Only `Patient` is a true aggregate. Other user types (`Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`) are domain entities, not aggregates.

**Rationale**:
- **Patient is an aggregate** because:
  - It owns medical attributes (Allergies, ChronicDiseases, etc.) with complex business rules
  - It enforces invariants (e.g., only providers can modify certain attributes)
  - It has a consistency boundary (medical attributes must be consistent with each other)
  - It's referenced by Encounters but also has its own domain logic

- **Other user types are entities** because:
  - They are primarily **reference data** - they identify who created a record or participated in an encounter
  - They don't own collections of domain objects that require consistency boundaries
  - They don't enforce complex business rules beyond basic validation (license numbers, organization names)
  - They're referenced by aggregates but don't have their own aggregate boundaries
  - They're more like "lookup entities" - you query them to find a provider, but the business logic lives in the aggregates that reference them

**DDD Principle**: Not every entity needs to be an aggregate. Aggregates are consistency boundaries. If an entity doesn't need to maintain consistency of related objects or enforce complex invariants, it's just a regular entity.

**Repository Pattern**: Provider entities are not accessed through the repository pattern since they're not aggregates. When provider data is needed, it's accessed directly through DbContext in the Infrastructure layer, or included in specifications that query aggregates (e.g., loading Encounters with their Provider information).

#### 7.0.3 Generic Repository with Specification Pattern

**Decision**: Use a single generic `IRepository<T>` interface for all aggregate roots, integrated with the Specification pattern.

**Rationale**:
- **DRY Principle**: Avoids code duplication - one implementation serves all aggregates (Patient, MedicalRecord, Encounter, ActionLog)
- **Consistency**: Same interface and behavior across all aggregates ensures predictable usage
- **Specification Pattern Integration**: All queries use specifications (`ISpecification<T>`), which:
  - Encapsulates business rules in queries
  - Makes queries reusable and composable
  - Improves testability (specifications can be tested independently)
  - Separates query logic from repository implementation
- **Type Safety**: Generic constraint `where T : IAggregateRoot` ensures only aggregate roots can be used
- **Framework Support**: `Ardalis.Specification.EntityFrameworkCore` provides EF Core implementation out of the box
- **Maintainability**: Changes to repository behavior affect all aggregates consistently

**Why Not Separate Repositories?**:
- **Unnecessary Abstraction**: Each aggregate would have identical methods (`GetByIdAsync`, `ListAsync`, etc.) - no aggregate-specific behavior needed
- **Code Duplication**: Would require maintaining multiple interfaces and implementations with the same logic
- **Violates Open/Closed Principle**: Adding a new aggregate would require creating a new repository interface and implementation

**Aggregate Roots Only**:
- Repository works exclusively with aggregate roots (entities that have their own consistency boundaries)
- Regular entities (like `Doctor`, `HealthcareEntity`) are not accessed through the repository
- This enforces DDD principle: repositories are for aggregates, not all entities
- Provider entities are queried through query service interfaces (defined in Core, implemented in Infrastructure)

**Specification Pattern Benefits**:
- **Reusable Queries**: `GetPatientByIdSpecification`, `ActivePatientsSpecification`, etc. can be reused across the application
- **Composable**: Specifications can be combined (e.g., `And`, `Or` operations)
- **Testable**: Specifications can be unit tested independently
- **Business Rules in Domain**: Query logic lives in specifications (Core layer), not in infrastructure

#### 7.0.4 Query Services for Non-Aggregate Entities

**Decision**: Use query service interfaces (in Core) with implementations (in Infrastructure) for querying non-aggregate entities and complex read-only operations.

**Rationale**:
- **Clean Architecture Compliance**: Interfaces in Core, implementations in Infrastructure maintains proper dependency direction
- **No Infrastructure in Core**: Core layer remains framework-agnostic - no DbContext references
- **Flexibility for Queries**: Following [Clean Architecture template guidance](https://github.com/ardalis/CleanArchitecture), queries don't need repository pattern - can use most convenient approach
- **Separation of Concerns**: Query services are read-only, separate from mutation operations (which use Repository pattern)
- **Performance**: Direct DbContext access allows query optimization without repository abstraction overhead

**Why Not Repository for Non-Aggregates?**:
- **Repository is for Aggregates**: Repository pattern provides consistency boundaries for aggregates - non-aggregates don't need this
- **Queries are Read-Only**: No need for aggregate consistency guarantees in read operations
- **Simpler Abstraction**: Query services can be simpler and more focused on specific query needs
- **DDD Principle**: Repository pattern should be reserved for aggregates where it provides value

**Pattern**:
- **Core Layer**: Define `IProviderQueryService`, `IUserQueryService`, etc. interfaces
- **Infrastructure Layer**: Implement query services using `MedicalCenterDbContext`
- **WebApi Layer**: Inject query service interfaces, use them in endpoints
- **Returns**: DTOs (can be in Core if domain concepts, or WebApi if presentation-specific) or domain entities

**Reference**: This follows the pattern from the [Clean Architecture template](https://github.com/ardalis/CleanArchitecture) where queries are explicitly noted as not requiring the repository pattern and can use whatever approach is most convenient.

#### 7.0.5 Auditable Entities Pattern

**Decision**: Use `IAuditableEntity` interface for opt-in audit tracking, with EF Core interceptor automatically setting audit properties.

**Rationale**:
- **Not All Entities Need Auditing**: Some entities like `ActionLog` are audit records themselves and don't need audit tracking
- **Opt-In Pattern**: Entities explicitly implement `IAuditableEntity` if they need audit tracking
- **Separation of Concerns**: Audit logic is in Infrastructure (interceptor), not in domain entities
- **Automatic Management**: EF Core interceptor automatically sets `CreatedAt` and `UpdatedAt` without domain code needing to manage timestamps
- **Future Extensibility**: Interface can be extended with `CreatedBy`/`UpdatedBy` properties if needed

**Implementation**:
- **Core Layer**: `IAuditableEntity` interface with `DateTime CreatedAt` and `DateTime? UpdatedAt` properties
- **Core Layer**: `BaseEntity` contains only `Id` (Guid) - no audit properties
- **Infrastructure Layer**: `AuditableEntityInterceptor` checks if entity implements `IAuditableEntity`:
  - On `EntityState.Added`: Sets `CreatedAt` to current time
  - On `EntityState.Modified`: Sets `UpdatedAt` to current time
  - Ignores entities that don't implement the interface
- **Domain Entities**: Entities that need auditing implement both `BaseEntity` and `IAuditableEntity`

**Example**:
```csharp
// Core layer
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
}

// Domain entity
public class Patient : BaseEntity, IAggregateRoot, IAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // ... other properties
}

// Non-auditable entity
public class ActionLog : BaseEntity, IAggregateRoot
{
    // Does NOT implement IAuditableEntity
    // ... other properties
}
```

### 7.1 Design Patterns

- **Generic Repository Pattern**: Single `IRepository<T>` interface for all aggregate roots, abstracting data access
- **Specification Pattern**: Encapsulate complex queries and business rules in reusable, composable specifications
- **Result Pattern**: Handle operation outcomes without exceptions for expected business errors
- **Value Objects**: Immutable domain concepts (e.g., `BloodType`)
- **Aggregates**: Consistency boundaries (Patient, MedicalRecord, Encounter, ActionLog)

### 7.2 Framework Choices

- **FastEndpoints**: Lightweight alternative to controllers
- **Ardalis.Specification**: Query encapsulation
- **Ardalis.GuardClauses**: Input validation
- **FluentValidation**: Request validation
- **AutoMapper**: DTO mapping
- **Entity Framework Core**: ORM

### 7.3 Testing Strategy

- **Classical School** of unit testing
- Test behavior, not implementation
- Integration tests at API boundaries
- Real database for managed dependencies
- Mocks only for unmanaged dependencies (external services)

### 7.4 Domain Purity

- Core layer remains framework-agnostic
- No infrastructure dependencies in domain
- Domain events for cross-aggregate communication (if needed)
- Rich domain models with business logic

### 7.5 Coding Patterns and Conventions

**Reference Implementation**: The [Ardalis Clean Architecture template](https://github.com/ardalis/CleanArchitecture) serves as the reference for coding patterns, conventions, and implementation details.

**Key Areas to Follow**:
- **Base Classes**: Follow patterns for `BaseEntity`, `ValueObject`, `IAggregateRoot` as shown in the template
- **Repository Implementation**: Use the generic repository pattern with Ardalis.Specification as demonstrated in the template
- **Domain Events**: Follow the domain event pattern and handler structure from the template
- **Project Organization**: Follow folder structure and naming conventions
- **Testing Patterns**: Use similar test organization and helper patterns
- **Code Style**: Follow the coding style and conventions used in the template

**Note**: The overall architecture (three-layer structure) is defined in this plan and should not be changed. However, when implementing specific features, developers should reference the Clean Architecture template for:
- How to structure classes within each layer
- How to implement common patterns (specifications, domain events, etc.)
- Naming conventions and code organization
- Testing approaches and test helpers

This ensures consistency with proven patterns while maintaining our specific architectural decisions.

---

## 8. Detailed Implementation Plan

This section provides a comprehensive, phase-by-phase implementation guide. Each phase results in a working, deliverable application that can be tested and demonstrated.

### Phase 1: Solution Scaffolding & Git Setup

**Goal**: Establish the solution structure, version control, and basic project setup.

**Deliverable**: A working solution with proper structure, Git repository, and basic README.

#### Tasks:

1. **Initialize Git Repository**
   - Create `.gitignore` file for .NET projects
   - Initialize git repository
   - Create initial commit

2. **Create Solution Structure**
   - Create solution file: `MedicalCenter.sln`
   - Create folder structure:
     ```
     MedicalCenter/
     ├── src/
     │   ├── MedicalCenter.Core/
     │   ├── MedicalCenter.Infrastructure/
     │   └── MedicalCenter.WebApi/
     ├── tests/
     │   ├── MedicalCenter.Core.Tests/
     │   ├── MedicalCenter.Infrastructure.Tests/
     │   └── MedicalCenter.WebApi.Tests/
     └── docs/
     ```

3. **Create Projects**
   - Create `MedicalCenter.Core` class library (.NET 8)
   - Create `MedicalCenter.Infrastructure` class library (.NET 8)
   - Create `MedicalCenter.WebApi` ASP.NET Core Web API (.NET 8)
   - Create test projects (xUnit) for each layer

4. **Set Up Project References**
   - WebApi → Infrastructure → Core
   - Infrastructure → Core
   - Test projects reference their respective projects

5. **Install Base NuGet Packages**
   - Core: Ardalis.GuardClauses, Ardalis.Specification
   - Infrastructure: EntityFrameworkCore, Ardalis.Specification.EntityFrameworkCore
   - WebApi: FastEndpoints, FluentValidation, AutoMapper
   - Tests: xUnit, FluentAssertions, Moq

6. **Create README.md**
   - Project overview
   - Architecture diagram reference
   - Prerequisites (.NET 8 SDK, Docker for database)
   - How to run instructions
   - Project structure explanation
   - Link to this implementation plan

7. **Basic Configuration Files**
   - `.editorconfig`
   - `Directory.Build.props` (if needed for common properties)
   - `appsettings.json` and `appsettings.Development.json` in WebApi

**Verification**:
- ✅ Solution builds successfully
- ✅ All projects reference correctly
- ✅ Git repository initialized with proper `.gitignore`
- ✅ README.md exists and is informative
- ✅ Can run `dotnet build` and `dotnet test` successfully

---

### Phase 2: Core Foundation & Base Classes

**Goal**: Establish base classes, common abstractions, and domain foundation.

**Deliverable**: Core layer with base classes, value objects, and aggregate root interface.

#### Tasks:

1. **Create Base Classes (Following Clean Architecture Template)**
   - `BaseEntity` (with Id only - not all entities are auditable)
   - `IAuditableEntity` interface (with CreatedAt, UpdatedAt properties)
   - `ValueObject` base class
   - `IAggregateRoot` interface
   - `BaseDomainEvent` (if using domain events)
   
   **Note**: Not all entities are auditable (e.g., `ActionLog`). Entities that need audit tracking should implement `IAuditableEntity`. The audit properties (CreatedAt, UpdatedAt) will be set automatically by an EF Core interceptor.

2. **Create Common Abstractions**
   - `IRepository<T>` interface (generic repository)
   - Common enums: `UserRole`, `ProviderType`, `EncounterType`, `RecordType`

3. **Create Result Pattern**
   - `Result<T>` class for operation outcomes
   - `Error` class for error handling
   - Extension methods for Result operations

4. **Write Unit Tests**
   - Test `BaseEntity` behavior
   - Test `ValueObject` equality
   - Test `Result<T>` pattern
   - Test guard clauses usage

5. **Update README.md**
   - Document base classes and their purpose
   - Add architecture decision notes

**Verification**:
- ✅ Core project builds
- ✅ All base classes compile
- ✅ Unit tests pass
- ✅ No infrastructure dependencies in Core

---

### Phase 3: Infrastructure Foundation

**Goal**: Set up EF Core, DbContext, and repository implementation.

**Deliverable**: Working database context with generic repository implementation.

#### Tasks:

1. **Database Setup**
   - Create `MedicalCenterDbContext` class
   - Configure connection string in `appsettings.json`
   - Set up Docker Compose for SQL Server (or PostgreSQL)
   - Create initial migration

2. **Repository Implementation**
   - Implement `Repository<T>` using `Ardalis.Specification.EntityFrameworkCore`
   - Register repository in DI container
   - Create base specification evaluator setup

3. **Audit Interceptors**
   - Create EF Core interceptor (`AuditableEntityInterceptor`) that:
     - Detects entities implementing `IAuditableEntity`
     - Sets `CreatedAt` when entity is added (EntityState.Added)
     - Sets `UpdatedAt` when entity is modified (EntityState.Modified)
     - Does NOT affect entities that don't implement `IAuditableEntity` (e.g., ActionLog)
   - Register interceptor in DbContext
   - Ensure interceptor gets current user context (for future CreatedBy/UpdatedBy if needed)

4. **Integration Tests Setup**
   - Create test database setup/teardown
   - Test repository basic operations (Add, GetById, List)
   - Test specification pattern

5. **Update README.md**
   - Database setup instructions
   - How to run migrations
   - Connection string configuration

**Verification**:
- ✅ Database migrations work
- ✅ Repository can add and retrieve entities
- ✅ Specifications work with repository
- ✅ Integration tests pass
- ✅ Can connect to database

---

### Phase 4: Identity System Foundation

**Goal**: Implement ASP.NET Core Identity with custom user types.

**Deliverable**: Working identity system with user registration and authentication.

#### Tasks:

1. **User Domain Model**
   - Create abstract `User` base class (inherits from BaseEntity)
   - Create `Patient`, `Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter` classes
   - Create `UserRole` enum

2. **Identity Configuration**
   - Configure ASP.NET Core Identity in Infrastructure
   - Create custom `UserStore` for role-specific users
   - Set up Identity DbContext (or integrate with main DbContext)
   - Configure Identity options (password requirements, lockout, etc.)

3. **Identity Services**
   - Create `IIdentityService` interface in Core
   - Implement `IdentityService` in Infrastructure
   - Create `ITokenProvider` interface in Core
   - Implement `TokenProvider` in Infrastructure (JWT)
   - Include role and custom claims in JWT tokens

4. **Database Schema**
   - Create migrations for Users table and role-specific tables
   - Create SQL views for user aggregates (using views approach)
   - Configure EF Core mappings for user hierarchy
   - Seed initial roles (SystemAdmin, Patient, Doctor, etc.)

5. **Authorization Policies Setup**
   - Register authorization policies in Infrastructure layer
   - Define role-based policies (RequirePatient, RequireDoctor, RequireProvider, etc.)
   - Define claims-based policies for fine-grained control
   - Create policy registration extension method

6. **Authentication Endpoints**
   - Create login endpoint (POST /auth/login)
   - Create patient registration endpoint (POST /patients)
   - Configure JWT authentication middleware
   - Configure authorization middleware with policies

7. **Tests**
   - ✅ **Domain Object Tests** (Completed):
     - Test `BaseEntity` behavior (ID generation)
     - Test `ValueObject` equality and comparison
     - Test `User` activation/deactivation behavior
     - Test `Patient` creation and properties
     - Test `Doctor` creation and properties
     - Test `HealthcareEntity` creation and properties
     - Test `Laboratory` creation and properties
     - Test `ImagingCenter` creation and properties
     - Test `Result<T>` pattern behavior
     - Test `Error` class behavior
   - ⏳ Test user registration (patient self-registration)
   - ⏳ Test login and token generation (verify claims included)
   - ⏳ Test authorization policies (each policy allows/denies correctly)
   - ⏳ Test resource-based authorization (patients can only access own data)
   - ⏳ Test JWT token validation

7. **Update README.md**
   - Authentication flow documentation
   - How to register a patient
   - How to obtain JWT token
   - API endpoint documentation

**Verification**:
- ✅ Can register a patient
- ✅ Can login and receive JWT token
- ✅ Token can be used for authenticated requests
- ✅ Authorization policies work
- ✅ All tests pass

---

### Phase 5: Patient Aggregate & Medical Attributes

**Goal**: Implement Patient aggregate with medical attributes domain model.

**Deliverable**: Working Patient aggregate with medical attributes management.

#### Tasks:

1. **Patient Aggregate**
   - Complete `Patient` class with medical attributes collections
   - Create `Allergy`, `ChronicDisease`, `Medication`, `Surgery` entities
   - Create `BloodType` value object
   - Add domain methods for managing medical attributes

2. **Domain Rules Implementation**
   - Implement business rules (only providers can modify certain attributes)
   - Add guard clauses and validation
   - Create domain exceptions if needed

3. **Specifications**
   - `PatientByIdSpecification`
   - `ActivePatientsSpecification`
   - `PatientsByNationalIdSpecification`

4. **EF Core Mappings**
   - Configure Patient aggregate mappings
   - Configure medical attribute entity mappings
   - Configure BloodType as owned entity

5. **Patient Endpoints**
   - GET /patients/self (get own patient data)
   - PUT /patients/self/medical-attributes (update medical attributes - with authorization)
   - GET /patients/self/medical-attributes

6. **Tests**
   - Test Patient aggregate business rules
   - Test medical attribute management
   - Test patient endpoints (integration tests)

7. **Update README.md**
   - Patient aggregate documentation
   - Medical attributes API documentation
   - Business rules explanation

**Verification**:
- ✅ Patient aggregate enforces business rules
- ✅ Medical attributes can be added/updated (with proper authorization)
- ✅ Patient endpoints work correctly
- ✅ All tests pass

---

### Phase 5.1: Authorization & User Management

**Goal**: Fix domain rule violations - patients cannot update their own medical attributes. Implement proper authorization and system admin user management.

**Deliverable**: Secure medical attribute updates with role-based authorization, system admin user seeding, and admin endpoints for user management.

#### Tasks:

1. **Authorization Policies**
   - Update `CanModifyMedicalAttributes` policy to include SystemAdmin
   - Verify policies correctly restrict access

2. **System Admin Seeding**
   - Create `SystemAdminSeeder` to seed `sys.admin@medicalcenter.com`
   - Integrate with existing seeding mechanism

3. **Endpoint Refactoring**
   - Change `PUT /patients/self/medical-attributes` to `PUT /patients/{patientId}/medical-attributes`
   - Remove patient self-update capability
   - Require `CanModifyMedicalAttributes` authorization policy
   - Accept `patientId` as route parameter

4. **System Admin Endpoints**
   - Create admin endpoints for user management (CRUD)
   - `POST /admin/users` - Create user (all types)
   - `GET /admin/users/{userId}` - Get user
   - `GET /admin/users` - List users (with filtering)
   - `PUT /admin/users/{userId}` - Update user
   - `DELETE /admin/users/{userId}` - Delete/deactivate user

5. **Identity Service Updates**
   - Extend `CreateUserAsync` to support all user types
   - Add role-specific properties (specialty, organizationName, etc.)

6. **Tests**
   - Test authorization policies
   - Test system admin seeder
   - Test user creation for all user types
   - Test medical attribute update authorization

**Verification**:
- ✅ Authorization policy updated (CanModifyMedicalAttributes includes SystemAdmin)
- ✅ Endpoint refactored (PUT /patients/{patientId}/medical-attributes with authorization)
- ✅ Domain tests created and passing (131 tests total)
- ✅ Domain rules verified (medical attribute constraints, user creation rules)
- ⏳ System admin seeder (infrastructure - manual verification)
- ⏳ System admin endpoints (infrastructure/presentation - manual verification)
- ⏳ Identity service extension (infrastructure - manual verification)

**Note**: Following classical school of testing, only domain tests are implemented. Infrastructure and presentation layer components are verified manually.

**See**: [Phase5.1_AuthorizationAndUserManagement.md](Phase5.1_AuthorizationAndUserManagement.md) for detailed implementation plan.

---

### Phase 6: Medical Records & Encounters

**Goal**: Implement MedicalRecord and Encounter aggregates.

**Deliverable**: Working medical records system with automatic encounter creation.

#### Tasks:

1. **MedicalRecord Aggregate**
   - Create `MedicalRecord` class
   - Implement business rules (only creator can modify, attachments immutable)
   - Create `RecordType` enum

2. **Encounter Aggregate**
   - Create `Encounter` class
   - Create `EncounterType` enum
   - Implement encounter creation logic

3. **Domain Services**
   - Create `EncounterCreationService` (handles automatic encounter creation)
   - Integrate with MedicalRecord creation

4. **Specifications**
   - `MedicalRecordByIdSpecification`
   - `MedicalRecordsByPatientSpecification`
   - `MedicalRecordsByProviderSpecification`
   - `EncountersByPatientSpecification`
   - `EncountersByProviderSpecification`

5. **EF Core Mappings**
   - Configure MedicalRecord mappings
   - Configure Encounter mappings
   - Set up relationships

6. **Provider Endpoints (Basic)**
   - POST /doctors/records (create medical record as doctor)
   - GET /doctors/records (list doctor's records with filters)
   - GET /doctors/encounters (list doctor's encounters)

7. **Tests**
   - Test MedicalRecord business rules
   - Test automatic encounter creation
   - Test provider endpoints

8. **Update README.md**
   - Medical records API documentation
   - Encounter creation flow
   - Provider endpoints documentation

**Verification**:
- ✅ Medical records can be created
- ✅ Encounters are created automatically
- ✅ Business rules are enforced
- ✅ Provider endpoints work
- ✅ All tests pass

---

### Phase 7: Query Services & Provider Lookups

**Goal**: Implement query services for non-aggregate entities (providers).

**Deliverable**: Working query services for provider entities.

#### Tasks:

1. **Query Service Interfaces (Core)**
   - Create `IProviderQueryService` interface
   - Create `IUserQueryService` interface
   - Define DTOs for provider data

2. **Query Service Implementations (Infrastructure)**
   - Implement `ProviderQueryService` using DbContext
   - Implement `UserQueryService`
   - Create specifications for provider queries (optional)

3. **Provider Endpoints Enhancement**
   - GET /doctors (list all doctors with filters)
   - GET /doctors/{id} (get doctor details)
   - Similar endpoints for other provider types

4. **Tests**
   - Test query services
   - Test provider lookup endpoints

5. **Update README.md**
   - Query services documentation
   - Provider lookup API documentation

**Verification**:
- ✅ Can query providers through query services
- ✅ Provider endpoints return correct data
- ✅ All tests pass

---

### Phase 8: Action Logging & Audit Trail

**Goal**: Implement action logging for audit and compliance.

**Deliverable**: Working audit trail system.

#### Tasks:

1. **ActionLog Aggregate**
   - Create `ActionLog` class
   - Define action types enum
   - Implement action logging domain logic

2. **Action Logging Service**
   - Create `IActionLogService` interface (Core)
   - Implement `ActionLogService` (Infrastructure)
   - Integrate with endpoints via middleware or filters

3. **Audit Interceptors Enhancement**
   - Enhance EF Core interceptors for comprehensive audit logging
   - Log data access and modifications

4. **Action Log Endpoints**
   - GET /patients/self/action-logs (patient can see their action logs)
   - GET /admin/action-logs (admin can see all action logs)

5. **Tests**
   - Test action logging
   - Test audit interceptors
   - Test action log endpoints

6. **Update README.md**
   - Action logging documentation
   - Audit trail features

**Verification**:
- ✅ Actions are logged correctly
- ✅ Patients can view their action logs
- ✅ Audit trail is comprehensive
- ✅ All tests pass

---

### Phase 9: Complete Provider Endpoints

**Goal**: Implement all provider-specific endpoints (Healthcare, Labs, Imaging).

**Deliverable**: Complete provider endpoint implementation.

#### Tasks:

1. **Healthcare Entity Endpoints**
   - POST /healthcare/records
   - GET /healthcare/records
   - GET /healthcare/encounters

2. **Laboratory Endpoints**
   - POST /labs/records
   - GET /labs/records
   - GET /labs/encounters

3. **Imaging Center Endpoints**
   - POST /imaging/records
   - GET /imaging/records
   - GET /imaging/encounters

4. **Authorization**
   - Ensure proper role-based authorization for each endpoint
   - Test authorization policies

5. **Tests**
   - Integration tests for all provider endpoints
   - Test authorization for each role

6. **Update README.md**
   - Complete API documentation
   - All endpoint references

**Verification**:
- ✅ All provider endpoints work
- ✅ Authorization is correct
- ✅ All tests pass

---

### Phase 10: Admin Features

**Goal**: Implement admin management endpoints.

**Deliverable**: Complete admin functionality.

#### Tasks:

1. **User Management Endpoints**
   - POST /admin/users (create user - non-patients)
   - GET /admin/users (list users with filters)
   - GET /admin/users/{id}
   - PUT /admin/users/{id}
   - DELETE /admin/users/{id}

2. **Records Management Endpoints**
   - GET /admin/records (list all records with filters)
   - GET /admin/records/{id}

3. **Encounters Management Endpoints**
   - GET /admin/encounters (list all encounters with filters)
   - GET /admin/encounters/{id}

4. **Admin Query Services**
   - Create admin-specific query services if needed
   - Implement complex filtering

5. **Tests**
   - Test all admin endpoints
   - Test admin authorization

6. **Update README.md**
   - Admin API documentation
   - Admin workflows

**Verification**:
- ✅ Admin can manage users
- ✅ Admin can view all records and encounters
- ✅ Authorization is enforced
- ✅ All tests pass

---

### Phase 11: Patient Self-Service Features

**Goal**: Complete patient self-service endpoints.

**Deliverable**: Full patient self-service functionality.

#### Tasks:

1. **Patient Endpoints Completion**
   - GET /patients/self/records (view own records)
   - GET /patients/self/report (generate patient report)
   - Any additional patient self-service features

2. **Patient Report Generation**
   - Implement report generation logic
   - Include medical records, encounters, medical attributes

3. **Tests**
   - Test all patient self-service endpoints
   - Test report generation

4. **Update README.md**
   - Patient self-service documentation
   - Report generation features

**Verification**:
- ✅ Patients can access all their data
- ✅ Reports are generated correctly
- ✅ All tests pass

---

### Phase 12: Testing & Quality Assurance

**Goal**: Comprehensive testing and code quality improvements.

**Deliverable**: Well-tested, production-ready application.

#### Tasks:

1. **Unit Tests**
   - Complete unit tests for domain logic
   - Test all business rules
   - Test value objects and entities

2. **Integration Tests**
   - Test all endpoints
   - Test database operations
   - Test authentication/authorization flows

3. **Code Quality**
   - Run code analysis
   - Fix code smells
   - Ensure consistent code style

4. **Performance Testing**
   - Identify performance bottlenecks
   - Optimize queries
   - Add indexes where needed

5. **Documentation**
   - Complete API documentation (Swagger)
   - Update README with complete setup guide
   - Add architecture diagrams if needed

6. **Update README.md**
   - Complete setup instructions
   - Testing guide
   - Deployment considerations

**Verification**:
- ✅ High test coverage for critical paths
- ✅ All tests pass
- ✅ Code quality is good
- ✅ Documentation is complete

---

## 9. Phase Deliverables Summary

Each phase produces a working, testable deliverable:

- **Phase 1**: Solution structure, Git repo, README
- **Phase 2**: Core foundation with base classes
- **Phase 3**: Database and repository working
- **Phase 4**: Identity system with authentication
- **Phase 5**: Patient aggregate with medical attributes
- **Phase 6**: Medical records and encounters
- **Phase 7**: Provider query services
- **Phase 8**: Action logging and audit trail
- **Phase 9**: Complete provider endpoints
- **Phase 10**: Admin management features
- **Phase 11**: Patient self-service complete
- **Phase 12**: Production-ready, well-tested application

## 10. README Maintenance Strategy

The README.md file should be updated at the end of each phase to include:
- New features added in that phase
- New API endpoints
- Updated setup instructions
- New dependencies or configuration
- Testing instructions for new features
- Links to relevant documentation

This ensures the README remains a living document that accurately reflects the current state of the application.

---

## 9. Technology Stack

### Core Technologies
- **.NET 8+**: Runtime and framework
- **C# 12**: Programming language
- **Entity Framework Core 8**: ORM
- **ASP.NET Core Identity**: Authentication/Authorization

### Key Libraries
- **FastEndpoints**: API framework
- **Ardalis.Specification**: Query pattern
- **Ardalis.GuardClauses**: Guard clauses
- **FluentValidation**: Validation
- **AutoMapper**: Object mapping

### Testing
- **xUnit**: Test framework
- **FluentAssertions**: Assertions
- **Moq/NSubstitute**: Mocking (for unmanaged dependencies only)

### Database
- **SQL Server** (or PostgreSQL): Primary database
- **Docker**: Local database setup for development/testing

---

## 10. Next Steps

This high-level plan provides the architectural foundation. Detailed implementation will follow, including:

- Detailed domain model design
- Specific endpoint implementations
- Database schema details
- Validation rules
- Authorization policies
- Testing strategies per layer

Each phase will be implemented following TDD principles, with tests driving the design and ensuring quality.

