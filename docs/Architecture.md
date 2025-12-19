# Medical Center System - Architecture Documentation

## Overview

The Medical Center Automation System is built using **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The system follows a three-layer architecture that separates concerns and maintains clear boundaries between domain logic, infrastructure, and presentation.

## Architecture Layers

### 1. Core Layer (Domain)

The Core layer contains the domain model and business logic. It has no dependencies on infrastructure or presentation concerns.

#### Key Components

The Core layer is organized following Domain-Driven Design principles with clear separation of concerns:

- **Abstractions/**: DDD building blocks and framework contracts
  - `BaseEntity`: Base class for all entities with `Id` (Guid)
  - `IAggregateRoot`: Marker interface for aggregate roots
  - `IAuditableEntity`: Interface for entities requiring audit tracking
  - `ValueObject`: Base class for value objects

- **Primitives/**: Result pattern, error handling, and pagination
  - `Result<T>`, `Error`, `ErrorCodes`: Operation outcome pattern
  - `ResultExtensions`: Extension methods for Result operations
  - `Pagination/`: `PaginatedList<T>`, `PaginationMetadata` for standardized pagination

- **SharedKernel/**: Shared domain concepts (ubiquitous language)
  - `User`: Abstract base class for all users
  - `UserRole`: User role enumeration
  - `ProviderType`: Healthcare provider type enumeration
  - `Attachment`: File attachment value object (shared concept)
  - `IRepository<T>`: Generic repository interface for aggregate roots (domain concept)
  - `IUnitOfWork`: Unit of Work interface for transaction management (domain concept)
  - `Events/`: Domain event base types (`IDomainEvent`, `DomainEventBase`, `IHasDomainEvents`)
    - `DomainEventBase`: Base class implementing `INotification` (MediatR) for all domain events
    - `IHasDomainEvents`: Interface for entities that can raise domain events
    - All domain events inherit from `DomainEventBase` and are dispatched via MediatR

- **Aggregates/**: Core domain model organized by bounded contexts
  - **Patients/**: Patient aggregate with medical attributes
    - `Patient`: Aggregate root
    - `Entities/`: Allergy, ChronicDisease, Medication, Surgery
    - `ValueObjects/`: BloodType
    - `Enums/`: BloodABO, BloodRh
    - `Specifications/`: PatientByIdSpecification, ActivePatientsSpecification, etc.
    - `Events/`: Patient-specific domain events (e.g., `PatientRegisteredEvent`)
  - **Doctors/**: Doctor aggregate
    - `Doctor`: Aggregate root
    - `Events/`: (Future) Doctor-specific domain events
  - **HealthcareStaff/**: Healthcare staff aggregate (renamed from HealthcareEntity)
    - `HealthcareStaff`: Aggregate root
    - `Events/`: (Future) HealthcareStaff-specific domain events
  - **Laboratories/**: Laboratory aggregate
    - `Laboratory`: Aggregate root
    - `Events/`: (Future) Laboratory-specific domain events
  - **ImagingCenters/**: Imaging center aggregate
    - `ImagingCenter`: Aggregate root
    - `Events/`: (Future) ImagingCenter-specific domain events
  - **SystemAdmins/**: System administrator aggregate
    - `SystemAdmin`: Aggregate root with organizational properties (CorporateId, Department)
    - `Events/`: (Future) SystemAdmin-specific domain events
  - **MedicalRecords/**: Medical record aggregate
    - `MedicalRecord`: Aggregate root
    - `ValueObjects/`: Practitioner (snapshot)
    - `Enums/`: RecordType
    - `Specifications/`: MedicalRecordByIdSpecification, MedicalRecordsByPatientSpecification
    - `Events/`: (Future) MedicalRecord-specific domain events
  - `Encounter`: (Planned) Encounter aggregate (requires domain events)
  - `ActionLog`: (Planned) Audit log aggregate

- **Queries/**: Query service interfaces for read operations
  - `IMedicalRecordQueryService`: Optimized queries for medical records
  - `IUserQueryService`: Optimized queries for user entities

- **Services/**: Domain service interfaces
  - `IIdentityService`: User identity management interface (includes claims and policy verification)
  - `IUserContext`: User context interface for accessing authenticated user information (UserId, UserName, Email, Role)
  - `IFileStorageService`: File storage abstraction interface
  - `ITokenProvider`: Token generation and validation interface
  - `IDateTimeProvider`: Unified time access interface

- **Authorization/**: Claims-based authorization infrastructure
  - `IdentityClaimTypes`: Claim type constants (AdminTier, Department, Certification)
  - `IdentityClaimValues`: Well-known claim values (AdminTier.Super, AdminTier.Standard)
  - `ClaimBasedPolicies`: Authorization policy names (CanManageAdmins, CanViewAuditTrail, CanAccessPHI)

#### Design Principles

- **No Infrastructure Dependencies**: Core layer is pure domain logic
- **Aggregate Boundaries**: Only aggregate roots are accessible via repositories
- **Value Objects**: Immutable, equality-based objects
- **Specification Pattern**: Complex queries encapsulated in specifications
- **Domain Organization**: Code organized following DDD principles
  - Aggregate-specific types (enums, value objects, entities, specifications) live within their aggregates
  - DDD building blocks (BaseEntity, ValueObject) in Abstractions/
  - Cross-cutting technical concerns (Result pattern, Pagination) in Primitives/
  - Shared domain concepts (User, Repository, Domain Events) in SharedKernel/
  - Query interfaces separated from domain services in Queries/

### 2. Infrastructure Layer

The Infrastructure layer implements data access and external service integrations.

#### Key Components

- **Entity Framework Core**:
  - `MedicalCenterDbContext`: Main database context
    - Inherits from `IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, ...>`
    - Uses `ApplicationUserRole` directly (no inheritance mapping, no discriminator column)
  - Entity configurations for all domain entities
  - Migrations for database schema management

- **Repository Implementation**:
  - `Repository<T>`: Generic repository using `Ardalis.Specification.EntityFrameworkCore`
  - Works only with aggregate roots

- **Identity Service**:
  - `IdentityService`: Implements `IIdentityService`
  - Handles Identity user creation (`CreateUserAsync`) - creates `ApplicationUser` only
  - Domain entity creation is handled by endpoints (following `RegisterPatientEndpoint` pattern)
  - Password management: `ChangePasswordAsync` (requires current password), `UpdatePasswordAsync` (admin password reset)
  - Claims verification: Database-only claims stored in `AspNetUserClaims` table (not in JWT tokens)
  - Policy verification: `SatisfiesPolicyAsync()` evaluates claims and roles via database lookup

- **Query Services**:
  - `UserQueryService`: Query service for non-aggregate user entities
  - Supports pagination via `PaginatedList<T>`
  - Handles role-based filtering and query filters

- **File Storage Service**:
  - `LocalFileStorageService`: Local filesystem implementation of `IFileStorageService`
  - Stores files in configurable directory
  - Supports file upload, download, and deletion
  - Can be replaced with cloud storage implementation (Azure Blob, S3, etc.)

- **Time Provider Service**:
  - `DateTimeProvider`: Implementation of `IDateTimeProvider` that returns UTC time
  - Registered as singleton (stateless, thread-safe)
  - Provides unified time access across the application
  - Enables testability by allowing time to be mocked in tests

- **EF Core Interceptors**:
  - `AuditableEntityInterceptor`: Automatically sets `CreatedAt` and `UpdatedAt`
    - Uses `IDateTimeProvider` for consistent time handling
    - Only affects entities implementing `IAuditableEntity`
  - `DomainEventDispatcherInterceptor`: Dispatches domain events after `SaveChanges`
    - Collects events from entities implementing `IHasDomainEvents`
    - Dispatches events via MediatR after successful save
    - Clears events after dispatch to prevent duplicate handling

- **MediatR Integration**:
  - Registered in `DependencyInjection.AddInfrastructure()`
  - Scans Core assembly for `INotificationHandler<T>` implementations
  - Handlers registered in Infrastructure layer (e.g., `PatientRegisteredEventHandler`)

- **Dependency Injection**:
  - `DependencyInjection.AddInfrastructure()`: Registers all infrastructure services
  - Scoped lifetime for DbContext and repositories
  - MediatR registered for domain event dispatching

#### Database Schema

- **Identity Tables**: ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.)
  - `ApplicationUserRole`: Custom user-role join entity with navigation properties
  - Configured directly in `IdentityDbContext` generics to avoid inheritance mapping
- **Domain Tables**: Patient, MedicalRecord, MedicalRecordAttachments, Doctor, HealthcareStaff, Laboratory, ImagingCenter, SystemAdmins
- **Relationships**: 
  - Practitioner aggregates (Doctor, HealthcareStaff, Laboratory, ImagingCenter, SystemAdmin) use shared primary key with ApplicationUser
  - MedicalRecord references Patient and Practitioner (practitioner snapshot as value object)
  - MedicalRecordAttachments is owned entity collection (part of MedicalRecord aggregate)
  - Claims stored in `AspNetUserClaims` table (database-only, not in JWT tokens)

### 3. Web API Layer (Presentation)

The Web API layer handles HTTP requests, validation, authorization, and DTOs.

#### Key Components

- **FastEndpoints**: API framework for endpoint definition
  - Endpoint classes inherit from `Endpoint<TRequest, TResponse>`
  - Built-in validation and authorization support
  - Route prefix: All endpoints prefixed with `/api`
  - Error handling: Problem Details format for standardized error responses

- **CQRS Separation**:
  - Commands: Endpoints that modify state (POST, PUT, DELETE, PATCH)
  - Queries: Endpoints that only read data (GET)
  - Clear separation of read and write operations for better maintainability

- **FluentValidation**: Request validation
  - Validators for all endpoints
  - Enforces business rules at API boundary

- **Authorization Policies**:
  - `RequireAdmin`: System admin access
  - `RequirePatient`: Patient-only access
  - `CanViewMedicalAttributes`: View medical attributes (Doctor, HealthcareStaff, SystemAdmin)
  - `CanModifyMedicalAttributes`: Modify medical attributes (Doctor, HealthcareStaff, SystemAdmin)
  - `CanViewRecords`: View records (Doctor, HealthcareStaff, LabUser, ImagingUser)
  - `CanModifyRecords`: Modify records (Doctor, HealthcareStaff, LabUser, ImagingUser)
  - `CanViewAllPatients`: View all patients (Doctor, HealthcareStaff, SystemAdmin)

- **Swagger/OpenAPI**:
  - FastEndpoints.Swagger (NSwag) for API documentation
  - Organized by endpoint groups (Admin, Auth, Patients, Allergies, etc.)

- **DTOs**: Data transfer objects for API contracts
  - Request DTOs: Input validation
  - Response DTOs: Output formatting

#### Endpoint Organization

- **Auth Group**: Authentication endpoints (register, login, refresh token)
- **Admin Group**: User management endpoints (CRUD operations)
- **Patients Group**: Patient self-service endpoints (including medical records)
- **Records Group**: Medical records endpoints (create, view, update, delete, file upload/download)
- **Medical Attributes Groups**: Allergies, ChronicDiseases, Medications, Surgeries

## Design Patterns

### Repository Pattern

- **Purpose**: Abstract data access for aggregate roots
- **Implementation**: Generic `IRepository<T>` interface
- **Usage**: Only aggregate roots use repositories; non-aggregates use query services

### Specification Pattern

- **Purpose**: Encapsulate complex queries
- **Implementation**: `Ardalis.Specification` library
- **Benefits**: Reusable, testable, composable queries

### Result Pattern

- **Purpose**: Represent operation outcomes without exceptions
- **Implementation**: `Result<T>` and `Error` classes
- **Usage**: Domain operations return `Result<T>` for expected failures

### Query Service Pattern

- **Purpose**: Query non-aggregate entities
- **Implementation**: `IUserQueryService` for practitioner aggregate roots
- **Benefits**: Separation of read operations from aggregate boundaries

### File Storage Pattern

- **Purpose**: Abstract file storage operations
- **Implementation**: `IFileStorageService` interface in Core, `LocalFileStorageService` in Infrastructure
- **Benefits**: 
  - Domain layer remains pure (no file system dependencies)
  - Easy to swap implementations (local filesystem, cloud storage)
  - Testable via interface

### Pagination Pattern

- **Purpose**: Standardize paginated responses
- **Implementation**: `PaginatedList<T>` and `PaginationMetadata`
- **Usage**: All list endpoints return paginated results

### Domain Events Pattern

- **Purpose**: Enable cross-aggregate communication and side effects
- **Implementation**: 
  - `DomainEventBase` implements MediatR's `INotification`
  - `BaseEntity` implements `IHasDomainEvents` for event collection
  - `DomainEventDispatcherInterceptor` dispatches events after `SaveChanges`
  - Handlers implement `INotificationHandler<T>` in Infrastructure layer
- **Benefits**:
  - Decouples aggregates from side effects
  - Enables event-driven architecture
  - Simple implementation using MediatR
- **Usage**:
  - Aggregates raise events: `AddDomainEvent(new SomeEvent(...))`
  - Events are automatically dispatched after successful save
  - Handlers process events asynchronously

### User Context Pattern

- **Purpose**: Abstract user claim access and provide strongly-typed user information
- **Implementation**: `IUserContext` interface in Core, `AspNetUserContextAdapter` in WebApi
- **Benefits**:
  - Decouples endpoints from ASP.NET Core's `HttpContext.User.Claims`
  - Provides strongly-typed access to user information (Guid UserId instead of string parsing)
  - Centralizes claim validation and error handling
  - Enables easy mocking in tests
  - Descriptive error messages for missing or invalid claims
- **Usage**:
  - All endpoints inject `IUserContext` instead of directly accessing `User.FindFirst()`
  - Properties throw `InvalidOperationException` with descriptive messages when claims are missing or invalid
  - Registered as scoped service (one per HTTP request)
  - Uses standard JWT claims: `ClaimTypes.NameIdentifier`, `ClaimTypes.Name`, `ClaimTypes.Email`, `ClaimTypes.Role`

### Time Handling Pattern

- **Purpose**: Provide unified time access and enable testability
- **Implementation**: `IDateTimeProvider` interface in Core, `DateTimeProvider` in Infrastructure
- **Benefits**:
  - Ensures consistent UTC time across the application
  - Makes time dependencies explicit through dependency injection
  - Enables time to be controlled/mocked in tests
  - Follows testing best practices (avoid ambient context)
- **Usage**: 
  - All infrastructure services, validators, and endpoints use `IDateTimeProvider` instead of `DateTime.UtcNow` or `DateTime.Now`
  - Domain entities receive time as parameters (don't retrieve it directly)
  - Registered as singleton in dependency injection

## Security Architecture

### Authentication

- **JWT Bearer Tokens**: Stateless authentication
- **Refresh Tokens**: Long-lived tokens for token renewal
- **ASP.NET Core Identity**: User and password management

### Authorization

- **Role-Based Access Control (RBAC)**: User roles (SystemAdmin, Patient, Doctor, etc.)
- **Claims-Based Authorization**: Database-only claims for privileged operations
  - Claims describe WHO the user is (identity attributes)
  - Policies describe WHAT the user can do (capabilities)
  - Claims stored in `AspNetUserClaims` table, NOT in JWT tokens (avoids token size issues)
  - Policy verification via `IIdentityService.SatisfiesPolicyAsync()` (database lookup)
- **Policy-Based Authorization**: Custom policies for fine-grained control
- **Resource-Based Authorization**: Users can only access their own resources

### Security Features

- **Password Requirements**: Enforced via Identity options
- **Token Expiration**: Configurable JWT expiration
- **HTTPS**: Enforced in production
- **Input Validation**: FluentValidation on all endpoints

## Data Flow

### Request Flow

1. **HTTP Request** → FastEndpoints receives request
2. **Validation** → FluentValidation validates request DTO
3. **Authorization** → Authorization policies check permissions
4. **Domain Service** → Business logic executed via domain services or repositories
5. **Response** → Response DTO returned to client

### Write Operations

1. Request → Validation → Authorization
2. Domain service/repository → Aggregate root
3. Aggregate enforces business rules and raises domain events
4. EF Core saves changes
5. Domain events dispatched via MediatR (after successful save)
6. Event handlers process side effects
7. Response returned

### Read Operations

1. Request → Validation → Authorization
2. Query service or repository → Database query
3. Results mapped to DTOs
4. Paginated response returned

## Technology Stack

### Core Technologies

- **.NET 10**: Runtime and framework
- **C# 12**: Programming language
- **Entity Framework Core 10**: ORM
- **ASP.NET Core Identity**: Authentication/Authorization

### Key Libraries

- **FastEndpoints**: API framework
- **FastEndpoints.Swagger**: OpenAPI documentation (NSwag)
- **MediatR**: Mediator pattern for domain events
- **Ardalis.Specification**: Query pattern
- **Ardalis.GuardClauses**: Guard clauses
- **FluentValidation**: Request validation

### Testing

- **xUnit**: Test framework
- **FluentAssertions**: Assertions
- **Classical School**: Testing approach (behavior-focused, not implementation-focused)

## Database Design

### Entity Relationships

- **Practitioner Aggregates**: Share primary key with `ApplicationUser` (one-to-one)
- **Patient**: Aggregate root with collections for medical attributes
- **Medical Attributes**: Owned by Patient aggregate (Allergy, ChronicDisease, Medication, Surgery)

### Query Filters

- **Global Query Filters**: EF Core filters for soft-delete (`IsActive`)
  - `Patient`: `HasQueryFilter(p => p.IsActive)`
  - `Allergy`, `ChronicDisease`, `Medication`, `Surgery`: Matching filters `HasQueryFilter(x => x.Patient.IsActive)`
  - `Doctor`, `HealthcareStaff`, `Laboratory`, `ImagingCenter`, `SystemAdmin`: `HasQueryFilter(x => x.IsActive)`
- **Matching Filters**: Child entities (medical attributes) have matching query filters to prevent inconsistent states when parent is filtered out
- **Admin Override**: `IgnoreQueryFilters()` for admin operations

### Migrations

- **Code-First Approach**: Database schema generated from entity configurations
- **Migration Management**: EF Core migrations for version control

## Error Handling

### Exception Handling

- **Global Exception Handler**: Implements `IExceptionHandler` interface for centralized exception handling
- **Problem Details Format**: All errors (validation, business logic, and unexpected exceptions) use RFC 7807 Problem Details format
- **Structured Error Responses**: Consistent error format across all endpoints
- **Unexpected Exceptions**: All unhandled exceptions return 500 Internal Server Error with generic message
- **Trace ID**: Exception responses include trace ID for correlation and debugging
- **Domain Exceptions**: Business rule violations handled via Result pattern

### Validation Errors

- **FluentValidation**: Request validation using FastEndpoints' `Validator<T>` base class
- **Validation Error Format**: Returns 400 Bad Request with Problem Details format including field-specific errors
- **Automatic Validation**: FastEndpoints automatically validates requests before endpoint execution

## Performance Considerations

### Database Queries

- **Eager Loading**: Use `Include()` for related entities
- **Pagination**: All list operations are paginated
- **Query Filters**: Efficient filtering at database level

### Caching

- **Future Consideration**: Add caching for frequently accessed data

## Scalability

### Horizontal Scaling

- **Stateless Design**: JWT tokens enable stateless authentication
- **Database**: Can scale independently

### Vertical Scaling

- **Async/Await**: All I/O operations are asynchronous
- **Connection Pooling**: EF Core connection pooling

## Future Considerations

- **CQRS**: Separate read and write models if needed
- **Event Sourcing**: For audit trail requirements
- **Microservices**: Split into bounded contexts if needed
- **Additional Domain Events**: Expand event coverage for other aggregates (MedicalRecord, Encounter, etc.)

