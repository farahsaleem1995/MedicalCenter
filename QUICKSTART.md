# Quick Start Guide - Medical Center API

## Prerequisites

- .NET 10 SDK installed
- SQL Server running (local instance: `localhost\MSSQLSERVER03`)
- SQL Server Management Studio or Azure Data Studio (optional, for database inspection)

## Step 1: Apply Database Migrations

The database needs to be created and migrations applied before running the application.

```bash
# Navigate to project root
cd D:\Projects\MedicalCenter

# Apply migrations (creates database and all tables)
dotnet ef database update --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
```

This will:
- Create the `MedicalCenter` database (if it doesn't exist)
- Create all Identity tables (AspNetUsers, AspNetRoles, etc.)
- Create the Patient table
- Create the RefreshToken table

## Step 2: Run the Application

```bash
# Navigate to WebApi project
cd src/MedicalCenter.WebApi

# Run the application
dotnet run
```

Or from the root directory:
```bash
dotnet run --project src/MedicalCenter.WebApi/MedicalCenter.WebApi.csproj
```

The API will start and be available at:
- **HTTPS**: `https://localhost:5001` (or the port shown in console)
- **HTTP**: `http://localhost:5000` (or the port shown in console)

## Step 3: Access OpenAPI/Swagger Documentation

Once the application is running, open your browser and navigate to:

```
https://localhost:5001/openapi/v1.json
```

Or if using Swagger UI (if configured):
```
https://localhost:5001/swagger
```

## Step 4: Test the API

### Register a Patient

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

**Expected Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe"
}
```

### Login

```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```

**Expected Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "Patient"
}
```

### Use Token for Authenticated Requests

```bash
curl -X GET https://localhost:5001/api/protected-endpoint \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Troubleshooting

### Database Connection Issues

If you get connection errors:

1. **Verify SQL Server is running**:
   ```bash
   # Check SQL Server service status
   sc query MSSQLSERVER
   ```

2. **Update connection string** in `src/MedicalCenter.WebApi/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=MedicalCenter;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

3. **Create database manually** (if needed):
   ```sql
   CREATE DATABASE MedicalCenter;
   ```

### Port Already in Use

If port 5001 is already in use, the application will automatically use another port. Check the console output for the actual port number.

### Migration Errors

If migrations fail:

1. **Check database exists**:
   ```sql
   SELECT name FROM sys.databases WHERE name = 'MedicalCenter';
   ```

2. **Drop and recreate** (⚠️ **WARNING**: This deletes all data):
   ```bash
   dotnet ef database drop --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
   dotnet ef database update --project src/MedicalCenter.Infrastructure --startup-project src/MedicalCenter.WebApi
   ```

## Configuration

### JWT Settings

JWT settings are in `src/MedicalCenter.WebApi/appsettings.json`:

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

### Password Requirements

Default password requirements (configured in `DependencyInjection.cs`):
- Minimum 8 characters
- At least one digit
- At least one lowercase letter
- At least one uppercase letter
- At least one non-alphanumeric character

## Development Tips

### View Logs

Logs are output to the console. Check for:
- Database connection messages
- Authentication/authorization errors
- Request/response details (in Development mode)

### Hot Reload

The application supports hot reload during development. Changes to code will automatically restart the application.

### Database Inspection

Use SQL Server Management Studio or Azure Data Studio to inspect:
- `AspNetUsers` - Identity users
- `AspNetRoles` - User roles
- `Patients` - Domain patient entities
- `RefreshTokens` - Refresh token storage

## Next Steps

- Test all authentication endpoints
- Explore OpenAPI documentation
- Review the codebase structure
- Check `README.md` for detailed architecture documentation

