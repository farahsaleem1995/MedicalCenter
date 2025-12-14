# Medical Center Automation System

A comprehensive medical center management system built with .NET, following Clean Architecture and Domain-Driven Design principles.

## Overview

This system provides a complete solution for managing medical records, patient information, encounters, and provider interactions in a medical center environment. It follows a three-layer architecture (Core, Infrastructure, WebApi) and implements DDD patterns for maintainability and scalability.

## Architecture

The system follows Clean Architecture principles with three main layers:

- **Core Layer**: Domain models, aggregates, value objects, specifications, and repository interfaces
- **Infrastructure Layer**: EF Core, Identity, repository implementations, query services
- **Web API Layer**: FastEndpoints, validation, authorization, DTOs

For detailed architecture documentation, see [docs/ImplementationPlan.md](docs/ImplementationPlan.md).

## Prerequisites

- .NET 8 SDK or later
- Docker (for local database setup)
- SQL Server or PostgreSQL (for production)
- Git (for version control)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd MedicalCenter
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Run the Application

```bash
cd src/MedicalCenter.WebApi
dotnet run
```

The API will be available at `https://localhost:5001` (or the configured port).

## Project Structure

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

**Note**: Currently, only domain unit tests are included. Integration tests for Infrastructure and WebApi will be added in later phases as needed.

## Technology Stack

- **.NET 8**: Runtime and framework
- **Entity Framework Core**: ORM
- **FastEndpoints**: API framework
- **Ardalis.Specification**: Query pattern
- **FluentValidation**: Request validation
- **AutoMapper**: Object mapping
- **xUnit**: Testing framework
- **FluentAssertions**: Test assertions

## Development

### Coding Patterns

This project follows patterns from the [Ardalis Clean Architecture template](https://github.com/ardalis/CleanArchitecture). Refer to that repository for coding conventions and patterns.

### Key Architectural Decisions

- **Generic Repository Pattern**: Single `IRepository<T>` for all aggregate roots
- **Specification Pattern**: Encapsulates complex queries
- **Query Services**: For non-aggregate entities (providers, users)
- **IAuditableEntity Interface**: Opt-in audit tracking via EF Core interceptor
- **Result Pattern**: For operation outcomes without exceptions

See [docs/ImplementationPlan.md](docs/ImplementationPlan.md) for detailed architectural decisions.

## Core Foundation

The Core layer provides the foundation for the domain model:

### Base Classes

- **`BaseEntity`**: Base class for all domain entities with `Id` property (Guid)
- **`IAuditableEntity`**: Interface for entities requiring audit tracking (CreatedAt, UpdatedAt)
- **`ValueObject`**: Base class for immutable value objects with equality comparison
- **`IAggregateRoot`**: Marker interface for aggregate root entities

### Domain Entities

- **`User`**: Abstract base class for all users with activation/deactivation behavior
  - **`Patient`**: Aggregate root for patients (inherits from User)
  - **`Doctor`**: Domain entity for doctors (inherits from User)
  - **`HealthcareEntity`**: Domain entity for healthcare staff (inherits from User)
  - **`Laboratory`**: Domain entity for lab technicians (inherits from User)
  - **`ImagingCenter`**: Domain entity for imaging technicians (inherits from User)

### Common Abstractions

- **`IRepository<T>`**: Generic repository interface for aggregate roots (uses Specification pattern)
- **Enums**: `UserRole`, `ProviderType`, `EncounterType`, `RecordType`

### Result Pattern

- **`Result<T>`**: Represents operation outcomes (success with value or failure with error)
- **`Error`**: Represents errors with code and message
- **`ResultExtensions`**: Extension methods for Result operations (Map, Bind)

### Architecture Notes

- **Audit Tracking**: Entities opt-in to audit tracking via `IAuditableEntity` interface. Audit properties are set automatically by EF Core interceptor (not all entities are auditable, e.g., `ActionLog`).
- **Repository Pattern**: Only aggregate roots are accessible through repositories. Non-aggregate entities use query services.
- **Result Pattern**: Used for operation outcomes instead of exceptions for expected business errors.
- **User Hierarchy**: Only `Patient` is an aggregate root. Other user types (`Doctor`, `HealthcareEntity`, `Laboratory`, `ImagingCenter`) are domain entities used for reference data.

## Infrastructure Foundation

The Infrastructure layer provides data access and external service implementations:

### Database Setup

1. **Connection String**: Configured in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\MSSQLSERVER03;Database=MedicalCenter;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

2. **Entity Framework Core**:
   - `MedicalCenterDbContext`: Main database context
   - Migrations: Use EF Core tools to create and apply migrations
   - Code-First approach: Database schema is generated from entity configurations

3. **Running Migrations**:
   ```bash
   # Install EF Core tools (if not already installed)
   dotnet tool install --global dotnet-ef
   
   # Apply migrations to database (creates Identity tables and Patient table)
   dotnet ef database update --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
   ```
   
   **Note**: The initial migration `InitialIdentityMigration` has been created and includes:
   - ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.)
   - Patient domain entity table
   - RefreshToken table

