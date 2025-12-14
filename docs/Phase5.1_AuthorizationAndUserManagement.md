# Phase 5.1: Authorization & User Management

## Overview

This phase addresses critical domain rule violations identified in Phase 5:
- **Patients cannot update their own medical attributes** - only authorized providers can
- **System Admin user management** - System admins can create and manage all user types
- **Role-based authorization** - Different roles have different permissions for medical attribute updates

## Domain Rules

### Medical Attribute Update Permissions

| Role | Can Update | Notes |
|------|------------|-------|
| **SystemAdmin** | All attributes | Full access |
| **Doctor** | All attributes | Full medical attribute management |
| **HealthcareStaff** | All attributes | Full medical attribute management |
| **Patient** | None | Patients cannot update their own attributes |
| **LabUser** | None | Lab users work with records, not patient attributes |
| **ImagingUser** | None | Imaging users work with records, not patient attributes |

### User Creation Permissions

| Role | Can Create | Notes |
|------|------------|-------|
| **SystemAdmin** | All user types | Full user management |
| **Patient** | Self (Patient) | Self-registration only |
| **Others** | None | Only SystemAdmin can create non-patient users |

## Implementation Tasks

### 1. Authorization Policies

#### 1.1 Update Existing Policy
- Update `CanModifyMedicalAttributes` policy to include `SystemAdmin`
- Ensure policy correctly restricts access to Doctor, HealthcareStaff, and SystemAdmin only

#### 1.2 Create Additional Policies (if needed)
- `CanManageUsers` - SystemAdmin only
- `CanViewAllPatients` - Already exists, verify it's correct

### 2. System Admin Seeding

#### 2.1 Create System Admin Seeder
- Create `SystemAdminSeeder.cs` in `src/MedicalCenter.Infrastructure/Data/Seeders/`
- Seed system admin user: `sys.admin@medicalcenter.com`
- Default password: Configurable (should be changed on first login)
- Role: `SystemAdmin`

#### 2.2 Integration with Existing Seeding
- Integrate with existing `SeedData()` extension method
- Ensure it runs after roles are seeded

### 3. Endpoint Refactoring

#### 3.1 Update Medical Attributes Endpoint
- **Current**: `PUT /patients/self/medical-attributes` (patients can update)
- **New**: `PUT /patients/{patientId}/medical-attributes` (providers update)
- Remove patient self-update capability
- Require `CanModifyMedicalAttributes` authorization policy
- Accept `patientId` as route parameter
- Verify patient exists before updating

#### 3.2 Endpoint Design Decision
**Decision: Single endpoint with authorization policies**

**Rationale:**
- Cleaner API design - one endpoint for all authorized roles
- Authorization logic handled by policies, not endpoint routing
- Easier to maintain and extend
- Follows RESTful principles (resource-based, not role-based URLs)

**Alternative Considered:**
- Separate endpoints per role (e.g., `/doctors/patients/{id}/medical-attributes`)
- **Rejected**: Would create code duplication and violate DRY principle

### 4. System Admin Endpoints

#### 4.1 User Management Endpoints

**Create User**
```http
POST /admin/users
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "email": "doctor@example.com",
  "password": "SecurePass123!",
  "role": "Doctor",
  "fullName": "Dr. John Smith",
  "specialty": "Cardiology" // For doctors
}
```

**Get User**
```http
GET /admin/users/{userId}
Authorization: Bearer {admin-token}
```

**List Users**
```http
GET /admin/users?role=Doctor&page=1&pageSize=10
Authorization: Bearer {admin-token}
```

**Update User**
```http
PUT /admin/users/{userId}
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "fullName": "Dr. John Smith Updated",
  "isActive": true
}
```

**Delete/Deactivate User**
```http
DELETE /admin/users/{userId}
Authorization: Bearer {admin-token}
```

#### 4.2 Endpoint Structure
- Group: `AdminGroup`
- Base path: `/admin/users`
- All endpoints require `RequireAdmin` policy

### 5. Identity Service Updates

#### 5.1 Extend CreateUserAsync
- Currently supports generic user creation
- Need to support creating all user types (Doctor, HealthcareStaff, LabUser, ImagingUser)
- Add role-specific properties (e.g., specialty for doctors)

#### 5.2 Create User DTOs
- `CreateDoctorRequest` - includes specialty
- `CreateHealthcareStaffRequest` - includes organizationName
- `CreateLabUserRequest` - includes labName
- `CreateImagingUserRequest` - includes centerName
- Generic `CreateUserRequest` for system admin

### 6. Domain Model Considerations

#### 6.1 User Creation Rules
- Only SystemAdmin can create non-patient users
- Patients can self-register (existing functionality)
- All user types must have corresponding domain entities created

