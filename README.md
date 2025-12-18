# Medical Center Automation System

A comprehensive medical center management system built with .NET 10, following Clean Architecture and Domain-Driven Design principles.

## Overview

The Medical Center Automation System provides a complete solution for managing medical records, patient information, encounters, and provider interactions. It follows a three-layer architecture (Core, Infrastructure, WebApi) and implements DDD patterns for maintainability and scalability.

## Quick Start

### Option 1: Docker (Recommended - No Installation Required)

The easiest way to get started is using Docker. You only need Docker Desktop installed.

#### Prerequisites

- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** - For version control

#### Step 1: Clone the Repository

```bash
git clone <repository-url>
cd MedicalCenter
```

#### Step 2: Configure Environment (Optional)

Copy the example environment file and customize if needed:

```bash
# Windows (PowerShell)
Copy-Item env.example .env

# Linux/Mac
cp env.example .env
```

Edit `.env` file to customize:
- SQL Server password (`SA_PASSWORD`)
- API ports (`API_HTTP_PORT`, `API_HTTPS_PORT`)
- JWT secret key (`JWT_SECRET_KEY`)

#### Step 3: Start with Docker Compose

```bash
# First time or after code changes (rebuilds and recreates containers)
docker-compose up --build

# Or if you want to force recreate containers even if image hasn't changed
docker-compose up --build --force-recreate
```

This will:
- Build the .NET application Docker image
- Recreate containers to ensure latest code and migrations are applied
- Start SQL Server container
- Start the Web API container (waits for SQL Server to be healthy)
- Automatically run database migrations on startup
- Seed initial data (roles, system admin)

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **Swagger**: `http://localhost:5000/swagger`

**Note**: The database and all tables are created automatically on first startup. Migrations run automatically when the container starts.

#### Step 4: Access the Application

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

#### Default System Admin Credentials

A system administrator account is automatically seeded when the database is created. You can use these credentials to log in:

- **Email**: `sys.admin@medicalcenter.com`
- **Password**: `Admin@123!ChangeMe`

**⚠️ Important**: Change the default password in production environments.

#### Stop the Application

```bash
# Stop containers (keeps data)
docker-compose stop

# Stop and remove containers (keeps data)
docker-compose down

# Stop and remove containers and volumes (⚠️ deletes data)
docker-compose down -v
```

#### View Logs

```bash
# View all logs
docker-compose logs

# Follow logs
docker-compose logs -f

# View specific service logs
docker-compose logs webapi
docker-compose logs sqlserver
```

---

### Option 2: Manual Setup (Requires .NET SDK and SQL Server)

### Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download)
- **SQL Server** - Local instance or SQL Server Express
- **Git** - For version control

### Step 1: Clone and Build

```bash
# Clone the repository
git clone <repository-url>
cd MedicalCenter

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Step 2: Configure Database

Update the connection string in `src/MedicalCenter.WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MedicalCenter;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Step 3: Apply Database Migrations

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply migrations (creates database and all tables)
dotnet ef database update --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
```

This creates:
- `MedicalCenter` database
- ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.)
- Domain entity tables (Patients, Doctors, etc.)
- RefreshToken table

### Step 4: Run the Application

```bash
# From project root
dotnet run --project src/MedicalCenter.WebApi/MedicalCenter.WebApi.csproj
```

The API will be available at:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

### Step 5: Access Swagger Documentation

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

### Default System Admin Credentials

A system administrator account is automatically seeded when the database is created. You can use these credentials to log in:

- **Email**: `sys.admin@medicalcenter.com`
- **Password**: `Admin@123!ChangeMe`

**⚠️ Important**: Change the default password in production environments.

### Step 6: Test the API

#### Register a Patient

```bash
curl -X POST https://localhost:5001/auth/patients \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "password": "SecurePass123!",
    "nationalId": "123456789",
    "dateOfBirth": "1990-01-01T00:00:00Z"
  }'
```

#### Login

```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```

#### Use Token for Authenticated Requests

```bash
curl -X GET https://localhost:5001/patients/self \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Project Structure

