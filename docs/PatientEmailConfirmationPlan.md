# Patient Email Confirmation Implementation Plan

**Status**: Temporary plan - to be removed after implementation  
**Date**: 2025-12-16  
**Last Updated**: 2025-12-18 (Updated for Core layer reorganization and MailDev development setup)  
**Objective**: Introduce email confirmation for patients only via SMTP. Unconfirmed patients cannot login. Confirmation email is sent on-demand via endpoint, not automatically upon registration.

## Development Environment: MailDev

For local development, use [MailDev](https://hub.docker.com/r/maildev/maildev) - a simple SMTP server that captures all emails and provides a web UI to view them.

### Option 1: Standalone Script (Recommended for local development)
```bash
./scripts/start-maildev.sh          # Start MailDev
./scripts/start-maildev.sh --stop   # Stop MailDev
./scripts/start-maildev.sh --logs   # View logs
```

### Option 2: Docker Compose (When running full stack)

If running the entire application through Docker Compose, MailDev is included automatically:

```bash
docker-compose up -d                # Starts all services including MailDev
docker-compose logs maildev         # View MailDev logs
```

When using Docker Compose, the SMTP host should be `maildev` (the container name) instead of `localhost`.

**Access Points:**
| Service | Standalone | Docker Compose |
|---------|------------|----------------|
| SMTP Server | `localhost:1025` | `maildev:1025` (internal) |
| Web UI | `http://localhost:1080` | `http://localhost:1080` |

## Important: Namespace Updates

This plan has been updated to reflect the Core layer reorganization completed on 2025-12-18:

- **`MedicalCenter.Core.Common`** → Split into:
  - `MedicalCenter.Core.Abstractions` (BaseEntity, IAggregateRoot, IAuditableEntity, ValueObject)
  - `MedicalCenter.Core.Primitives` (Result, Error, ErrorCodes, Pagination)
  - `MedicalCenter.Core.SharedKernel` (User, UserRole, IRepository, IUnitOfWork, Attachment)
- **Aggregate namespaces**:
  - `MedicalCenter.Core.Aggregates.Patients` (Patient - renamed from `Patient` singular)
  - `MedicalCenter.Core.Aggregates.Patients.Specifications` (Patient specifications)
  - `MedicalCenter.Core.Aggregates.Patients.Entities` (Allergy, ChronicDisease, Medication, Surgery)
  - `MedicalCenter.Core.Aggregates.Patients.ValueObjects` (BloodType)
  - `MedicalCenter.Core.Aggregates.Patients.Enums` (BloodABO, BloodRh)

All code examples in this plan use the updated namespaces.

---

## Current State

### Current Implementation
- Patients can register and immediately login
- No email confirmation required
- No SMTP infrastructure
- All user types (Patient, Doctor, etc.) have same login flow
- `ApplicationUser` (Identity) has `EmailConfirmed` property (ASP.NET Core Identity built-in, currently set to `true`)

### Requirements
1. **Patients Only**: Email confirmation applies only to Patient role
2. **SMTP Integration**: Use SMTP for sending confirmation emails
3. **Abstraction in Core**: `ISmtpClient` interface in Core layer
4. **Options Pattern**: SMTP configuration via ASP.NET Core Options pattern
5. **On-Demand Confirmation**: Email is NOT sent automatically on registration
6. **Client-Initiated**: Client must request confirmation email via dedicated endpoint
7. **Login Restriction**: Unconfirmed patients cannot login

---

## Target State

### Domain Model
- **No domain model changes**: Email confirmation is an Identity concern, not a domain concern
- Patient aggregate remains unchanged
- Email confirmation handled at Identity layer (ApplicationUser)

### Infrastructure
- `ISmtpClient` interface in Core layer (abstraction)
- `SmtpClient` implementation in Infrastructure layer
- `SmtpOptions` class for configuration (Options pattern)
- SMTP configuration in `appsettings.json`
- `ApplicationUser` (Identity) has `EmailConfirmed` property (ASP.NET Core Identity built-in)
- Email confirmation token stored in separate table or ApplicationUser properties

### API Endpoints
- `POST /auth/patients/request-confirmation` - Request confirmation email (sends email with token)
- `POST /auth/patients/confirm-email` - Confirm email using token
- Login endpoint updated to check confirmation status for patients (via Identity service)

### Business Rules
- **Patients**: Must have confirmed email to login (Identity concern)
- **Other Users**: No confirmation required (Doctor, HealthcareStaff, etc.)
- **Identity Service**: Has flag to indicate if confirmation is required for a role
- **Identity Service**: Has method to check if user is unconfirmed
- **Token Expiration**: Confirmation tokens expire after configured time (default: 24 hours)
- **Token Single Use**: Token can only be used once
- **On-Demand**: Confirmation email only sent when client requests it

---

## Implementation Steps

### Step 1: Add ISmtpClient Interface to Core Layer

**File**: `src/MedicalCenter.Core/Services/ISmtpClient.cs`

**Implementation**:
```csharp
namespace MedicalCenter.Core.Services;

/// <summary>
/// Abstraction for sending emails via SMTP.
/// </summary>
public interface ISmtpClient
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML or plain text)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
```

**Required Using Statement**:
```csharp
using MedicalCenter.Core.Primitives; // For Result
```

**Notes**:
- Returns `Result` (not `Result<T>`) since we only need success/failure
- Simple interface - can be extended later if needed
- No dependency on infrastructure concerns
- `Result` is now in `MedicalCenter.Core.Primitives` namespace

---

### Step 2: Create SmtpOptions Configuration Class

**File**: `src/MedicalCenter.Infrastructure/Options/SmtpOptions.cs`

**Implementation**:
```csharp
namespace MedicalCenter.Infrastructure.Options;

/// <summary>
/// SMTP configuration options.
/// Supports both production SMTP servers and MailDev for development.
/// </summary>
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// SMTP server host address.
    /// For MailDev: "localhost"
    /// For production: e.g., "smtp.gmail.com"
    /// </summary>
    public string Host { get; set; } = string.Empty;
    
    /// <summary>
    /// SMTP server port.
    /// For MailDev: 1025
    /// For production: typically 587 (TLS) or 465 (SSL)
    /// </summary>
    public int Port { get; set; } = 587;
    
    /// <summary>
    /// Enable SSL/TLS encryption.
    /// For MailDev: false
    /// For production: true
    /// </summary>
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>
    /// SMTP username for authentication.
    /// For MailDev: not required (leave empty)
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// SMTP password for authentication.
    /// For MailDev: not required (leave empty)
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    
    /// <summary>
    /// When true, uses MailDev mode (no authentication required).
    /// Set to true in Development environment.
    /// </summary>
    public bool UseMailDev { get; set; } = false;
    
    /// <summary>
    /// MailDev Web UI port for viewing captured emails.
    /// Default: 1080
    /// </summary>
    public int MailDevWebPort { get; set; } = 1080;
}
```

**Configuration in appsettings.json** (Production):
```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "your-username",
    "Password": "your-password",
    "FromEmail": "noreply@medicalcenter.com",
    "FromName": "Medical Center",
    "UseMailDev": false
  }
}
```

**Configuration in appsettings.Development.json** (MailDev - Standalone):
```json
{
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "EnableSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@medicalcenter.local",
    "FromName": "Medical Center (Dev)",
    "UseMailDev": true,
    "MailDevWebPort": 1080
  }
}
```

**Note for Docker Compose**: When running via Docker Compose, the SMTP host is automatically configured via environment variables to use `maildev` instead of `localhost`. See the docker-compose.yml configuration below.

---

### Step 3: Implement SmtpClient in Infrastructure Layer

**File**: `src/MedicalCenter.Infrastructure/Services/SmtpClient.cs`

**Implementation**:
```csharp
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// SMTP client implementation for sending emails.
/// Supports both production SMTP servers and MailDev for development.
/// </summary>
public class SmtpClient : ISmtpClient
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpClient> _logger;

    public SmtpClient(IOptions<SmtpOptions> options, ILogger<SmtpClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new System.Net.Mail.SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                // Skip credentials for MailDev (development mode - no authentication needed)
                Credentials = _options.UseMailDev 
                    ? null 
                    : new NetworkCredential(_options.Username, _options.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                To = { new MailAddress(to) },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, cancellationToken);
            
            // Log success with MailDev web UI link in development
            if (_options.UseMailDev)
            {
                _logger.LogInformation(
                    "Email sent via MailDev. View at: http://localhost:{Port}",
                    _options.MailDevWebPort);
            }
            else
            {
                _logger.LogInformation("Email sent to {To}", to);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return Result.Failure(Error.Failure($"Failed to send email: {ex.Message}"));
        }
    }
}
```

**Required Using Statements**:
```csharp
using MedicalCenter.Core.Primitives; // For Result, Error
using MedicalCenter.Core.Services; // For ISmtpClient
using Microsoft.Extensions.Logging; // For ILogger
```

**Notes**:
- Uses `System.Net.Mail.SmtpClient` (or consider MailKit for better async support)
- Handles exceptions and returns Result
- Configured via Options pattern
- **MailDev Support**: When `UseMailDev = true`, credentials are skipped
- Logs MailDev web UI link in development for easy access
- `Result` and `Error` are now in `MedicalCenter.Core.Primitives` namespace

---

### Step 4: Update IIdentityService Interface

**File**: `src/MedicalCenter.Core/Services/IIdentityService.cs`

**Changes**:
1. Update `CreateUserAsync` to accept `requireEmailConfirmation` parameter
2. Add `IsUserUnconfirmedAsync` method to check if user is unconfirmed

**Update CreateUserAsync**:
```csharp
/// <summary>
/// Creates a new Identity user (ApplicationUser) with the specified email and password.
/// </summary>
/// <param name="email">User email address</param>
/// <param name="password">User password</param>
/// <param name="role">User role</param>
/// <param name="requireEmailConfirmation">If true, user starts with EmailConfirmed = false (for Patient role)</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>User ID if successful</returns>
Task<Result<Guid>> CreateUserAsync(
    string email,
    string password,
    UserRole role,
    bool requireEmailConfirmation = false,
    CancellationToken cancellationToken = default);
```

**Add New Method**:
```csharp
/// <summary>
/// Checks if a user's email is unconfirmed.
/// </summary>
/// <param name="userId">User ID</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>True if user exists and email is unconfirmed, false otherwise</returns>
Task<bool> IsUserUnconfirmedAsync(Guid userId, CancellationToken cancellationToken = default);
```

**Required Using Statement**:
```csharp
using MedicalCenter.Core.SharedKernel; // For UserRole
using MedicalCenter.Core.Primitives; // For Result<T>
```

**Notes**:
- Email confirmation is an Identity concern, not a domain concern
- `requireEmailConfirmation` flag allows service to set `EmailConfirmed = false` for Patient role
- `IsUserUnconfirmedAsync` checks Identity user's `EmailConfirmed` property
- `UserRole` is now in `MedicalCenter.Core.SharedKernel` namespace
- `Result<T>` is now in `MedicalCenter.Core.Primitives` namespace

---

### Step 5: Update IdentityService Implementation

**File**: `src/MedicalCenter.Infrastructure/Services/IdentityService.cs`

**Changes**:
1. Update `CreateUserAsync` to respect `requireEmailConfirmation` parameter
2. Implement `IsUserUnconfirmedAsync` method
3. Add methods for token generation and email confirmation (Identity concerns)

**Update CreateUserAsync**:
```csharp
public async Task<Result<Guid>> CreateUserAsync(
    string email,
    string password,
    UserRole role,
    bool requireEmailConfirmation = false,
    CancellationToken cancellationToken = default)
{
    // Check if user already exists
    var existingUser = await userManager.FindByEmailAsync(email);
    if (existingUser != null)
    {
        return Result<Guid>.Failure(Error.Conflict("A user with this email already exists."));
    }
    
    // Note: Error and Result are from MedicalCenter.Core.Primitives namespace

    // Create Identity user
    var userId = Guid.NewGuid();
    var identityUser = new ApplicationUser
    {
        Id = userId,
        UserName = email,
        Email = email,
        EmailConfirmed = !requireEmailConfirmation // Set based on flag
    };

    var identityResult = await userManager.CreateAsync(identityUser, password);
    if (!identityResult.Succeeded)
    {
        var errors = identityResult.Errors.Select(e => e.Description);
        return Result<Guid>.Failure(Error.Validation(string.Join("; ", errors)));
    }

    // Assign role
    var roleName = role.ToString();
    var roleResult = await userManager.AddToRoleAsync(identityUser, roleName);
    if (!roleResult.Succeeded)
    {
        // Rollback: delete user if role assignment fails
        await userManager.DeleteAsync(identityUser);
        var errors = roleResult.Errors.Select(e => e.Description);
        return Result<Guid>.Failure(Error.Validation(string.Join("; ", errors)));
    }

    return Result<Guid>.Success(userId);
}
```

**Add IsUserUnconfirmedAsync**:
```csharp
public async Task<bool> IsUserUnconfirmedAsync(Guid userId, CancellationToken cancellationToken = default)
{
    var identityUser = await userManager.FindByIdAsync(userId.ToString());
    if (identityUser == null)
    {
        return false; // User doesn't exist
    }

    return !identityUser.EmailConfirmed;
}
```

**Add Token Management Methods** (Identity concerns):
```csharp
/// <summary>
/// Generates an email confirmation token for a user.
/// </summary>
public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
{
    var identityUser = await userManager.FindByIdAsync(userId.ToString());
    if (identityUser == null)
    {
        return Result<string>.Failure(Error.NotFound("User"));
    }

    if (identityUser.EmailConfirmed)
    {
        return Result<string>.Failure(Error.Validation("Email is already confirmed."));
    }

    // Use ASP.NET Core Identity's token provider
    var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);
    return Result<string>.Success(token);
}

/// <summary>
/// Confirms a user's email using the provided token.
/// </summary>
public async Task<Result> ConfirmEmailAsync(
    Guid userId,
    string token,
    CancellationToken cancellationToken = default)
{
    var identityUser = await userManager.FindByIdAsync(userId.ToString());
    if (identityUser == null)
    {
        return Result.Failure(Error.NotFound("User"));
    }

    if (identityUser.EmailConfirmed)
    {
        return Result.Failure(Error.Validation("Email is already confirmed."));
    }

    var result = await userManager.ConfirmEmailAsync(identityUser, token);
    if (!result.Succeeded)
    {
        var errors = result.Errors.Select(e => e.Description);
        return Result.Failure(Error.Validation(string.Join("; ", errors)));
    }

    return Result.Success();
}
```

**Required Using Statements**:
```csharp
using MedicalCenter.Core.Primitives; // For Result, Result<T>, Error
```

**Notes**:
- Uses ASP.NET Core Identity's built-in `EmailConfirmed` property
- Uses Identity's token generation and confirmation methods
- Token management is handled by Identity framework
- `Result`, `Result<T>`, and `Error` are now in `MedicalCenter.Core.Primitives` namespace

---

### Step 6: Create Request Confirmation Email Endpoint

**File**: `src/MedicalCenter.WebApi/Endpoints/Auth/RequestEmailConfirmationEndpoint.cs`

**Implementation**:
```csharp
using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Request email confirmation endpoint for patients.
/// </summary>
public class RequestEmailConfirmationEndpoint(
    IRepository<Patient> patientRepository,
    IIdentityService identityService,
    ISmtpClient smtpClient,
    IUnitOfWork unitOfWork)
    : Endpoint<RequestEmailConfirmationRequest>
{
    public override void Configure()
    {
        Post("/auth/patients/request-confirmation");
        Group<AuthGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Request email confirmation";
            s.Description = "Sends a confirmation email to the patient. The patient must use the token from the email to confirm their account.";
            s.Responses[200] = "Confirmation email sent successfully";
            s.Responses[400] = "Validation error";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(RequestEmailConfirmationRequest req, CancellationToken ct)
    {
        // Find patient by email
        var specification = new PatientByEmailSpecification(req.Email);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        // Check if already confirmed (via Identity service)
        var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(patient.Id, ct);
        if (!isUnconfirmed)
        {
            ThrowError("Email is already confirmed", 400);
            return;
        }

        // Generate confirmation token (via Identity service)
        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(patient.Id, ct);
        if (tokenResult.IsFailure)
        {
            ThrowError(tokenResult.Error!.Message, 400);
            return;
        }

        var token = tokenResult.Value!;

        // Send confirmation email
        var confirmationUrl = $"{GetBaseUrl()}/auth/patients/confirm-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(patient.Email)}";
        var emailBody = GenerateConfirmationEmailBody(patient.FullName, confirmationUrl);

        var sendResult = await smtpClient.SendEmailAsync(
            patient.Email,
            "Confirm Your Email - Medical Center",
            emailBody,
            ct);

        if (sendResult.IsFailure)
        {
            // Log error but don't fail the request - token is already generated
            // Client can request again if email fails
            // Consider logging the error
        }

        await Send.OkAsync(ct);
    }

    private string GetBaseUrl()
    {
        // Get base URL from configuration or request
        // This should come from appsettings or HttpContext
        return "https://api.medicalcenter.com"; // Placeholder
    }

    private string GenerateConfirmationEmailBody(string fullName, string confirmationUrl)
    {
        return $@"
            <html>
            <body>
                <h2>Email Confirmation</h2>
                <p>Hello {fullName},</p>
                <p>Please click the link below to confirm your email address:</p>
                <p><a href=""{confirmationUrl}"">{confirmationUrl}</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not request this confirmation, please ignore this email.</p>
            </body>
            </html>";
    }
}
```

**Required Using Statements**:
```csharp
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.SharedKernel; // For IRepository<T>, IUnitOfWork
```

**Notes**:
- Uses `IIdentityService` to check confirmation status and generate tokens
- No domain entity changes needed
- Token generation handled by Identity framework
- `IRepository<T>` and `IUnitOfWork` are now in `MedicalCenter.Core.SharedKernel` namespace
- Patient aggregate is now in `MedicalCenter.Core.Aggregates.Patients` namespace

**Request DTO**:
```csharp
namespace MedicalCenter.WebApi.Endpoints.Auth;

public class RequestEmailConfirmationRequest
{
    public string Email { get; set; } = string.Empty;
}
```

**Validator**:
```csharp
public class RequestEmailConfirmationRequestValidator : Validator<RequestEmailConfirmationRequest>
{
    public RequestEmailConfirmationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
```

**Notes**:
- Endpoint is anonymous (patient doesn't need to be logged in)
- Generates token and sends email
- Token expires in 24 hours (configurable)
- Email sending failure doesn't fail the request (token is generated, can retry)

---

### Step 7: Create Confirm Email Endpoint

**File**: `src/MedicalCenter.WebApi/Endpoints/Auth/ConfirmEmailEndpoint.cs`

**Implementation**:
```csharp
using FastEndpoints;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure.Authorization;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Confirm email endpoint for patients.
/// </summary>
public class ConfirmEmailEndpoint(
    IRepository<Patient> patientRepository,
    IIdentityService identityService)
    : Endpoint<ConfirmEmailRequest>
{
    public override void Configure()
    {
        Post("/auth/patients/confirm-email");
        Group<AuthGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Confirm email address";
            s.Description = "Confirms a patient's email address using the token sent via email.";
            s.Responses[200] = "Email confirmed successfully";
            s.Responses[400] = "Invalid or expired token";
            s.Responses[404] = "Patient not found";
        });
    }

    public override async Task HandleAsync(ConfirmEmailRequest req, CancellationToken ct)
    {
        // Find patient by email
        var specification = new PatientByEmailSpecification(req.Email);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);

        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        // Check if already confirmed (via Identity service)
        var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(patient.Id, ct);
        if (!isUnconfirmed)
        {
            ThrowError("Email is already confirmed", 400);
            return;
        }

        // Confirm email (via Identity service)
        var confirmResult = await identityService.ConfirmEmailAsync(patient.Id, req.Token, ct);
        if (confirmResult.IsFailure)
        {
            int statusCode = confirmResult.Error!.Code.ToStatusCode();
            ThrowError(confirmResult.Error.Message, statusCode);
            return;
        }

        await Send.OkAsync(ct);
    }
}
```

**Notes**:
- Uses `IIdentityService.ConfirmEmailAsync` to confirm email
- No domain entity changes needed
- No transaction needed (Identity handles it)

**Request DTO**:
```csharp
namespace MedicalCenter.WebApi.Endpoints.Auth;

public class ConfirmEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
```

**Validator**:
```csharp
public class ConfirmEmailRequestValidator : Validator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

---

### Step 8: Update RegisterPatientEndpoint

**File**: `src/MedicalCenter.WebApi/Endpoints/Auth/RegisterPatientEndpoint.cs`

**Changes**:
- Update `CreateUserAsync` call to pass `requireEmailConfirmation: true` for Patient role
- Ensure `using MedicalCenter.Core.SharedKernel;` is present for `UserRole`

**Update** (line 44-48):
```csharp
using MedicalCenter.Core.SharedKernel;

// Step 1: Create Identity user (generic user creation)
var createUserResult = await identityService.CreateUserAsync(
    req.Email,
    req.Password,
    UserRole.Patient,
    requireEmailConfirmation: true, // Patients require email confirmation
    ct);
```

**Notes**:
- Patients start with `EmailConfirmed = false` in Identity
- No domain entity changes needed
- `UserRole` is from `MedicalCenter.Core.SharedKernel` namespace

---

### Step 9: Create PatientByEmailSpecification

**File**: `src/MedicalCenter.Core/Aggregates/Patients/Specifications/PatientByEmailSpecification.cs`

**Implementation**:
```csharp
using Ardalis.Specification;

namespace MedicalCenter.Core.Aggregates.Patients.Specifications;

/// <summary>
/// Specification to get a patient by email address.
/// </summary>
public class PatientByEmailSpecification : Specification<Patient>
{
    public PatientByEmailSpecification(string email)
    {
        Query.Where(p => p.Email == email);
    }
}
```

---

### Step 10: Update Login Endpoint

**File**: `src/MedicalCenter.WebApi/Endpoints/Auth/LoginEndpoint.cs`

**Changes**:
- After successful password validation, check if user is Patient
- If Patient, use `IIdentityService.IsUserUnconfirmedAsync` to check confirmation status
- If not confirmed, return 403 Forbidden with appropriate message

**Add Check** (after password validation, around line 43):
```csharp
using MedicalCenter.Core.SharedKernel;

// Check email confirmation for patients (Identity concern)
if (user.Role == UserRole.Patient)
{
    var isUnconfirmed = await identityService.IsUserUnconfirmedAsync(user.Id, ct);
    if (isUnconfirmed)
    {
        ThrowError("Email address must be confirmed before logging in. Please check your email for the confirmation link.", 403);
        return;
    }
}
```

**Add Dependency**:
- Add `IIdentityService` to constructor dependencies

**Notes**:
- Only applies to Patient role
- Other user types (Doctor, HealthcareStaff, etc.) can login without confirmation
- Returns 403 Forbidden (not 401 Unauthorized) since credentials are valid but account is not activated
- Uses Identity service to check confirmation status (Identity concern)
- `UserRole` is from `MedicalCenter.Core.SharedKernel` namespace

---

### Step 11: Register Services in DependencyInjection

**File**: `src/MedicalCenter.Infrastructure/DependencyInjection.cs`

**Add**:
```csharp
// Register SMTP client
services.AddScoped<ISmtpClient, SmtpClient>();

// Configure SMTP options
services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
```

---

### Step 12: Verify ApplicationUser EmailConfirmed Property

**File**: `src/MedicalCenter.Infrastructure/Identity/ApplicationUser.cs`

**Verify**:
- `ApplicationUser` inherits from `IdentityUser<Guid>` which has `EmailConfirmed` property
- No changes needed - ASP.NET Core Identity provides this property

**Notes**:
- `EmailConfirmed` is a built-in property of `IdentityUser`
- Token generation and confirmation handled by Identity framework
- No EF Core configuration changes needed for Identity tables

---

### Step 13: Verify No Database Migration Needed

**Notes**:
- **No migration needed**: `EmailConfirmed` is already part of `AspNetUsers` table (ASP.NET Core Identity)
- Identity framework handles token storage internally
- No domain entity changes, so no Patient table changes

**Verify**:
- Check that `AspNetUsers` table has `EmailConfirmed` column (should already exist)
- Identity token providers handle token generation and validation

---

### Step 14: Update Tests

**Files to Update**:
- No domain tests needed - Patient aggregate unchanged
- Email confirmation is Identity concern, not domain concern

**Note**: Tests are only for the Core (domain) layer. Since email confirmation is an Identity concern and Patient aggregate is unchanged, no domain tests are needed. API endpoint behavior is verified through manual testing.

---

### Step 15: Update Documentation

**Files to Update**:
- `docs/Features.md`: Add email confirmation endpoints
- `docs/Architecture.md`: Add SMTP service to infrastructure
- `README.md`: Update if authentication flow is documented

**Features.md Changes**:
- Add "Email Confirmation" section under Authentication
- Document `POST /auth/patients/request-confirmation` endpoint
- Document `POST /auth/patients/confirm-email` endpoint
- Document login restriction for unconfirmed patients

---

### Step 16: Verification Checklist

**Core Layer:**
- [ ] `ISmtpClient` interface created in Core layer

**Infrastructure Layer:**
- [ ] `SmtpOptions` class created with Options pattern (including MailDev support)
- [ ] `SmtpClient` implementation created (with MailDev mode)
- [ ] SMTP configuration added to `appsettings.json` (production)
- [ ] SMTP configuration added to `appsettings.Development.json` (MailDev)
- [ ] `IIdentityService.CreateUserAsync` updated with `requireEmailConfirmation` parameter
- [ ] `IIdentityService.IsUserUnconfirmedAsync` method added
- [ ] `IIdentityService.GenerateEmailConfirmationTokenAsync` method added
- [ ] `IIdentityService.ConfirmEmailAsync` method added
- [ ] `IdentityService` implementation updated
- [ ] Services registered in DependencyInjection

**Endpoints:**
- [ ] `RegisterPatientEndpoint` updated to pass `requireEmailConfirmation: true`
- [ ] `PatientByEmailSpecification` created
- [ ] `RequestEmailConfirmationEndpoint` created (uses Identity service)
- [ ] `ConfirmEmailEndpoint` created (uses Identity service)
- [ ] Login endpoint updated to check email confirmation via Identity service

**MailDev Development Setup:**
- [ ] `scripts/start-maildev.sh` bash script created
- [ ] Script is executable (`chmod +x`)
- [ ] MailDev container starts successfully
- [ ] MailDev web UI accessible at http://localhost:1080
- [ ] SMTP server accessible at localhost:1025

**Database:**
- [ ] **No EF Core configuration changes needed** (Identity handles EmailConfirmed)
- [ ] **No migration needed** (EmailConfirmed already in AspNetUsers table)

**Documentation & Build:**
- [ ] All existing tests still pass
- [ ] Documentation updated
- [ ] Build successful

**Manual Testing (with MailDev):**
- [ ] MailDev container running via `./scripts/start-maildev.sh`
- [ ] Patient registration creates unconfirmed account (EmailConfirmed = false)
- [ ] Request confirmation endpoint sends email (visible in MailDev web UI)
- [ ] Confirm email endpoint activates account (EmailConfirmed = true)
- [ ] Unconfirmed patient cannot login (403 Forbidden)
- [ ] Confirmed patient can login successfully
- [ ] Other user types (Doctor, etc.) can login without confirmation

---

## Database Schema Changes

### No Domain Schema Changes

**No changes needed**:
- Patient aggregate table unchanged (email confirmation is Identity concern)
- `AspNetUsers` table already has `EmailConfirmed` column (ASP.NET Core Identity built-in)
- Identity framework handles token storage internally

**Verify**:
- `AspNetUsers.EmailConfirmed` column exists (should already exist from Identity setup)
- No migration needed for domain entities

---

## Configuration Example

### appsettings.json

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@medicalcenter.com",
    "FromName": "Medical Center"
  }
}
```

**Notes**:
- Use app-specific passwords for Gmail
- Consider using environment variables for sensitive data
- For production, use secure configuration management

---

## API Endpoints Summary

### Request Email Confirmation
- **Endpoint**: `POST /api/auth/patients/request-confirmation`
- **Auth**: Anonymous
- **Request**: `{ "email": "patient@example.com" }`
- **Response**: `200 OK` (email sent)
- **Behavior**: Generates token via Identity service, sends email, token expiration handled by Identity framework

### Confirm Email
- **Endpoint**: `POST /api/auth/patients/confirm-email`
- **Auth**: Anonymous
- **Request**: `{ "email": "patient@example.com", "token": "abc123..." }`
- **Response**: `200 OK` (email confirmed)
- **Behavior**: Validates token via Identity service, confirms email (sets EmailConfirmed = true), token cleared by Identity framework

### Login (Updated)
- **Endpoint**: `POST /api/auth/login`
- **Auth**: Anonymous
- **Behavior**: For Patient role, checks confirmation status via `IIdentityService.IsUserUnconfirmedAsync`. Returns 403 if unconfirmed.

---

## Business Rules Summary

1. **Patients Only**: Email confirmation applies only to Patient role
2. **Unconfirmed Cannot Login**: Patients with unconfirmed email (Identity concern) cannot login
3. **Identity Concern**: Email confirmation handled at Identity layer, not domain layer
4. **On-Demand Email**: Confirmation email is NOT sent automatically on registration
5. **Client-Initiated**: Client must call `/auth/patients/request-confirmation` to receive email
6. **Token Expiration**: Confirmation tokens expire (handled by Identity framework)
7. **Single Use Token**: Token is cleared after successful confirmation (handled by Identity framework)
8. **Other Users**: Doctor, HealthcareStaff, etc. can login without confirmation

---

## Security Considerations

1. **Token Generation**: Use cryptographically secure random tokens (Guid.NewGuid().ToString("N"))
2. **Token Expiration**: Enforce expiration to prevent token reuse
3. **Rate Limiting**: Consider rate limiting on request confirmation endpoint to prevent email spam
4. **Email Validation**: Validate email format before sending
5. **Error Messages**: Don't reveal if email exists in system (security through obscurity)
6. **HTTPS**: Ensure confirmation links use HTTPS
7. **Token Storage**: Tokens stored in database (not in URL if possible, but URL is acceptable for email links)

---

## Rollback Plan

If issues arise during implementation:

1. **Migration Rollback**: `dotnet ef database update <previous-migration>`
2. **Code Revert**: Revert changes in reverse order of implementation steps
3. **Database Cleanup**: Manually remove added columns if needed
4. **Configuration Cleanup**: Remove SMTP configuration from appsettings.json

---

## MailDev Development Setup

### Bash Script: scripts/start-maildev.sh

Create this bash script to easily manage the MailDev Docker container:

**File**: `scripts/start-maildev.sh`

```bash
#!/bin/bash

# ============================================================================
# MailDev Docker Container Startup Script
# ============================================================================
# Description: Starts MailDev SMTP server for development environment.
# MailDev captures all outgoing emails and provides a web UI to view them.
#
# Usage:
#   ./scripts/start-maildev.sh [options]
#
# Options:
#   --stop      Stop and remove the MailDev container
#   --restart   Restart the MailDev container
#   --logs      Show container logs
#   --help      Show this help message
#
# Web UI: http://localhost:1080
# SMTP Port: 1025
#
# Reference: https://hub.docker.com/r/maildev/maildev
# ============================================================================

set -e

CONTAINER_NAME="medicalcenter-maildev"
SMTP_PORT=1025
WEB_PORT=1080
IMAGE="maildev/maildev"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  MailDev - Development SMTP${NC}"
    echo -e "${BLUE}================================${NC}"
}

show_help() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  (none)      Start MailDev container"
    echo "  --stop      Stop and remove the MailDev container"
    echo "  --restart   Restart the MailDev container"
    echo "  --logs      Show container logs (follow mode)"
    echo "  --status    Show container status"
    echo "  --help      Show this help message"
    echo ""
    echo "Ports:"
    echo "  SMTP:     localhost:${SMTP_PORT}"
    echo "  Web UI:   http://localhost:${WEB_PORT}"
    echo ""
    echo "Reference: https://hub.docker.com/r/maildev/maildev"
    exit 0
}

check_docker() {
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed or not in PATH"
        echo "Please install Docker: https://docs.docker.com/get-docker/"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running"
        echo "Please start Docker Desktop or the Docker daemon"
        exit 1
    fi
}

stop_container() {
    if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Stopping MailDev container..."
        docker stop ${CONTAINER_NAME} 2>/dev/null || true
        docker rm ${CONTAINER_NAME} 2>/dev/null || true
        print_info "MailDev container stopped and removed"
    else
        print_warn "MailDev container is not running"
    fi
}

show_logs() {
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Showing MailDev logs (Ctrl+C to exit)..."
        docker logs -f ${CONTAINER_NAME}
    else
        print_error "MailDev container is not running"
        exit 1
    fi
}

show_status() {
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "MailDev is running"
        echo ""
        docker ps --filter "name=${CONTAINER_NAME}" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        echo "  📧 SMTP Server:  localhost:${SMTP_PORT}"
        echo "  🌐 Web UI:       http://localhost:${WEB_PORT}"
    else
        print_warn "MailDev is not running"
        echo "Run '$0' to start MailDev"
    fi
}

start_container() {
    check_docker
    print_header
    echo ""
    
    # Check if container already exists and is running
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "MailDev is already running"
        echo ""
        echo "  📧 SMTP Server:  localhost:${SMTP_PORT}"
        echo "  🌐 Web UI:       http://localhost:${WEB_PORT}"
        echo ""
        print_info "To stop: $0 --stop"
        exit 0
    fi
    
    # Remove stopped container if exists
    if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Removing stopped MailDev container..."
        docker rm ${CONTAINER_NAME} 2>/dev/null || true
    fi
    
    print_info "Starting MailDev container..."
    print_info "Pulling latest image..."
    
    docker pull ${IMAGE} --quiet
    
    docker run -d \
        --name ${CONTAINER_NAME} \
        -p ${SMTP_PORT}:1025 \
        -p ${WEB_PORT}:1080 \
        --restart unless-stopped \
        ${IMAGE}
    
    # Wait for container to be ready
    print_info "Waiting for MailDev to be ready..."
    sleep 2
    
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        echo ""
        print_info "MailDev is now running!"
        echo ""
        echo "  📧 SMTP Server:  localhost:${SMTP_PORT}"
        echo "  🌐 Web UI:       http://localhost:${WEB_PORT}"
        echo ""
        echo "  Configure your application with:"
        echo "    Host:     localhost"
        echo "    Port:     ${SMTP_PORT}"
        echo "    SSL:      false"
        echo "    UseMailDev: true"
        echo ""
        print_info "View captured emails at: http://localhost:${WEB_PORT}"
        print_info "To stop: $0 --stop"
    else
        print_error "Failed to start MailDev container"
        echo "Check Docker logs: docker logs ${CONTAINER_NAME}"
        exit 1
    fi
}

# Main script
case "${1:-}" in
    --stop)
        stop_container
        ;;
    --restart)
        stop_container
        start_container
        ;;
    --logs)
        show_logs
        ;;
    --status)
        show_status
        ;;
    --help|-h)
        show_help
        ;;
    "")
        start_container
        ;;
    *)
        print_error "Unknown option: $1"
        echo ""
        show_help
        ;;
esac
```

**Make executable** (on Windows with Git Bash or WSL):
```bash
chmod +x scripts/start-maildev.sh
```

---

### Docker Compose Integration

When running the application through Docker Compose, MailDev is included as a service. Add the following to `docker-compose.yml`:

```yaml
  maildev:
    image: maildev/maildev
    container_name: medicalcenter-maildev
    ports:
      - "${MAILDEV_WEB_PORT:-1080}:1080"   # Web UI
      - "${MAILDEV_SMTP_PORT:-1025}:1025"  # SMTP
    networks:
      - medicalcenter-network
    restart: unless-stopped
    profiles:
      - dev  # Only runs in dev profile: docker-compose --profile dev up
```

**Update the webapi service** to configure SMTP for MailDev when running via Docker Compose:

```yaml
  webapi:
    # ... existing configuration ...
    environment:
      # ... existing env vars ...
      - Smtp__Host=maildev
      - Smtp__Port=1025
      - Smtp__EnableSsl=false
      - Smtp__UseMailDev=true
      - Smtp__FromEmail=noreply@medicalcenter.local
      - Smtp__FromName=Medical Center (Dev)
    depends_on:
      - sqlserver
      - maildev  # Add dependency on maildev
```

**Usage:**
```bash
# Start all services including MailDev (dev profile)
docker-compose --profile dev up -d

# Or start without MailDev (production-like)
docker-compose up -d
```

**Important**: When using Docker Compose, the SMTP host is `maildev` (the container name), not `localhost`, because containers communicate over the internal Docker network.

---

### Development Workflow

#### Standalone Development (without Docker Compose)

1. **Start MailDev** before running the application:
   ```bash
   ./scripts/start-maildev.sh
   ```

2. **Run the application** with Development configuration:
   - `appsettings.Development.json` is automatically used
   - SMTP is configured for MailDev (localhost:1025)

3. **Trigger email confirmation**:
   - Register a patient via `POST /api/auth/register`
   - Request confirmation email via `POST /api/auth/patients/request-confirmation`

4. **View captured emails**:
   - Open http://localhost:1080 in your browser
   - All emails sent by the application are captured here
   - Click an email to view full HTML rendering

5. **Stop MailDev** when done:
   ```bash
   ./scripts/start-maildev.sh --stop
   ```

#### Docker Compose Development

1. **Start the full stack** with dev profile:
   ```bash
   docker-compose --profile dev up -d
   ```

2. **Trigger email confirmation** (same as above)

3. **View captured emails** at http://localhost:1080

4. **Stop all services**:
   ```bash
   docker-compose --profile dev down
   ```

> **Note**: MailDev is for development only. Never use in production.

---

## Notes

- **Separation of Concerns**: 
  - Email confirmation is an **Identity concern**, not a domain concern
  - SMTP abstraction in Core, implementation in Infrastructure
  - Identity service handles email confirmation logic
- **Options Pattern**: SMTP configuration follows ASP.NET Core Options pattern
- **Identity Framework**: Uses ASP.NET Core Identity's built-in `EmailConfirmed` property and token providers
- **On-Demand**: Email is not sent automatically - client controls when to request it
- **Patient Only**: Other user types are not affected by this feature
- **No Domain Changes**: Patient aggregate remains unchanged - all logic in Identity layer
- **No Migration Needed**: `EmailConfirmed` already exists in `AspNetUsers` table
- **MailDev for Development**: Use MailDev Docker container to capture and view emails locally

---

## Post-Implementation

After successful implementation:
1. Remove this plan document (`docs/PatientEmailConfirmationPlan.md`)
2. Update main documentation files
3. Commit changes with appropriate message
4. Verify all tests pass
5. Perform manual testing
6. Configure SMTP settings for production environment

---

**End of Plan**