#### 6.2 Medical Attribute Update Rules
- Updates must be done by authorized providers
- Patients can only view their own attributes
- All updates should be auditable (who updated, when)

## File Structure

```
src/MedicalCenter.WebApi/Endpoints/
├── Admin/
│   ├── AdminGroup.cs
│   ├── CreateUserEndpoint.cs
│   ├── CreateUserEndpoint.Request.cs
│   ├── CreateUserEndpoint.Response.cs
│   ├── CreateUserEndpoint.Validator.cs
│   ├── GetUserEndpoint.cs
│   ├── GetUserEndpoint.Response.cs
│   ├── ListUsersEndpoint.cs
│   ├── ListUsersEndpoint.Response.cs
│   ├── UpdateUserEndpoint.cs
│   ├── UpdateUserEndpoint.Request.cs
│   ├── UpdateUserEndpoint.Response.cs
│   ├── UpdateUserEndpoint.Validator.cs
│   ├── DeleteUserEndpoint.cs
│   └── DeleteUserEndpoint.Response.cs
└── Patients/
    ├── UpdatePatientMedicalAttributesEndpoint.cs (UPDATED)
    ├── UpdatePatientMedicalAttributesEndpoint.Request.cs (UPDATED)
    ├── UpdatePatientMedicalAttributesEndpoint.Response.cs
    └── UpdatePatientMedicalAttributesEndpoint.Validator.cs (UPDATED)

src/MedicalCenter.Infrastructure/
├── Data/Seeders/
│   └── SystemAdminSeeder.cs (NEW)
└── Authorization/
    └── AuthorizationExtensions.cs (UPDATED)
```

## Testing Strategy

**Important Note**: We are only doing **domain testing** following the **classical school** of unit testing. We test units of behavior (business goals), not units of code. Tests focus on domain logic, business rules, and invariants.

### Domain Tests (Classical School)
- Test domain business rules and invariants
- Test that patients cannot update their own medical attributes (domain rule)
- Test that medical attribute updates follow domain constraints
- Test user creation domain rules (only SystemAdmin can create non-patient users)
- Test value object equality and immutability
- Test aggregate consistency boundaries

### What We Do NOT Test (Following Classical School Principles)
- **Authorization policies** - Infrastructure concern, tested via integration tests later
- **System admin seeder** - Infrastructure concern, verified manually
- **Endpoints** - Presentation layer, tested via integration tests later
- **Repository implementations** - Infrastructure, tested indirectly via integration tests
- **Framework code** - Not domain logic

### Test Focus Areas
1. **Domain Rules**: Verify business rules are enforced in domain entities
2. **Invariants**: Test that aggregates maintain consistency
3. **Value Objects**: Test equality, immutability, and business constraints
4. **Business Logic**: Test domain methods that implement business rules

### Test Naming Convention
Tests should describe business scenarios, not methods:
- ✅ `Forbids_Patient_From_Updating_Own_MedicalAttributes`
- ✅ `Allows_Doctor_To_Update_Patient_MedicalAttributes` (domain rule validation)
- ❌ `UpdateMedicalAttributes_ThrowsException_WhenCalledByPatient`

## Migration Considerations

- No database schema changes required
- System admin user will be seeded via migration or application startup
- Consider adding migration to seed system admin if needed

## Security Considerations

- System admin password should be configurable via appsettings
- Consider requiring password change on first login
- All admin endpoints must require SystemAdmin role
- Medical attribute updates must verify patient exists
- Audit logging for all user management operations

## Verification Checklist

### Domain Layer (Classical School Testing) - COMPLETED
- ✅ Domain rules enforce medical attribute constraints (date validations, consistency)
- ✅ Domain tests verify business rules and invariants (131 tests passing)
- ✅ Patient authorization rules tested (medical attribute updates, consistency)
- ✅ User creation rules tested (all user types, role assignments)
- ✅ All domain tests pass (classical school approach - testing behavior, not implementation)

### Infrastructure Layer (Manual Verification)
- ✅ System admin user seeded successfully
- ✅ Authorization policies correctly restrict access
- ✅ Identity service supports creating all user types

### Presentation Layer (Manual Verification)
- ✅ Endpoints properly secured with authorization policies
- ✅ Patients cannot access update endpoint (authorization enforced)
- ✅ Doctors and HealthcareStaff can access update endpoint (authorization enforced)
- ✅ SystemAdmin endpoints created for user management

### Documentation
- ✅ Implementation plan updated with Phase 5.1
- ✅ Phase 5.1 plan document created
- ✅ Testing strategy documented (domain testing only, classical school)