```
MedicalCenter/
├── src/
│   ├── MedicalCenter.Core/              # Domain layer (entities, aggregates, value objects)
│   ├── MedicalCenter.Infrastructure/    # Data access & external services
│   └── MedicalCenter.WebApi/            # API endpoints & presentation
├── tests/
│   └── MedicalCenter.Core.Tests/        # Domain unit tests
└── docs/                                 # Documentation
    ├── Architecture.md                  # Architecture documentation
    ├── Features.md                      # Features documentation
    └── ImplementationPlan.md          # Implementation roadmap
```

## Technology Stack

- **.NET 10**: Runtime and framework
- **Entity Framework Core 10**: ORM
- **FastEndpoints**: API framework
- **FastEndpoints.Swagger**: OpenAPI documentation (NSwag)
- **ASP.NET Core Identity**: Authentication/Authorization
- **FluentValidation**: Request validation
- **Ardalis.Specification**: Query pattern
- **xUnit**: Testing framework
- **FluentAssertions**: Test assertions

## Key Features

### Authentication & Authorization

- ✅ Patient self-registration
- ✅ JWT-based authentication
- ✅ Refresh token mechanism
- ✅ Role-based access control (RBAC)
- ✅ Policy-based authorization

### Patient Management

- ✅ Patient aggregate with medical attributes
- ✅ Blood type management (create/update)
- ✅ Allergies, Chronic Diseases, Medications, Surgeries
- ✅ Patient self-service endpoints

### Medical Attributes Management

- ✅ CRUD operations for all medical attributes
- ✅ Provider-based authorization
- ✅ Comprehensive validation

### Medical Records

- ✅ Medical record creation and management
- ✅ File attachment support (upload, download, add/remove)
- ✅ Local filesystem file storage (configurable)
- ✅ Multiple attachments per record (up to 10, configurable)
- ✅ Practitioner and patient views
- ✅ Practitioner-based authorization

### Admin Features

- ✅ User management (CRUD)
- ✅ Pagination support
- ✅ User filtering and search
- ✅ Password management

### API Features

- ✅ RESTful API design
- ✅ Swagger/OpenAPI documentation
- ✅ Pagination for list endpoints
- ✅ Comprehensive validation
- ✅ Error handling (Problem Details format)
- ✅ File upload/download support

## Documentation

- **[Architecture](docs/Architecture.md)** - Comprehensive architecture documentation
- **[Features](docs/Features.md)** - Detailed features documentation with API endpoint details and enum value mappings
- **[Implementation Plan](docs/ImplementationPlan.md)** - Implementation roadmap and progress

## Testing

### Run Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/MedicalCenter.Core.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

- **210 domain unit tests** passing
- Tests follow classical school approach (behavior-focused)
- AAA pattern (Arrange, Act, Assert)

## Configuration

### JWT Settings

Configure in `src/MedicalCenter.WebApi/appsettings.json`:

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

**⚠️ Important**: Change the `SecretKey` in production to a secure, randomly generated key.

### File Storage Settings

Configure in `src/MedicalCenter.WebApi/appsettings.json`:

```json
{
  "FileStorage": {
    "Path": "./attachments",
    "MaxFileSizeBytes": 10485760,
    "MaxAttachmentsPerRecord": 10,
    "AllowedContentTypes": [
      "application/pdf",
      "image/jpeg",
      "image/png",
      "image/jpg",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      "application/msword",
      "application/vnd.ms-excel"
    ]
  }
}
```

**Note**: The `Path` is relative to the application's working directory. In Docker, this will be inside the container. For production, consider using absolute paths or cloud storage.

### Password Requirements

- Minimum 8 characters
- At least one digit
- At least one lowercase letter
- At least one uppercase letter
- At least one non-alphanumeric character

## Development

### Coding Patterns