### Repository Implementation

- **`Repository<T>`**: Generic repository implementation using `Ardalis.Specification.EntityFrameworkCore`
- Works only with aggregate roots (entities implementing `IAggregateRoot`)
- All queries use the Specification pattern for encapsulation

### Audit Interceptors

- **`AuditableEntityInterceptor`**: Automatically sets `CreatedAt` and `UpdatedAt` for entities implementing `IAuditableEntity`
- Only affects entities that opt-in via the interface
- Non-auditable entities (e.g., `ActionLog`) are not affected

### Dependency Injection

Infrastructure services are registered via `DependencyInjection.AddInfrastructure()` extension method:
- `MedicalCenterDbContext` (scoped)
- `IRepository<T>` (scoped, generic)
- `IIdentityService` (scoped)
- `ITokenProvider` (scoped)
- `AuditableEntityInterceptor` (scoped)

## Identity System (Phase 4)

The system uses ASP.NET Core Identity for authentication and authorization:

### Features

- **User Registration**: Patients can self-register via `POST /patients`
- **Authentication**: JWT-based authentication via `POST /auth/login`
- **Authorization**: Role-based access control (RBAC) with policies
- **Token Management**: Access tokens and refresh tokens

### User Roles

- `SystemAdmin`: System administrators
- `Patient`: Patients receiving care
- `Doctor`: Medical doctors
- `HealthcareStaff`: Hospital/clinic staff
- `LabUser`: Laboratory technicians
- `ImagingUser`: Imaging technicians

### API Endpoints

#### Patient Registration
```http
POST /patients
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "nationalId": "123456789",
  "dateOfBirth": "1990-01-01T00:00:00Z"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe"
}
```

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "Patient"
}
```

### Using the API

1. **Register a Patient**:
   ```bash
   curl -X POST https://localhost:5001/patients \
     -H "Content-Type: application/json" \
     -d '{
       "fullName": "John Doe",
       "email": "john.doe@example.com",
       "password": "SecurePass123!",
       "nationalId": "123456789",
       "dateOfBirth": "1990-01-01T00:00:00Z"
     }'
   ```

2. **Login**:
   ```bash
   curl -X POST https://localhost:5001/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "email": "john.doe@example.com",
       "password": "SecurePass123!"
     }'
   ```

3. **Use Token for Authenticated Requests**:
   ```bash
   curl -X GET https://localhost:5001/api/protected-endpoint \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

### Configuration

JWT settings are configured in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "MedicalCenter",
    "Audience": "MedicalCenter",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

**Important**: Change the `SecretKey` in production to a secure, randomly generated key.

## Testing

### Domain Object Tests

Comprehensive unit tests have been created for all domain entities following the classical school of unit testing:

- **`BaseEntityTests`**: Tests for entity ID generation
- **`ValueObjectTests`**: Tests for value object equality and comparison
- **`UserTests`**: Tests for user activation/deactivation behavior
- **`PatientTests`**: Tests for patient creation and properties
- **`DoctorTests`**: Tests for doctor creation and properties
- **`HealthcareEntityTests`**: Tests for healthcare entity creation and properties
- **`LaboratoryTests`**: Tests for laboratory entity creation and properties
- **`ImagingCenterTests`**: Tests for imaging center entity creation and properties
- **`ResultTests`**: Tests for Result pattern behavior
- **`ErrorTests`**: Tests for Error class behavior

All tests follow the AAA (Arrange, Act, Assert) pattern and focus on testing behavior, not implementation details.

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/MedicalCenter.Core.Tests

# Run tests with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

## Implementation Status

- ✅ Phase 1: Solution Scaffolding & Git Setup
- ✅ Phase 2: Core Foundation & Base Classes
- ✅ Phase 3: Infrastructure Foundation
- ✅ Phase 4: Identity System Foundation (In Progress - Domain Object Testing Complete)
- ⏳ Phase 5: Patient Aggregate & Medical Attributes
- ⏳ Phase 6: Medical Records & Encounters
- ⏳ Phase 7: Query Services & Provider Lookups
- ⏳ Phase 8: Action Logging & Audit Trail
- ⏳ Phase 9: Complete Provider Endpoints
- ⏳ Phase 10: Admin Features
- ⏳ Phase 11: Patient Self-Service Features
- ⏳ Phase 12: Testing & Quality Assurance

**Current Focus**: Phase 4 - Domain object testing and behavior validation. All domain entities (User, Patient, Doctor, HealthcareEntity, Laboratory, ImagingCenter) have comprehensive unit tests covering their core behaviors.

## Documentation

- [Implementation Plan](docs/ImplementationPlan.md) - Detailed implementation guide
- [Domain Model](docs/MedicalCentre.md) - Business domain documentation
- [Medical Attributes](docs/MedicalAttributes_DomainModel.md) - Patient medical attributes model

## License

[Add your license here]

## Contributing

[Add contributing guidelines here]

