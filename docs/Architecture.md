# Medical Center System - Architecture Documentation

## Overview

The Medical Center Automation System is built using **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The system follows a three-layer architecture that separates concerns and maintains clear boundaries between domain logic, infrastructure, and presentation.

## Architecture Layers

### 1. Core Layer (Domain)

The Core layer contains the domain model and business logic. It has no dependencies on infrastructure or presentation concerns.

#### Key Components

- **Entities**: Domain entities representing business concepts
  - `BaseEntity`: Base class for all entities with `Id` (Guid)
  - `User`: Abstract base class for all users
  - `Patient`: Aggregate root for patients
  - `Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`: Provider entities

- **Aggregates**: Consistency boundaries
  - `Patient`: Root aggregate containing medical attributes (Allergies, ChronicDiseases, Medications, Surgeries)
  - `MedicalRecord`: (Planned) Medical record aggregate
  - `Encounter`: (Planned) Encounter aggregate
  - `ActionLog`: (Planned) Audit log aggregate

- **Value Objects**: Immutable objects defined by their attributes
  - `BloodType`: ABO type and Rh factor combination
  - `Result<T>`: Operation outcome pattern

- **Specifications**: Encapsulate complex business queries
  - Uses `Ardalis.Specification` pattern
  - Example: `PatientByIdSpecification`

- **Repository Interfaces**: Abstractions for data access
  - `IRepository<T>`: Generic repository for aggregate roots only

- **Domain Services**: Operations that don't fit within a single entity
  - `IIdentityService`: User identity management interface

#### Design Principles

- **No Infrastructure Dependencies**: Core layer is pure domain logic
- **Aggregate Boundaries**: Only aggregate roots are accessible via repositories
- **Value Objects**: Immutable, equality-based objects
- **Specification Pattern**: Complex queries encapsulated in specifications

### 2. Infrastructure Layer

The Infrastructure layer implements data access and external service integrations.

#### Key Components

- **Entity Framework Core**:
  - `MedicalCenterDbContext`: Main database context
  - Entity configurations for all domain entities
  - Migrations for database schema management

- **Repository Implementation**:
  - `Repository<T>`: Generic repository using `Ardalis.Specification.EntityFrameworkCore`
  - Works only with aggregate roots

- **Identity Service**:
  - `IdentityService`: Implements `IIdentityService`
  - Handles user creation, password management
  - Supports provider user creation (Doctor, HealthcareEntity, Laboratory, ImagingCenter)

- **Query Services**:
  - `UserQueryService`: Query service for non-aggregate user entities
  - Supports pagination via `PaginatedList<T>`
  - Handles role-based filtering and query filters

- **Audit Interceptors**:
  - `AuditableEntityInterceptor`: Automatically sets `CreatedAt` and `UpdatedAt`
  - Only affects entities implementing `IAuditableEntity`

- **Dependency Injection**:
  - `DependencyInjection.AddInfrastructure()`: Registers all infrastructure services
  - Scoped lifetime for DbContext and repositories

#### Database Schema

- **Identity Tables**: ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.)
- **Domain Tables**: Patient, Doctor, HealthcareEntity, Laboratory, ImagingCenter
- **Relationships**: Provider entities use shared primary key with ApplicationUser

### 3. Web API Layer (Presentation)

The Web API layer handles HTTP requests, validation, authorization, and DTOs.

#### Key Components

- **FastEndpoints**: API framework for endpoint definition
  - Endpoint classes inherit from `Endpoint<TRequest, TResponse>`
  - Built-in validation and authorization support

- **FluentValidation**: Request validation
  - Validators for all endpoints
  - Enforces business rules at API boundary

- **Authorization Policies**:
  - `RequireAdmin`: System admin access
  - `RequirePatient`: Patient-only access
  - `CanModifyMedicalAttributes`: Provider access to medical attributes

- **Swagger/OpenAPI**:
  - FastEndpoints.Swagger (NSwag) for API documentation
  - Organized by endpoint groups (Admin, Auth, Patients, Allergies, etc.)

- **DTOs**: Data transfer objects for API contracts
  - Request DTOs: Input validation
  - Response DTOs: Output formatting

#### Endpoint Organization

- **Auth Group**: Authentication endpoints (register, login, refresh token)
- **Admin Group**: User management endpoints (CRUD operations)
- **Patients Group**: Patient self-service endpoints
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
- **Implementation**: `IUserQueryService` for provider entities
- **Benefits**: Separation of read operations from aggregate boundaries

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

- **Provider Entities**: Share primary key with `ApplicationUser` (one-to-one)
- **Patient**: Aggregate root with collections for medical attributes
- **Medical Attributes**: Owned by Patient aggregate (Allergy, ChronicDisease, Medication, Surgery)

### Query Filters

- **Global Query Filters**: EF Core filters for soft-delete (`IsActive`)
- **Admin Override**: `IgnoreQueryFilters()` for admin operations

### Migrations

- **Code-First Approach**: Database schema generated from entity configurations
- **Migration Management**: EF Core migrations for version control

## Error Handling

### Exception Handling

- **Global Exception Middleware**: Catches unhandled exceptions
- **Structured Error Responses**: Consistent error format
- **Domain Exceptions**: Business rule violations

### Validation Errors

- **FluentValidation**: Returns 400 Bad Request with validation errors
- **Model State**: Automatic model validation

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