This project follows patterns from the [Ardalis Clean Architecture template](https://github.com/ardalis/CleanArchitecture).

### Key Architectural Decisions

- **Generic Repository Pattern**: Single `IRepository<T>` for aggregate roots (in SharedKernel/)
- **Specification Pattern**: Encapsulates complex queries
- **Query Services**: For optimized read operations (practitioners, users) - interfaces in Queries/
- **Domain Organization**: Organized following DDD principles
  - DDD building blocks (BaseEntity, ValueObject, IAggregateRoot) in `Abstractions/`
  - Cross-cutting technical concerns (Result pattern, Pagination) in `Primitives/`
  - Shared domain concepts (User, Repository, Domain Events) in `SharedKernel/`
  - Aggregate-specific types (enums, value objects, entities, specifications) within their aggregates
- **IAuditableEntity Interface**: Opt-in audit tracking via EF Core interceptor
- **Result Pattern**: For operation outcomes without exceptions (in Primitives/)
- **Pagination Pattern**: Standardized paginated responses (in Primitives/Pagination/)
- **Time Handling Pattern**: `IDateTimeProvider` for unified time access and testability (in Services/)

## Implementation Status

- ✅ **Phase 1**: Solution Scaffolding & Git Setup
- ✅ **Phase 2**: Core Foundation & Base Classes
- ✅ **Phase 2.1**: Core Layer Reorganization (DDD-aligned structure: Abstractions/, Primitives/, SharedKernel/, Queries/, organized Aggregates/)
- ✅ **Phase 3**: Infrastructure Foundation
- ✅ **Phase 4**: Identity System Foundation
- ✅ **Phase 5**: Patient Aggregate & Medical Attributes
- 🔄 **Phase 6**: Medical Records (Medical Records complete, Encounters postponed - requires domain events)
- 🔄 **Phase 7**: Query Services & Provider Lookups (Partially Complete - UserQueryService implemented)
- 🔄 **Phase 10**: Admin Features (Partially Complete - User management endpoints implemented)
- ⏳ **Phase 8**: Action Logging & Audit Trail
- ⏳ **Phase 9**: Complete Provider Endpoints
- ⏳ **Phase 11**: Patient Self-Service Features
- ⏳ **Phase 12**: Testing & Quality Assurance
- ✅ **Phase 13**: Dockerization

See [ImplementationPlan.md](docs/ImplementationPlan.md) for detailed progress.

## Troubleshooting

### Docker Issues

#### Containers Won't Start

1. **Check Docker is running**:
   ```bash
   docker ps
   ```

2. **View container logs**:
   ```bash
   docker-compose logs
   ```

3. **Rebuild containers**:
   ```bash
   docker-compose up --build
   ```

#### Database Connection Issues in Docker

1. **Check SQL Server container is healthy**:
   ```bash
   docker-compose ps
   ```

2. **Verify connection string** uses service name `sqlserver` (not `localhost`)

3. **Check SQL Server logs**:
   ```bash
   docker-compose logs sqlserver
   ```

4. **Restart services**:
   ```bash
   docker-compose restart
   ```

#### Migration Errors in Docker

1. **Check application logs** for migration errors:
   ```bash
   docker-compose logs webapi
   ```

2. **Manually run migrations** (if needed):
   ```bash
   docker-compose exec webapi dotnet ef database update --project /src/src/MedicalCenter.Infrastructure --startup-project /src/src/MedicalCenter.WebApi
   ```

#### Port Conflicts

If ports 5000, 5001, or 1433 are already in use:

1. **Update `.env` file** with different ports:
   ```env
   API_HTTP_PORT=5002
   API_HTTPS_PORT=5003
   SQL_SERVER_PORT=1434
   ```

2. **Restart containers**:
   ```bash
   docker-compose down
   docker-compose up
   ```

#### Clean Up Docker Resources

```bash
# Stop and remove containers (keeps volumes)
docker-compose down

# Stop and remove containers and volumes (⚠️ deletes all data)
docker-compose down -v

# Remove all Docker resources (⚠️ removes all containers, images, volumes)
docker system prune -a --volumes
```

### Manual Setup Issues

#### Database Connection Issues

1. **Verify SQL Server is running**
2. **Check connection string** in `appsettings.json`
3. **Create database manually** if needed:
   ```sql
   CREATE DATABASE MedicalCenter;
   ```

#### Port Already in Use

The application will automatically use another port. Check the console output for the actual port number.

#### Migration Errors

1. **Check database exists**
2. **Drop and recreate** (⚠️ **WARNING**: This deletes all data):
   ```bash
   dotnet ef database drop --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
   dotnet ef database update --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
   ```

## License

[Add your license here]

## Contributing

[Add contributing guidelines here]
