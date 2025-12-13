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
│   ├── MedicalCenter.Core.Tests/        # Domain unit tests
│   ├── MedicalCenter.Infrastructure.Tests/  # Integration tests
│   └── MedicalCenter.WebApi.Tests/      # API integration tests
└── docs/                                 # Documentation
```

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

## Implementation Status

- ✅ Phase 1: Solution Scaffolding & Git Setup
- ✅ Phase 2: Core Foundation & Base Classes
- ⏳ Phase 3: Infrastructure Foundation (Next)
- ⏳ Phase 4: Identity System Foundation
- ⏳ Phase 5: Patient Aggregate & Medical Attributes
- ⏳ Phase 6: Medical Records & Encounters
- ⏳ Phase 7: Query Services & Provider Lookups
- ⏳ Phase 8: Action Logging & Audit Trail
- ⏳ Phase 9: Complete Provider Endpoints
- ⏳ Phase 10: Admin Features
- ⏳ Phase 11: Patient Self-Service Features
- ⏳ Phase 12: Testing & Quality Assurance

## Documentation

- [Implementation Plan](docs/ImplementationPlan.md) - Detailed implementation guide
- [Domain Model](docs/MedicalCentre.md) - Business domain documentation
- [Medical Attributes](docs/MedicalAttributes_DomainModel.md) - Patient medical attributes model

## License

[Add your license here]

## Contributing

[Add contributing guidelines here]

