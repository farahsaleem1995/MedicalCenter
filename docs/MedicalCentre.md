# Ubiquitous Language

This document defines the shared language between domain experts and developers.

## Core Terms

| Term           | Meaning                                                      |
| -------------- | ------------------------------------------------------------ |
| Patient        | Person receiving medical care                                |
| Provider       | Actor delivering healthcare (Doctor, Lab, Hospital, Imaging) |
| Medical Record | A stored medical event                                       |
| Encounter      | A real interaction between a patient and a provider          |
| Action Log     | Tracked activity performed on patient data                   |

## Why "Encounter"

We chose the term "Encounter" because:

- It is part of global healthcare standards (HL7 / FHIR)
- It represents real-world medical interactions
- It avoids vague technical terms like "Assignment"

<br>
<br>

# High-Level Technical Architecture

## Architecture Overview

Three-layer architecture:

### Core Layer

- Aggregates and entities
- Domain rules
- Guard clauses
- Specifications
- Repository interfaces

### Infrastructure Layer

- EF Core
- Identity
- Specification implementations
- Audit interceptors

### Web API Layer

- FastEndpoints
- Validation
- Authorization
- Exception handling
- Action logging
- Request logging
- Result pattern

## Packages

| Layer          | Libraries                                   |
| -------------- | ------------------------------------------- |
| Core           | Ardalis.Specification, Ardalis.GuardClauses |
| Infrastructure | EF Core, Identity, Ardalis.Specification.EF |
| Web API        | FastEndpoints, FluentValidation, AutoMapper |

## Key Architectural Decisions

- UseCases layer removed to avoid MediatR/DTO complexity
- Result pattern lives in API layer
- Core remains framework-agnostic

<br>
<br>

# Business / Domain Model

## User Roles

| Role             | Description           |
| ---------------- | --------------------- |
| System Admin     | System management     |
| Patient          | Receives care         |
| Doctor           | Treats patients       |
| Healthcare Staff | Hospital/Clinic staff |
| Lab User         | Lab technician        |
| Imaging User     | Imaging technician    |

## Aggregates

### Patient

- Identity information
- Owns encounters

### MedicalRecord

- Medical content
- Attachments
- Creator info

Rules:

- Only creator can modify
- Attachments are immutable

### Encounter

- PatientId
- ProviderId
- ProviderType
- EncounterType
- Timestamp

Created automatically when records are added.

### ActionLog

- Tracks data access and changes
- Visible to patient

## Key Business Rules

- No manual "assignment"
- Encounters are created through real actions
- Patients do not manage provider relations

# Endpoint Draft - Medical Centre Automation System

## Design Principles

- Resource-based URLs (no verbs)
- RESTful structure
- Filtering via query strings
- Encounters replace vague "assignments"
- RBAC enforced via policies and claims

<br>

# Patient Endpoints

## Self Registration

POST /patients

## Self Access

GET /patients/self  
GET /patients/self/records  
GET /patients/self/action-logs  
GET /patients/self/report

<br>

# Doctor Endpoints

POST /doctors/records  
GET /doctors/records  
GET /doctors/encounters

### Supported Filters

?patientId=  
?dateFrom=  
?dateTo=  
?recordType=

<br>

# Healthcare Entity Endpoints

POST /healthcare/records  
GET /healthcare/records  
GET /healthcare/encounters

<br>

# Laboratory Endpoints

POST /labs/records  
GET /labs/records  
GET /labs/encounters

<br>

# Imaging Center Endpoints

POST /imaging/records  
GET /imaging/records  
GET /imaging/encounters

<br>

# Admin Endpoints

## Users Management

POST /admin/users  
GET /admin/users  
GET /admin/users/{id}  
PUT /admin/users/{id}  
DELETE /admin/users/{id}

### Filters

?role=  
?status=  
?organizationId=  
?createdBefore=  
?createdAfter=

## Records Management

GET /admin/records  
GET /admin/records/{id}

### Filters

?patientId=  
?providerId=  
?providerType=  
?dateFrom=  
?dateTo=

## Encounters Management

GET /admin/encounters  
GET /admin/encounters/{id}

### Filters

?patientId=  
?providerId=  
?providerType=  
?encounterType=  
?dateFrom=  
?dateTo=

<br>

# Folder Structure

```
Endpoints
├─ Admin
│   ├─ Users
│   ├─ Records
│   └─ Encounters
├─ Patients
│   ├─ Self
│   └─ Registration
├─ Doctors
│   ├─ Records
│   └─ Encounters
├─ Healthcare
│   ├─ Records
│   └─ Encounters
├─ Labs
│   ├─ Records
│   └─ Encounters
├─ Imaging
│   ├─ Records
│   └─ Encounters
```

# Identity Database Design

## 1. Users Table (EF Core Identity)

