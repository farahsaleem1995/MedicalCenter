# Medical Center System - Architecture Documentation

## Overview

The Medical Center Automation System is built using **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The system follows a three-layer architecture that separates concerns and maintains clear boundaries between domain logic, infrastructure, and presentation.

## Architecture Layers

### 1. Core Layer (Domain)

The Core layer contains the domain model and business logic. It has no dependencies on infrastructure or presentation concerns.

#### Key Components

The Core layer is organized around domain concepts, not technical classifications:

- **Common**: Shared abstractions, base classes, and common concepts
  - `BaseEntity`: Base class for all entities with `Id` (Guid)
  - `User`: Abstract base class for all users
  - `ValueObject`: Base class for value objects
  - `IRepository<T>`: Generic repository interface for aggregate roots
  - `IUnitOfWork`: Unit of Work interface for transaction management
  - `Attachment`: File attachment value object (common concept)
  - `UserRole`: User role enumeration (common concept)
  - `ProviderType`: Healthcare provider type enumeration (common concept)
  - `Result<T>`, `Error`: Operation outcome pattern

- **Aggregates**: Consistency boundaries organized by domain concepts
  - `Patient`: Root aggregate containing medical attributes
    - Contains: `BloodType` value object, `BloodABO` enum, `BloodRh` enum
    - Medical attributes: Allergies, ChronicDiseases, Medications, Surgeries
  - `MedicalRecord`: Medical record aggregate with file attachments
    - Contains: `RecordType` enum
    - Contains: `Practitioner` value object (snapshot)
  - `Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`: Practitioner aggregate roots
  - `Encounter`: (Planned) Encounter aggregate (requires domain events)
  - `ActionLog`: (Planned) Audit log aggregate

- **Specifications**: Encapsulate complex business queries
  - Uses `Ardalis.Specification` pattern
  - Example: `PatientByIdSpecification`

- **Domain Services**: Operations that don't fit within a single entity
  - `IIdentityService`: User identity management interface
  - `IFileStorageService`: File storage abstraction interface

#### Design Principles

- **No Infrastructure Dependencies**: Core layer is pure domain logic
- **Aggregate Boundaries**: Only aggregate roots are accessible via repositories
- **Value Objects**: Immutable, equality-based objects
- **Specification Pattern**: Complex queries encapsulated in specifications
- **Domain Organization**: Code organized around domain concepts, not technical classifications
  - Aggregate-specific types (enums, value objects) live within their aggregates
  - Common abstractions and shared concepts live in Common folder

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
  - Handles user creation, password management
  - Supports practitioner user creation (Doctor, HealthcareEntity, Laboratory, ImagingCenter)

- **Query Services**:
  - `UserQueryService`: Query service for non-aggregate user entities
  - Supports pagination via `PaginatedList<T>`
  - Handles role-based filtering and query filters

- **File Storage Service**:
  - `LocalFileStorageService`: Local filesystem implementation of `IFileStorageService`
  - Stores files in configurable directory
  - Supports file upload, download, and deletion
  - Can be replaced with cloud storage implementation (Azure Blob, S3, etc.)

- **Audit Interceptors**:
  - `AuditableEntityInterceptor`: Automatically sets `CreatedAt` and `UpdatedAt`
  - Only affects entities implementing `IAuditableEntity`

- **Dependency Injection**:
  - `DependencyInjection.AddInfrastructure()`: Registers all infrastructure services
  - Scoped lifetime for DbContext and repositories

#### Database Schema

- **Identity Tables**: ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.)
  - `ApplicationUserRole`: Custom user-role join entity with navigation properties
  - Configured directly in `IdentityDbContext` generics to avoid inheritance mapping
- **Domain Tables**: Patient, MedicalRecord, MedicalRecordAttachments, Doctor, HealthcareEntity, Laboratory, ImagingCenter
- **Relationships**: 
  - Practitioner aggregates use shared primary key with ApplicationUser
  - MedicalRecord references Patient and Practitioner (practitioner snapshot as value object)
  - MedicalRecordAttachments is owned entity collection (part of MedicalRecord aggregate)

### 3. Web API Layer (Presentation)

The Web API layer handles HTTP requests, validation, authorization, and DTOs.

#### Key Components

- **FastEndpoints**: API framework for endpoint definition
  - Endpoint classes inherit from `Endpoint<TRequest, TResponse>`
  - Built-in validation and authorization support
  - Route prefix: All endpoints prefixed with `/api`
  - Error handling: Problem Details format for standardized error responses

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

## Security Architecture

### Authentication

- **JWT Bearer Tokens**: Stateless authentication
- **Refresh Tokens**: Long-lived tokens for token renewal
- **ASP.NET Core Identity**: User and password management

### Authorization

- **Role-Based Access Control (RBAC)**: User roles (SystemAdmin, Patient, Doctor, etc.)
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
3. Aggregate enforces business rules
4. EF Core saves changes
5. Response returned

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
  - `Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`: `HasQueryFilter(x => x.IsActive)`
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

- **Domain Events**: For cross-aggregate communication
- **CQRS**: Separate read and write models if needed
- **Event Sourcing**: For audit trail requirements
- **Microservices**: Split into bounded contexts if needed