| Column       | Type     | Notes                                     |
| ------------ | -------- | ----------------------------------------- |
| Id           | Guid     | Primary Key                               |
| UserName     | string   | Login username                            |
| PasswordHash | string   | Identity password hash                    |
| Email        | string   | Email address                             |
| Role         | string   | Role discriminator (Admin, Patient, etc.) |
| FullName     | string   | Full name of user                         |
| IsActive     | bool     | Active status                             |
| CreatedAt    | DateTime | Timestamp                                 |
| UpdatedAt    | DateTime | Timestamp                                 |

---

## 2. Role-Specific Tables

### PatientDetails

| Column      | Type     | Notes              |
| ----------- | -------- | ------------------ |
| UserId      | Guid     | FK to Users.Id     |
| NationalId  | string   | Unique national ID |
| DateOfBirth | DateTime | Patient DOB        |

### DoctorDetails

| Column        | Type   | Notes                  |
| ------------- | ------ | ---------------------- |
| UserId        | Guid   | FK to Users.Id         |
| LicenseNumber | string | Medical license number |
| Specialty     | string | Medical specialty      |

### HealthcareDetails

| Column           | Type   | Notes                |
| ---------------- | ------ | -------------------- |
| UserId           | Guid   | FK to Users.Id       |
| OrganizationName | string | Hospital/Clinic name |
| Department       | string | Department           |

### LabDetails

| Column        | Type   | Notes              |
| ------------- | ------ | ------------------ |
| UserId        | Guid   | FK to Users.Id     |
| LabName       | string | Laboratory name    |
| LicenseNumber | string | Lab license number |

### ImagingDetails

| Column        | Type   | Notes                  |
| ------------- | ------ | ---------------------- |
| UserId        | Guid   | FK to Users.Id         |
| CenterName    | string | Imaging center name    |
| LicenseNumber | string | Imaging license number |

# Identity Domain Modeling & EF Core Mapping

## 1. Base User Aggregate

```csharp
public abstract class User : BaseEntity
{
    public string FullName { get; protected set; }
    public UserRole Role { get; protected set; }
    public bool IsActive { get; protected set; }

    protected User() { }
}
```

## 2. Derived Aggregates

### Patient

```csharp
public class Patient : User
{
    public string NationalId { get; private set; }
    public DateTime DateOfBirth { get; private set; }
}
```

### Doctor

```csharp
public class Doctor : User
{
    public string LicenseNumber { get; private set; }
    public string Specialty { get; private set; }
}
```

### Healthcare Entity

```csharp
public class HealthcareEntity : User
{
    public string OrganizationName { get; private set; }
    public string Department { get; private set; }
}
```

### Laboratory

```csharp
public class Laboratory : User
{
    public string LabName { get; private set; }
    public string LicenseNumber { get; private set; }
}
```

### Imaging Center

```csharp
public class ImagingCenter : User
{
    public string CenterName { get; private set; }
    public string LicenseNumber { get; private set; }
}
```

### 3. EF Core Mapping Strategy

- Use SQL Views to join Users + role-specific tables.
- Map views to domain aggregates in DbContext:

```csharp
modelBuilder.Entity<Patient>()
    .ToView("vw_Patients")
    .HasKey(p => p.Id);

modelBuilder.Entity<Doctor>()
    .ToView("vw_Doctors")
    .HasKey(d => d.Id);

modelBuilder.Entity<HealthcareEntity>()
    .ToView("vw_HealthcareEntities")
    .HasKey(h => h.Id);

modelBuilder.Entity<Laboratory>()
    .ToView("vw_Laboratories")
    .HasKey(l => l.Id);

modelBuilder.Entity<ImagingCenter>()
    .ToView("vw_ImagingCenters")
    .HasKey(i => i.Id);
```

### Notes

- Views handle the join between Users and role-specific tables.
- Keeps domain aggregates clean and framework-agnostic.
- AutoMapper or manual mapping can convert view entities to aggregates.

# Identity Services

## 1. Identity Service

### Responsibilities

- User registration (only patients can self-register; other users created by Admin)
- Password management
- Role-based authorization
- Retrieving user details by role

### Interface

```csharp
public interface IIdentityService
{
    Task<Result> RegisterPatientAsync(PatientRegistrationDto dto);
    Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<IReadOnlyList<User>> GetUsersByRoleAsync(UserRole role);
}
```

### Notes

- All service methods use the Result pattern; expected errors are returned as results, not exceptions.
- Role-based claims are used for authorization.

## 1. Token Provider Service

### Responsibilities

- JWT token generation
- Refresh token generation and validation

### Interface

```csharp
public interface ITokenProvider
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token, out ClaimsPrincipal? principal);
    bool ValidateRefreshToken(string token);
}
```

### Notes

- JWT contains user role claims.
- Refresh tokens are securely stored in the database and validated against it.
