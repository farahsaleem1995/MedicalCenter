# Medical Center System - Features Documentation

## Overview

This document provides a comprehensive overview of all implemented features in the Medical Center Automation System.

## Authentication & Authorization

### User Registration

**Endpoint**: `POST /auth/patients`

- Patient self-registration
- Creates both Identity user and Patient aggregate
- Returns 204 No Content on success
- Validates email uniqueness and password strength
- Email confirmation required (OTP sent via email)
- **Authorization**: None (public endpoint)

**Request**:
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "nationalId": "123456789",
  "dateOfBirth": "1990-01-01T00:00:00Z"
}
```

**Success Response**: `204 No Content`

**Error Responses**:

**400 Bad Request** (Validation Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "email": [
      "Email is required",
      "Email must be a valid email address"
    ],
    "password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter"
    ]
  }
}
```

**409 Conflict** (Email Already Exists):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "A user with email 'john.doe@example.com' already exists."
}
```

**Note**: After registration, patients must confirm their email using the email confirmation endpoint before they can generate access tokens.

### User Login

**Endpoint**: `POST /auth/login`

- Authenticates user credentials
- Returns JWT token and refresh token
- Includes user role in response
- **Authorization**: None (public endpoint)

**Request**:
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Success Response** (`200 OK`):
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

**Error Responses**:

**400 Bad Request** (Validation Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password is required"]
  }
}
```

**401 Unauthorized** (Invalid Credentials or Unconfirmed Email):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed. Invalid email or password."
}
```

### Token Refresh

**Endpoint**: `POST /auth/refresh-token`

- Refreshes expired JWT tokens
- Validates refresh token
- Returns new JWT and refresh tokens

**Request**:
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-new-refresh-token"
}
```

### Get Current User Information

**Endpoint**: `GET /auth/self`

- Returns authenticated user's generic information
- Available to all authenticated users
- Does not include sensitive information like `IsActive` status
- **Authorization**: Any authenticated user

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "Patient"
}
```

**Error Responses**:

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

### Logout

**Endpoint**: `POST /auth/logout`

- Revokes all refresh tokens for the authenticated user
- Requires authentication
- Returns 204 No Content on success

### Email Confirmation

**Request Email Confirmation Code**

**Endpoint**: `GET /auth/confirm?email={email}`

- Sends a 6-digit OTP code to the user's email for email confirmation
- Always returns 204 No Content (prevents user enumeration)
- Code expires in up to 9 minutes (configurable via Identity settings)

**Confirm Email**

**Endpoint**: `POST /auth/confirm`

- Confirms user's email using the 6-digit OTP code
- Returns 204 No Content on success
- Returns 400 Bad Request for invalid or expired code

**Request**:
```json
{
  "email": "john.doe@example.com",
  "code": "123456"
}
```

**Response**: `204 No Content`

**Note**: Unconfirmed users cannot generate access tokens. They must confirm their email first.

### Password Management

**Request Password Reset**

**Endpoint**: `GET /auth/password-reset?email={email}`

- Sends a 6-digit OTP code to the user's email for password reset
- Always returns 204 No Content (prevents user enumeration)
- Code expires in up to 9 minutes (configurable via Identity settings)

**Reset Password**

**Endpoint**: `POST /auth/password-reset`

- Resets user's password using the 6-digit OTP code
- Revokes all refresh tokens for the user (forces re-authentication)
- Returns 204 No Content on success
- Returns 400 Bad Request for invalid or expired code
- Returns 404 Not Found if user doesn't exist

**Request**:
```json
{
  "email": "john.doe@example.com",
  "code": "123456",
  "newPassword": "NewSecurePass123!"
}
```

**Response**: `204 No Content`

**Change Password (Authenticated Users)**

**Endpoint**: `PUT /auth/password`

- Changes password for authenticated users
- Requires current password verification
- Revokes all refresh tokens for the user (forces re-authentication)
- Returns 204 No Content on success
- Returns 400 Bad Request for invalid current password

**Request**:
```json
{
  "currentPassword": "OldSecurePass123!",
  "newPassword": "NewSecurePass123!"
}
```

**Response**: `204 No Content`

### User Roles

- **SystemAdmin**: System administrators with full access
- **Patient**: Patients receiving care
- **Doctor**: Medical doctors
- **HealthcareStaff**: Hospital/clinic staff
- **LabUser**: Laboratory technicians
- **ImagingUser**: Imaging technicians

## Admin User Management

Admin endpoints for managing all non-patient users. All endpoints require `RequireAdmin` policy (SystemAdmin role).

### List Users

**Endpoint**: `GET /admin/users`

- Lists all users with pagination, filtering, and sorting
- Supports filtering by role, active status, and national ID (partial match)
- Supports sorting by multiple fields
- Filtering by SystemAdmin role requires Super Administrator privileges
- **Authorization**: `RequireAdmin` policy (SystemAdmin role)

**Query Parameters**:
- `pageNumber` (default: 1, minimum: 1)
- `pageSize` (default: 10, minimum: 1, maximum: 100)
- `role` (optional): Filter by user role (Doctor, HealthcareStaff, LabUser, ImagingUser, Patient, SystemAdmin)
- `isActive` (optional): Filter by active status (true, false)
- `nationalId` (optional): Filter by national ID (partial match, case-insensitive, uses `EF.Functions.Like`)
- `sortBy` (optional): Sort field — `FullName` (default), `Email`, `Role`, `CreatedAt`, `NationalId`
- `sortDirection` (optional): `Asc` (default) or `Desc`

**Success Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "fullName": "Dr. Jane Smith",
      "email": "jane.smith@example.com",
      "nationalId": "1234567890",
      "role": "Doctor",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "doctorDetails": {
        "licenseNumber": "MD-1234",
        "specialty": "Cardiology"
      }
    }
  ],
  "metadata": {
    "totalCount": 50,
    "pageSize": 10,
    "currentPage": 1,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Get User by ID

**Endpoint**: `GET /admin/users/{id}`

- Retrieves user details by ID (includes deactivated users)
- Returns role-specific details alongside common properties
- **Authorization**: `RequireAdmin` policy (SystemAdmin role)

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Dr. Jane Smith",
  "email": "jane.smith@example.com",
  "nationalId": "1234567890",
  "role": "Doctor",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "doctorDetails": {
    "licenseNumber": "MD-1234",
    "specialty": "Cardiology"
  }
}
```

### Create User

**Endpoint**: `POST /admin/users`

- Creates a non-patient user (Doctor, HealthcareStaff, LabUser, ImagingUser, SystemAdmin)
- Creates both Identity user and domain entity in a transaction
- `NationalId` is **required** for all user types
- SystemAdmin creation requires Super Administrator privileges
- **Authorization**: `RequireAdmin` policy (SystemAdmin role)

**Request**:
```json
{
  "fullName": "Dr. Jane Smith",
  "email": "jane.smith@example.com",
  "password": "SecurePass123!",
  "role": "Doctor",
  "nationalId": "1234567890",
  "licenseNumber": "MD-1234",
  "specialty": "Cardiology"
}
```

**Role-specific fields**:
- **Doctor**: `licenseNumber` (required), `specialty` (required)
- **HealthcareStaff**: `organizationName` (required), `department` (required)
- **LabUser**: `labName` (required), `licenseNumber` (required)
- **ImagingUser**: `centerName` (required), `licenseNumber` (required)
- **SystemAdmin**: `corporateId` (required), `department` (required)

**Validation rules**:
- `fullName`: Required, max 200 characters
- `email`: Required, valid email, max 256 characters
- `password`: Required, min 8 characters
- `role`: Required, must be a non-Patient role
- `nationalId`: Required, max 50 characters

**Success Response** (`200 OK`):
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "jane.smith@example.com",
  "fullName": "Dr. Jane Smith",
  "role": "Doctor"
}
```

**Error Responses**:
- `400 Bad Request`: Validation errors
- `403 Forbidden`: Only Super Admins can create SystemAdmin accounts
- `409 Conflict`: Email already exists

### Update User

**Endpoint**: `PUT /admin/users/{id}`

- Updates user details (partial update — only provided fields are updated)
- Can update deactivated users
- SystemAdmin accounts can only be updated by Super Administrators
- Revokes all refresh tokens for the user after update
- **Authorization**: `RequireAdmin` policy (SystemAdmin role)

**Request**:
```json
{
  "fullName": "Dr. Jane Smith-Johnson",
  "nationalId": "9876543210",
  "specialty": "Interventional Cardiology"
}
```

**Common updatable fields** (all optional):
- `fullName`: Updated full name
- `nationalId`: Updated national ID

**Role-specific updatable fields** (all optional):
- **Doctor**: `specialty`
- **HealthcareStaff**: `organizationName`, `department` (both required together)
- **LabUser**: `labName`
- **ImagingUser**: `centerName`
- **SystemAdmin**: `corporateId`, `department`

**Success Response**: `204 No Content`

**Error Responses**:
- `400 Bad Request`: Validation errors
- `403 Forbidden`: Only Super Admins can update SystemAdmin accounts
- `404 Not Found`: User not found

## Patient Features

### Get Own Patient Data

**Endpoint**: `GET /patients/self`

- Returns authenticated patient's medical and patient-specific information
- Includes blood type and medical attributes summary
- Does not include generic user information (use `/auth/self` for that)
- **Authorization**: `RequirePatient` policy (Patient role only)

**Success Response** (`200 OK`):
```json
{
  "nationalId": "123456789",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "bloodType": "A+",
  "allergies": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Peanuts",
      "severity": "Severe",
      "notes": "Causes anaphylaxis"
    }
  ],
  "chronicDiseases": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Diabetes Type 2",
      "diagnosisDate": "2020-01-15T00:00:00Z",
      "notes": "Controlled with medication"
    }
  ],
  "medications": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "name": "Aspirin",
      "dosage": "100mg daily",
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": null,
      "notes": "For heart health"
    }
  ],
  "surgeries": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "name": "Appendectomy",
      "date": "2015-06-10T00:00:00Z",
      "surgeon": "Dr. Smith",
      "notes": "Laparoscopic procedure"
    }
  ]
}
```

**Error Responses**:

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Not a Patient):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Patient not found."
}
```

**Note**: Generic user information (ID, email, full name, role) should be retrieved from `/auth/self` endpoint.

### Get Medical Attributes

**Endpoint**: `GET /patients/self/medical-attributes`

- Returns all medical attributes for authenticated patient
- Includes allergies, chronic diseases, medications, surgeries
- **Authorization**: `RequirePatient` policy (Patient role only)

**Success Response** (`200 OK`):
```json
{
  "allergies": [
    {
      "id": "...",
      "name": "Peanuts",
      "severity": "Severe",
      "notes": "Causes anaphylaxis"
    }
  ],
  "chronicDiseases": [...],
  "medications": [...],
  "surgeries": [...]
}
```

## Practitioner Features

### Patient Lookup (Practitioners)

#### List Patients

**Endpoint**: `GET /patients`

- Lists patients with pagination
- Supports filtering by:
  - `searchTerm` (searches in full name, email, national ID)
  - `dateOfBirthFrom`, `dateOfBirthTo`
- **Returns active patients only**
- **Authorization**: `CanViewPatients` policy (Doctor, HealthcareStaff, LabUser, ImagingUser, SystemAdmin)

**Query Parameters**:
- `pageNumber` (default: 1, minimum: 1)
- `pageSize` (default: 10, minimum: 1, maximum: 100)
- `searchTerm` (optional)
- `dateOfBirthFrom` (optional)
- `dateOfBirthTo` (optional)

#### Get Patient By ID

**Endpoint**: `GET /patients/{patientId}`

- Retrieves basic patient information by ID
- **Optimized**: No longer includes large medical attribute collections (allergies, chronic diseases, medications, surgeries) for performance reasons. Use dedicated endpoints for these collections.
- **Authorization**: `CanViewPatients` policy (Doctor, HealthcareStaff, LabUser, ImagingUser, SystemAdmin)

### Get Own Practitioner Attributes

**Endpoint**: `GET /api/practitioners/self`

- Returns authenticated practitioner's custom attributes based on their role
- Supports all practitioner types: Doctor, HealthcareStaff, Laboratory, and ImagingCenter
- Returns role-specific attributes only (other attributes will be null)
- **Authorization**: `RequirePractitioner` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)

### Get Own Medical Records (Practitioner)

**Endpoint**: `GET /api/practitioners/self/records`

- Lists medical records created by the authenticated practitioner
- Supports filtering by:
  - `patientId` (optional)
  - `recordType` (optional)
  - `dateFrom`, `dateTo` (optional)
- Supports pagination
- **Authorization**: `CanViewRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser, SystemAdmin)

**Success Response** (`200 OK`):

**Doctor**:
```json
{
  "role": "Doctor",
  "licenseNumber": "MD12345",
  "specialty": "Cardiology",
  "organizationName": null,
  "department": null,
  "labName": null,
  "centerName": null,
  "corporateId": null
}
```

**HealthcareStaff**:
```json
{
  "role": "HealthcareStaff",
  "licenseNumber": null,
  "specialty": null,
  "organizationName": "City General Hospital",
  "department": "Emergency Department",
  "labName": null,
  "centerName": null,
  "corporateId": null
}
```

**LabUser**:
```json
{
  "role": "LabUser",
  "licenseNumber": "LAB78901",
  "specialty": null,
  "organizationName": null,
  "department": null,
  "labName": "City Medical Laboratory",
  "centerName": null,
  "corporateId": null
}
```

**ImagingUser**:
```json
{
  "role": "ImagingUser",
  "licenseNumber": "IMG45678",
  "specialty": null,
  "organizationName": null,
  "department": null,
  "labName": null,
  "centerName": "Advanced Imaging Center",
  "corporateId": null
}
```

**Error Responses**:

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Not a Practitioner):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Practitioner Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Practitioner not found."
}
```

**Note**: Generic user information (ID, email, full name, role) should be retrieved from `/auth/self` endpoint.

## Medical Attributes Management

Medical attributes endpoints use separate policies for view and modify operations:
- **View operations** (List): `CanViewMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin)
- **Modify operations** (Create, Update, Delete): `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin)

This separation provides flexibility for future role-based permission changes.

All medical attribute endpoints are scoped to a specific patient: `/patients/{patientId}/{attributeType}/{attributeId?}`

### Allergies

#### List Allergies
**Endpoint**: `GET /patients/{patientId}/allergies`

- Returns all allergies for a specific patient
- **Authorization**: `CanViewMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin)

**Success Response** (`200 OK`):
```json
{
  "allergies": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "patientId": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Peanuts",
      "severity": "Severe",
      "notes": "Causes anaphylaxis",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-02T00:00:00Z"
    }
  ]
}
```

**Error Responses**:

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Insufficient Permissions):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Patient with ID '550e8400-e29b-41d4-a716-446655440001' was not found."
}
```

#### Create Allergy
**Endpoint**: `POST /patients/{patientId}/allergies`

- Creates a new allergy for a specific patient
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "name": "Peanuts",
  "severity": "Severe",
  "notes": "Causes anaphylaxis"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Peanuts",
  "severity": "Severe",
  "notes": "Causes anaphylaxis",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Error Responses**:

**400 Bad Request** (Validation Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "name": ["Name is required"],
    "severity": ["Severity is required", "Severity must be a valid enum value"]
  }
}
```

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Insufficient Permissions):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Patient with ID '550e8400-e29b-41d4-a716-446655440001' was not found."
}
```

#### Update Allergy
**Endpoint**: `PUT /patients/{patientId}/allergies/{allergyId}`

- Updates an existing allergy (name cannot be changed)
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "severity": "Moderate",
  "notes": "Updated notes"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Peanuts",
  "severity": "Moderate",
  "notes": "Updated notes",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

**Error Responses**:

**400 Bad Request** (Validation Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "severity": ["Severity must be a valid enum value"]
  }
}
```

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Insufficient Permissions):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient or Allergy Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Allergy with ID '550e8400-e29b-41d4-a716-446655440000' was not found for patient."
}
```

#### Delete Allergy
**Endpoint**: `DELETE /patients/{patientId}/allergies/{allergyId}`

- Deletes an existing allergy
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**:

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Insufficient Permissions):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient or Allergy Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Allergy with ID '550e8400-e29b-41d4-a716-446655440000' was not found for patient."
}
```

### Chronic Diseases

#### List Chronic Diseases
**Endpoint**: `GET /patients/{patientId}/chronic-diseases`

- Returns all chronic diseases for a specific patient
- **Authorization**: `CanViewMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin)

**Success Response** (`200 OK`):
```json
{
  "chronicDiseases": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "patientId": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Diabetes Type 2",
      "diagnosisDate": "2020-01-15T00:00:00Z",
      "notes": "Controlled with medication",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-02T00:00:00Z"
    }
  ]
}
```

#### Create Chronic Disease
**Endpoint**: `POST /patients/{patientId}/chronic-diseases`

- Creates a new chronic disease for a specific patient
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "name": "Diabetes Type 2",
  "diagnosisDate": "2020-01-15T00:00:00Z",
  "notes": "Controlled with medication"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Diabetes Type 2",
  "diagnosisDate": "2020-01-15T00:00:00Z",
  "notes": "Controlled with medication",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

#### Update Chronic Disease
**Endpoint**: `PUT /patients/{patientId}/chronic-diseases/{chronicDiseaseId}`

- Updates an existing chronic disease (name and diagnosisDate cannot be changed)
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "notes": "Updated notes"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Diabetes Type 2",
  "diagnosisDate": "2020-01-15T00:00:00Z",
  "notes": "Updated notes",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

#### Delete Chronic Disease
**Endpoint**: `DELETE /patients/{patientId}/chronic-diseases/{chronicDiseaseId}`

- Deletes an existing chronic disease
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**: Same as Delete Allergy (401, 403, 404)

### Medications

#### List Medications
**Endpoint**: `GET /patients/{patientId}/medications`

- Returns all medications for a specific patient
- **Authorization**: `CanViewMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin)

**Success Response** (`200 OK`):
```json
{
  "medications": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "patientId": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Aspirin",
      "dosage": "100mg daily",
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": null,
      "notes": "For heart health",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-02T00:00:00Z"
    }
  ]
}
```

#### Create Medication
**Endpoint**: `POST /patients/{patientId}/medications`

- Creates a new medication for a specific patient
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "name": "Aspirin",
  "dosage": "100mg daily",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": null,
  "notes": "For heart health"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Aspirin",
  "dosage": "100mg daily",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": null,
  "notes": "For heart health",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

#### Update Medication
**Endpoint**: `PUT /patients/{patientId}/medications/{medicationId}`

- Updates an existing medication (name and startDate cannot be changed)
- EndDate must be after startDate
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "dosage": "200mg daily",
  "endDate": "2024-12-31T00:00:00Z",
  "notes": "Updated notes"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Aspirin",
  "dosage": "200mg daily",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T00:00:00Z",
  "notes": "Updated notes",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

#### Delete Medication
**Endpoint**: `DELETE /patients/{patientId}/medications/{medicationId}`

- Deletes an existing medication
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**: Same as Delete Allergy (401, 403, 404)

### Surgeries

#### List Surgeries
**Endpoint**: `GET /patients/{patientId}/surgeries`

- Returns all surgeries for a specific patient
- **Authorization**: `CanViewMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin)

**Success Response** (`200 OK`):
```json
{
  "surgeries": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "patientId": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Appendectomy",
      "date": "2015-06-10T00:00:00Z",
      "surgeon": "Dr. Smith",
      "notes": "Laparoscopic procedure",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-02T00:00:00Z"
    }
  ]
}
```

#### Create Surgery
**Endpoint**: `POST /patients/{patientId}/surgeries`

- Creates a new surgery for a specific patient
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "name": "Appendectomy",
  "date": "2015-06-10T00:00:00Z",
  "surgeon": "Dr. Smith",
  "notes": "Laparoscopic procedure"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Appendectomy",
  "date": "2015-06-10T00:00:00Z",
  "surgeon": "Dr. Smith",
  "notes": "Laparoscopic procedure",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

#### Update Surgery
**Endpoint**: `PUT /patients/{patientId}/surgeries/{surgeryId}`

- Updates an existing surgery (name and date cannot be changed)
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)

**Request**:
```json
{
  "surgeon": "Dr. Johnson",
  "notes": "Updated notes"
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Appendectomy",
  "date": "2015-06-10T00:00:00Z",
  "surgeon": "Dr. Johnson",
  "notes": "Updated notes",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

#### Delete Surgery
**Endpoint**: `DELETE /patients/{patientId}/surgeries/{surgeryId}`

- Deletes an existing surgery
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**: Same as Delete Allergy (401, 403, 404)

### Blood Type

#### Update Blood Type
**Endpoint**: `PUT /patients/{patientId}/blood-type`

- Updates the blood type for a specific patient
- **Authorization**: `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff)
- Both ABO and Rh must be provided together, or both omitted to clear the blood type

**Request**:
```json
{
  "abo": 1,
  "rh": 1
}
```

Or to clear the blood type:
```json
{
  "abo": null,
  "rh": null
}
```

**Success Response** (`200 OK`):
```json
{
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "bloodType": "A+"
}
```

**Error Responses**:

**400 Bad Request** (Validation Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "abo": ["ABO and Rh must both be provided or both omitted"],
    "rh": ["ABO and Rh must both be provided or both omitted"]
  }
}
```

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Insufficient Permissions):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Patient with ID '550e8400-e29b-41d4-a716-446655440001' was not found."
}
```

**Note**: When blood type is cleared, `bloodType` will be `null` in the response.

## Admin Features

### Get Own Admin Attributes

**Endpoint**: `GET /api/admin/self`

- Returns authenticated system admin's custom attributes
- Includes CorporateId and Department
- **Authorization**: `RequireAdmin` policy (SystemAdmin only)

**Success Response** (`200 OK`):
```json
{
  "corporateId": "SYS-ADMIN-001",
  "department": "IT Department"
}
```

**Error Responses**:

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Not a System Admin):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (System Admin Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "System admin not found."
}
```

**Note**: Generic user information (ID, email, full name, role) should be retrieved from `/auth/self` endpoint.

### User Management

All admin endpoints require `RequireAdmin` policy (SystemAdmin only).

### User Management

#### List Users
**Endpoint**: `GET /admin/users`

- Paginated list of all users
- Optional filtering by role and active status
- Supports pagination with `pageNumber` and `pageSize` query parameters
- Includes deactivated users (admin override)
- **Authorization**: `RequireAdmin` policy (SystemAdmin only)
- **Note**: All SystemAdmin users can view and retrieve SystemAdmin users in the list. Filtering by SystemAdmin role requires `CanManageAdmins` policy (Super Admin only).

**Query Parameters**:
- `pageNumber` (optional, default: 1, minimum: 1)
- `pageSize` (optional, default: 10, minimum: 1, maximum: 100)
- `role` (optional): Filter by user role (Doctor, HealthcareStaff, LabUser, ImagingUser, Patient, SystemAdmin)
- `isActive` (optional): Filter by active status (true, false)

**Success Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "john.doe@example.com",
      "fullName": "John Doe",
      "role": "Patient",
      "isActive": true
    }
  ],
  "metadata": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

#### Get User
**Endpoint**: `GET /admin/users/{id}`

- Returns detailed user information
- Includes role-specific details if applicable
- **Authorization**: `RequireAdmin` policy (SystemAdmin only)
- **Note**: Only system admins can see `IsActive` status. This field is not exposed in non-admin endpoints.
- **Note**: All SystemAdmin users can retrieve and view SystemAdmin user details. Only Super Admins can modify SystemAdmin accounts.

**Response Examples**:

**Patient**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "role": "Patient",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": {
    "nationalId": "123456789",
    "dateOfBirth": "1990-01-01T00:00:00Z",
    "bloodType": "A+"
  },
  "doctorDetails": null,
  "healthcareStaffDetails": null,
  "laboratoryDetails": null,
  "imagingCenterDetails": null
}
```

**Doctor**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Dr. Jane Smith",
  "email": "doctor@example.com",
  "role": "Doctor",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": {
    "licenseNumber": "MD12345",
    "specialty": "Cardiology"
  },
  "healthcareStaffDetails": null,
  "laboratoryDetails": null,
  "imagingCenterDetails": null
}
```

**HealthcareStaff**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Staff",
  "email": "staff@example.com",
  "role": "HealthcareStaff",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": {
    "organizationName": "City Hospital",
    "department": "Emergency"
  },
  "laboratoryDetails": null,
  "imagingCenterDetails": null
}
```

**LabUser**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Lab Technician",
  "email": "lab@example.com",
  "role": "LabUser",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": null,
  "laboratoryDetails": {
    "labName": "City Lab"
  },
  "imagingCenterDetails": null
}
```

**ImagingUser**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Imaging Technician",
  "email": "imaging@example.com",
  "role": "ImagingUser",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": null,
  "laboratoryDetails": null,
  "imagingCenterDetails": {
    "centerName": "City Imaging Center"
  }
}
```

**SystemAdmin**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "System Administrator",
  "email": "admin@example.com",
  "role": "SystemAdmin",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": null,
  "laboratoryDetails": null,
  "imagingCenterDetails": null,
  "systemAdminDetails": {
    "corporateId": "SYS-ADMIN-001",
    "department": "IT"
  }
}
```

#### Create User
**Endpoint**: `POST /admin/users`

- Creates practitioner users (Doctor, HealthcareStaff, LabUser, ImagingUser, SystemAdmin)
- Cannot create patients (use registration endpoint)
- Validates email uniqueness
- **Authorization**: `RequireAdmin` policy (SystemAdmin only). Creating SystemAdmin users requires `CanManageAdmins` policy (Super Admin only)

**Request** (Doctor):
```json
{
  "email": "doctor@example.com",
  "password": "SecurePass123!",
  "fullName": "Dr. Jane Smith",
  "role": "Doctor",
  "licenseNumber": "MD12345",
  "specialty": "Cardiology"
}
```

**Request** (HealthcareStaff):
```json
{
  "email": "staff@example.com",
  "password": "SecurePass123!",
  "fullName": "John Staff",
  "role": "HealthcareStaff",
  "organizationName": "City Hospital",
  "department": "Emergency"
}
```

**Request** (LabUser):
```json
{
  "email": "lab@example.com",
  "password": "SecurePass123!",
  "fullName": "Lab Technician",
  "role": "LabUser",
  "labName": "City Lab"
}
```

**Request** (ImagingUser):
```json
{
  "email": "imaging@example.com",
  "password": "SecurePass123!",
  "fullName": "Imaging Technician",
  "role": "ImagingUser",
  "centerName": "City Imaging Center"
}
```

**Request** (SystemAdmin):
```json
{
  "email": "admin@example.com",
  "password": "SecurePass123!",
  "fullName": "System Administrator",
  "role": "SystemAdmin",
  "corporateId": "SYS-ADMIN-001",
  "department": "IT"
}
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "doctor@example.com",
  "fullName": "Dr. Jane Smith",
  "role": "Doctor"
}
```

#### Update User
**Endpoint**: `PUT /admin/users/{id}`

- Updates user information
- Cannot change email or role
- Updates provider-specific details
- **Authorization**: `RequireAdmin` policy (SystemAdmin only). Updating SystemAdmin users (including CorporateId and Department) requires `CanManageAdmins` policy (Super Admin only)

**Request**:
```json
{
  "fullName": "Dr. Jane Smith Updated",
  "specialty": "Neurology"
}
```

**Response Examples**:

**Doctor**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Dr. Jane Smith Updated",
  "email": "doctor@example.com",
  "role": "Doctor",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": {
    "licenseNumber": "MD12345",
    "specialty": "Neurology"
  },
  "healthcareStaffDetails": null,
  "laboratoryDetails": null,
  "imagingCenterDetails": null
}
```

**HealthcareStaff**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Staff Updated",
  "email": "staff@example.com",
  "role": "HealthcareStaff",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": {
    "organizationName": "City Hospital Updated",
    "department": "Cardiology"
  },
  "laboratoryDetails": null,
  "imagingCenterDetails": null
}
```

**LabUser**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Lab Technician Updated",
  "email": "lab@example.com",
  "role": "LabUser",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": null,
  "laboratoryDetails": {
    "labName": "City Lab Updated"
  },
  "imagingCenterDetails": null
}
```

**ImagingUser**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "Imaging Technician Updated",
  "email": "imaging@example.com",
  "role": "ImagingUser",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "patientDetails": null,
  "doctorDetails": null,
  "healthcareStaffDetails": null,
  "laboratoryDetails": null,
  "imagingCenterDetails": {
    "centerName": "City Imaging Center Updated"
  }
}
```

#### Delete User (Deactivate)
**Endpoint**: `DELETE /admin/users/{id}`

- Soft-deletes user (sets `IsActive` to false)
- Does not permanently delete user data
- **Authorization**: `RequireAdmin` policy (SystemAdmin only). Deactivating SystemAdmin users requires `CanManageAdmins` policy (Super Admin only)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**: Standard admin error responses (401 Unauthorized, 403 Forbidden, 404 Not Found)

#### Change Password
**Endpoint**: `PUT /admin/users/{id}/password`

- Changes user password (admin can change without current password)
- Validates password strength
- **Authorization**: `RequireAdmin` policy (SystemAdmin only). Changing SystemAdmin passwords requires `CanManageAdmins` policy (Super Admin only)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**: Standard admin error responses (400 Bad Request for validation, 401 Unauthorized, 403 Forbidden, 404 Not Found)

**Request**:
```json
{
  "newPassword": "NewSecurePass123!"
}
```

## Pagination

All list endpoints return paginated results using the `PaginatedList<T>` pattern.

### Pagination Metadata

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true
}
```

## Enum Values

The API uses numeric enum values in requests and responses. API clients must use the numeric values (integers) when sending enum values in requests, and should expect numeric values in responses. Below are the mappings for all enums used in the API.

### BloodABO

Enum values for ABO blood group types:

| Numeric Value | String Name | Description |
|--------------|-------------|-------------|
| 1 | A | Blood type A |
| 2 | B | Blood type B |
| 3 | AB | Blood type AB |
| 4 | O | Blood type O |

**Usage**: Used in blood type requests and responses. When updating a patient's blood type, both `abo` (BloodABO) and `rh` (BloodRh) must be provided together.

### BloodRh

Enum values for Rh factor:

| Numeric Value | String Name | Description |
|--------------|-------------|-------------|
| 1 | Positive | Rh positive (+) |
| 2 | Negative | Rh negative (-) |

**Usage**: Used in blood type requests and responses. Combined with BloodABO to represent complete blood types (e.g., "A+", "B-", "AB+", "O-").

### UserRole

Enum values for user roles:

| Numeric Value | String Name | Description |
|--------------|-------------|-------------|
| 1 | SystemAdmin | System administrator |
| 2 | Patient | Patient user |
| 3 | Doctor | Doctor/physician |
| 4 | HealthcareStaff | Healthcare staff member |
| 5 | LabUser | Laboratory user |
| 6 | ImagingUser | Imaging center user |

**Usage**: Used in user management endpoints and authentication responses. String representation is returned in responses (e.g., `"role": "Patient"`), but numeric values are used in database storage and internal processing.

### RecordType

Enum values for medical record types:

| Numeric Value | String Name | Description |
|--------------|-------------|-------------|
| 1 | ConsultationNote | Consultation note |
| 2 | LaboratoryResult | Laboratory test result |
| 3 | ImagingReport | Imaging study report |
| 4 | Prescription | Prescription record |
| 5 | Diagnosis | Diagnosis record |
| 6 | TreatmentPlan | Treatment plan |
| 99 | Other | Other record type |

**Usage**: Used in medical records endpoints (future feature).

### Notes for API Clients

1. **Request Format**: Always send numeric enum values (integers) in requests:
   - Blood type enums: `"abo": 1, "rh": 1` (not `"abo": "A"` or `"rh": "Positive"`)
   - UserRole: `"role": 3` (not `"role": "Doctor"`)
   - RecordType: `"recordType": 2` (not `"recordType": "LaboratoryResult"`)

2. **Response Format**: Enum values in responses depend on the endpoint:
   - **BloodABO** and **BloodRh**: Always numeric values (e.g., `"abo": 1, "rh": 1`)
   - **UserRole**: Always string representation (e.g., `"role": "Patient"`, `"role": "Doctor"`) for backward compatibility and readability
   - **RecordType**: Numeric values when used in responses

3. **Null Values**: Some enum fields may be nullable. Use `null` to clear or omit optional enum values (e.g., clearing blood type by setting both `abo` and `rh` to `null`).

4. **Validation**: Invalid enum values will result in a 400 Bad Request validation error with details about the invalid value.

5. **Example Request (Update Blood Type)**:
```json
{
  "abo": 1,
  "rh": 1
}
```
This sets the blood type to "A+". The numeric values correspond to the enum mappings shown above.

## Validation

### Request Validation

All endpoints use FluentValidation for request validation:

- **Email**: Valid email format, uniqueness check
- **Password**: Minimum 8 characters, complexity requirements
- **Required Fields**: Non-nullable fields validated
- **Date Ranges**: Start date before end date, end date after start date
- **Enum Values**: Valid enum values
- **String Length**: Maximum length constraints

### Validation Error Response

Validation errors are returned in Problem Details format (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

The `errors` object contains field-specific validation messages, where:
- **Key**: The field name that failed validation
- **Value**: An array of error messages for that field

## API Documentation

### Swagger/OpenAPI

- **Swagger UI**: Available at root path (`/`) when running in development
- **OpenAPI Spec**: Available at `/openapi/v1.json`
- **JWT Authentication**: Swagger UI supports JWT Bearer token authentication
- **Endpoint Groups**: Organized by feature (Admin, Auth, Patients, Allergies, Chronic-Diseases, Medications, Surgeries)

## Error Responses

The API uses **RFC 7807 Problem Details** format for standardized error responses. All errors follow this consistent structure, making it easier for API clients to handle errors programmatically.

### Problem Details Format

All error responses follow the RFC 7807 Problem Details specification:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "See the errors field for details",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

### Standard Problem Details Fields

- **`type`**: A URI reference that identifies the problem type (typically a link to the HTTP status code specification)
- **`title`**: A short, human-readable summary of the problem type
- **`status`**: The HTTP status code
- **`detail`**: A human-readable explanation specific to this occurrence of the problem
- **`errors`**: (Optional) Additional error details, typically used for validation errors

### HTTP Status Codes

- **200 OK**: Successful request
- **201 Created**: Resource created successfully
- **204 No Content**: Successful deletion or update (no response body)
- **400 Bad Request**: Validation error or invalid request
- **401 Unauthorized**: Authentication required or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate email)
- **500 Internal Server Error**: Server error

### Error Response Examples

#### Validation Error (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "email": [
      "Email is required",
      "Email must be a valid email address"
    ],
    "password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one digit"
    ],
    "dateOfBirth": [
      "Date of birth cannot be in the future"
    ]
  }
}
```

#### Authentication Error (401 Unauthorized)

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication failed. Invalid email or password."
}
```

#### Authorization Error (403 Forbidden)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

#### Resource Not Found (404 Not Found)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Patient with ID '550e8400-e29b-41d4-a716-446655440000' was not found."
}
```

#### Conflict Error (409 Conflict)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "A user with email 'john.doe@example.com' already exists."
}
```

#### Domain/Business Rule Error (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "A transaction is already in progress."
}
```

#### Server Error (500 Internal Server Error)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred while processing your request."
}
```

### Error Handling Best Practices

1. **Always check the `status` field** to determine the HTTP status code
2. **Use the `detail` field** for user-facing error messages
3. **Check the `errors` field** for validation errors (field-specific errors)
4. **Use the `type` field** for programmatic error handling (if needed)
5. **Handle 401 errors** by refreshing the authentication token
6. **Handle 403 errors** by checking user permissions
7. **Log the full error response** for debugging purposes (but don't expose sensitive details to end users)

### Validation Error Details

When validation fails, the `errors` object contains field-specific error messages:

- **Key**: The field name that failed validation
- **Value**: An array of error messages for that field

Example:
```json
{
  "errors": {
    "email": ["Email is required", "Email must be a valid email address"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

### Global Exception Handling

The API includes global exception handling middleware that:
- Catches unhandled exceptions
- Converts them to Problem Details format
- Logs errors for debugging
- Returns consistent error responses

This ensures that even unexpected errors follow the Problem Details format.

## Security Features

### Authentication

- JWT Bearer token authentication
- Refresh token mechanism
- Token expiration and renewal
- Secure password hashing via ASP.NET Core Identity

### Authorization

- **Role-Based Access Control (RBAC)**: Primary authorization mechanism
- **Policy-Based Authorization**: Named policies for fine-grained control
  - `RequireAdmin`: SystemAdmin only
  - `RequirePatient`: Patient only
  - `CanManageAdmins`: Super Admin only (for managing SystemAdmin accounts)
  - `CanViewMedicalAttributes`: Doctor, HealthcareStaff, SystemAdmin
  - `CanModifyMedicalAttributes`: Doctor, HealthcareStaff, SystemAdmin
  - `CanViewRecords`: Doctor, HealthcareStaff, LabUser, ImagingUser
  - `CanModifyRecords`: Doctor, HealthcareStaff, LabUser, ImagingUser
  - `CanViewAllPatients`: Doctor, HealthcareStaff, SystemAdmin
  - `CanViewPatients`: Doctor, HealthcareStaff, LabUser, ImagingUser, SystemAdmin
- **Resource-Based Authorization**: Users can only access their own data (for patient endpoints)
- **Sensitive Information**: `IsActive` status is only exposed to system administrators through admin endpoints
- **SystemAdmin Management**: All SystemAdmin users can retrieve/view SystemAdmin accounts. Creating, updating, deleting, and changing passwords for SystemAdmin accounts require `CanManageAdmins` policy (Super Admin only)

### Password Security

- Minimum 8 characters
- Requires uppercase, lowercase, digit, and special character
- Password hashing via ASP.NET Core Identity

## Medical Records

Medical records allow providers to create, view, and manage medical records for patients. Records can include file attachments.

### File Upload

**Endpoint**: `POST /api/records/attachments/upload`

- Uploads a file attachment that can be referenced when creating medical records
- Returns attachment metadata including fileId
- **Authorization**: `CanModifyRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)
- Maximum file size: 10MB
- Allowed content types: PDF, images (JPEG, PNG), documents (DOC, DOCX, XLS, XLSX)

**Request**: Multipart form data
- `file`: IFormFile (required)

**Success Response** (`200 OK`):
```json
{
  "attachmentId": "550e8400-e29b-41d4-a716-446655440000",
  "fileName": "lab-result.pdf",
  "fileSize": 1024000,
  "contentType": "application/pdf",
  "uploadedAt": "2024-01-01T00:00:00Z"
}
```

### Create Medical Record

**Endpoint**: `POST /api/records`

- Creates a new medical record for a patient
- Practitioner is automatically set from authenticated user
- Can optionally include attachment references from previously uploaded files
- **Authorization**: `CanModifyRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)

**Request**:
```json
{
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "recordType": 2,
  "title": "Blood Test Results",
  "content": "Patient shows elevated glucose levels...",
  "attachmentIds": ["550e8400-e29b-41d4-a716-446655440000"]
}
```

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "practitionerId": "550e8400-e29b-41d4-a716-446655440003",
  "recordType": 2,
  "title": "Blood Test Results",
  "content": "Patient shows elevated glucose levels...",
  "attachments": [
    {
      "fileId": "550e8400-e29b-41d4-a716-446655440000",
      "fileName": "lab-result.pdf",
      "fileSize": 1024000,
      "contentType": "application/pdf",
      "uploadedAt": "2024-01-01T00:00:00Z"
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### List Medical Records (Provider)

**Endpoint**: `GET /api/records`

- Lists medical records created by the current authenticated provider
- Supports filtering by patient, record type, and date range
- Supports pagination with `pageNumber` and `pageSize` query parameters
- **Authorization**: `CanViewRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)

**Query Parameters**:
- `pageNumber` (optional, default: 1, minimum: 1)
- `pageSize` (optional, default: 10, minimum: 1, maximum: 100)
- `patientId` (optional): Filter by patient ID
- `recordType` (optional): Filter by record type
- `dateFrom` (optional): Filter records from this date
- `dateTo` (optional): Filter records to this date

**Success Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "patientId": "550e8400-e29b-41d4-a716-446655440001",
      "patient": {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "fullName": "John Doe",
        "email": "john.doe@example.com"
      },
      "practitionerId": "550e8400-e29b-41d4-a716-446655440003",
      "practitioner": {
        "fullName": "Dr. Jane Smith",
        "email": "doctor@example.com",
        "role": 3
      },
      "recordType": 2,
      "title": "Blood Test Results",
      "createdAt": "2024-01-01T00:00:00Z",
      "attachmentCount": 1
    }
  ],
  "metadata": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

### Get Medical Record

**Endpoint**: `GET /api/records/{recordId}`

- Gets a specific medical record
- Only the practitioner who created the record can view it (enforced at endpoint level)
- **Authorization**: `CanViewRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)

**Success Response** (`200 OK`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "practitionerId": "550e8400-e29b-41d4-a716-446655440003",
  "recordType": 2,
  "title": "Blood Test Results",
  "content": "Patient shows elevated glucose levels...",
  "attachments": [...],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

### Update Medical Record

**Endpoint**: `PUT /api/records/{recordId}`

- Updates a medical record
- Only the practitioner who created the record can modify it (enforced at endpoint level)
- Attachments cannot be modified through this endpoint (use add/remove attachment endpoints)
- **Authorization**: `CanModifyRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)

**Request**:
```json
{
  "title": "Updated Title",
  "content": "Updated content"
}
```

**Response**: Same as Get Medical Record

### Delete Medical Record

**Endpoint**: `DELETE /api/records/{recordId}`

- Soft deletes a medical record
- Only the creator can delete the record (enforced at endpoint level)
- **Authorization**: `CanModifyRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

**Error Responses**: Standard error responses (401 Unauthorized, 403 Forbidden, 404 Not Found)

### Add Attachment to Record

**Endpoint**: `POST /api/records/{recordId}/attachments`

- Uploads a file attachment and adds it to an existing medical record
- Only the practitioner who created the record can add attachments (enforced at endpoint level)
- **Authorization**: `CanModifyRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)
- Maximum file size: 10MB (configurable)
- Allowed content types: PDF, images (JPEG, PNG), documents (DOC, DOCX, XLS, XLSX)
- Maximum attachments per record: 10 (configurable)

**Request**: Multipart form data
- `file`: IFormFile (required)

**Response**:
```json
{
  "attachmentId": "550e8400-e29b-41d4-a716-446655440000",
  "fileName": "additional-report.pdf",
  "fileSize": 2048000,
  "contentType": "application/pdf",
  "uploadedAt": "2024-01-02T00:00:00Z"
}
```

### Remove Attachment from Record

**Endpoint**: `DELETE /api/records/{recordId}/attachments/{attachmentId}`

- Removes an attachment from an existing medical record
- Only the creator of the record can remove attachments (enforced at endpoint level)
- The file itself is not deleted from storage (only the reference is removed)
- **Authorization**: `CanModifyRecords` policy (Doctor, HealthcareStaff, LabUser, ImagingUser)
- Returns 204 No Content on success

**Success Response**: `204 No Content`

### Download Attachment

**Endpoint**: `GET /api/records/{recordId}/attachments/{attachmentId}/download`

- Downloads a file attachment from a medical record
- Practitioners can download attachments from records they created
- Patients can download attachments from their own records
- **Authorization**: `RequirePatientOrPractitioner` policy (Patient, Doctor, HealthcareStaff, LabUser, ImagingUser)

**Success Response**: File stream with appropriate Content-Type header

**Error Responses**: Standard error responses (401 Unauthorized, 403 Forbidden, 404 Not Found)

### Generate Medical Report

**Endpoint**: `GET /patients/self/report`

- Generates a PDF medical report for the authenticated patient
- Includes patient information, medical attributes (allergies, chronic diseases, medications, surgeries), and medical records
- Supports optional date filtering for medical records
- Returns PDF file as a stream
- **Authorization**: `RequirePatient` policy (Patient role only)

**Query Parameters**:
- `dateFrom` (optional, DateTime): Filter medical records from this date (inclusive). Only records created on or after this date will be included.
- `dateTo` (optional, DateTime): Filter medical records to this date (inclusive). Only records created on or before this date will be included.

**Request Example**:
```
GET /patients/self/report?dateFrom=2024-01-01T00:00:00Z&dateTo=2024-12-31T23:59:59Z
```

**Success Response** (`200 OK`):
- Content-Type: `application/pdf`
- Content-Disposition: `attachment; filename="MedicalReport_{PatientName}_{Date}.pdf"`
- Body: PDF file stream

The PDF report includes:
1. **Header**: Medical Center title and generation timestamp
2. **Patient Information**: Full name, email, National ID, date of birth, blood type
3. **Medical Attributes**:
   - Allergies (with severity and notes)
   - Chronic Diseases (with diagnosis date and notes)
   - Medications (with dosage, date range, and notes)
   - Surgeries (with surgeon and notes)
4. **Medical Records**: All active medical records within the date range (if specified), including:
   - Record type
   - Title and content
   - Creation date
   - Practitioner name
   - Attachment count
5. **Footer**: Confidentiality notice

**Error Responses**:

**400 Bad Request** (Validation Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains validation errors",
  "errors": {
    "dateFrom": ["DateFrom cannot be in the future."],
    "dateTo": ["DateTo must be greater than or equal to DateFrom."]
  }
}
```

**401 Unauthorized** (Not Authenticated):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required."
}
```

**403 Forbidden** (Not a Patient):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**404 Not Found** (Patient Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Patient not found."
}
```

**Note**: The report is generated using QuestPDF library and includes all patient data available at the time of generation. If no date filters are provided, all active medical records are included.

### Patient View Records

**Endpoint**: `GET /api/patients/self/records`

- Lists all medical records for the authenticated patient
- Supports filtering by record type and date range
- Supports pagination with `pageNumber` and `pageSize` query parameters
- **Authorization**: `RequirePatient` policy (Patient role only)

**Query Parameters**:
- `pageNumber` (optional, default: 1, minimum: 1)
- `pageSize` (optional, default: 10, minimum: 1, maximum: 100)
- `recordType` (optional): Filter by record type
- `dateFrom` (optional): Filter records from this date
- `dateTo` (optional): Filter records to this date

**Response**:
```json
{
  "records": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "recordType": 2,
      "title": "Blood Test Results",
      "createdAt": "2024-01-01T00:00:00Z",
      "attachmentCount": 1
    }
  ],
  "metadata": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

**Endpoint**: `GET /api/patients/self/records/{recordId}`

- Gets a specific medical record for the authenticated patient
- Returns 404 if record does not belong to patient
- **Authorization**: `RequirePatient` policy (Patient role only)

**Success Response** (`200 OK`): Same as Get Medical Record

**Error Responses**: Standard error responses (401 Unauthorized, 403 Forbidden, 404 Not Found)

### Business Rules

- **Only Practitioner Can Modify**: Only the practitioner who created a record can modify or delete it
- **Only Practitioner Can Manage Attachments**: Only the practitioner can add or remove attachments from a record
- **Attachments Can Be Removed**: Attachments can be removed from records by the practitioner, but the file itself remains in storage for audit purposes
- **Soft Delete**: Records are soft-deleted (IsActive = false), attachment metadata is deleted, but files are kept
- **Multiple Attachments**: Records can have multiple attachments (up to 10 per record, configurable)
- **File Storage**: Files are stored separately from database records; removing an attachment from a record does not delete the file

## Action Logging

The Action Log system tracks business-critical operations for audit and compliance purposes. It uses a global post-processor that checks for `[ActionLog]` attributes on endpoints and only logs successful requests (2xx status codes).

### Get Action Logs

**Endpoint**: `GET /api/action-logs`

**Authorization**: `CanViewActionLog` policy (SystemAdmin role or AdminTier claim)

**Query Parameters**:
- `pageNumber` (optional, default: 1, minimum: 1)
- `pageSize` (optional, default: 20, minimum: 1, maximum: 100)
- `startDate` (optional, ISO 8601 format): Filter by start date (inclusive)
- `endDate` (optional, ISO 8601 format): Filter by end date (inclusive)
- `userId` (optional, Guid): Filter by user ID
- `actionName` (optional, string): Filter by action name

**Response**:
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "actionName": "CreateUser",
      "description": "System administrator created a new user account",
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "payload": "{\"email\":\"user@example.com\",\"role\":\"Doctor\"}",
      "executedAt": "2024-01-01T12:00:00Z"
    }
  ],
  "metadata": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

**Note**: Results are always ordered by `ExecutedAt` descending (newest first).

### Marked Endpoints

The following business-critical endpoints are marked with `[ActionLog]` attribute:

**Admin**:
- `CreateUserEndpoint` - "System administrator created a new user account"
- `UpdateUserEndpoint` - "System administrator updated a user account"
- `DeleteUserEndpoint` - "System administrator deactivated a user account"
- `ChangePasswordEndpoint` - "System administrator changed a user's password"

**Auth**:
- `RegisterPatientEndpoint` - "New patient registered"
- `ChangePasswordEndpoint` - "User changed their password"

**Records**:
- `CreateRecordEndpoint` - "Medical record created"
- `UpdateRecordEndpoint` - "Medical record updated"
- `DeleteRecordEndpoint` - "Medical record deleted"
- `GetRecordEndpoint` - "Medical record viewed"
- `DownloadAttachmentEndpoint` - "Medical record attachment downloaded"
- `UploadAttachmentEndpoint` - "File attachment uploaded"
- `AddAttachmentToRecordEndpoint` - "Attachment added to medical record"
- `RemoveAttachmentFromRecordEndpoint` - "Attachment removed from medical record"

**Patients**:
- All allergy endpoints (Create, Update, Delete)
- All chronic disease endpoints (Create, Update, Delete)
- All medication endpoints (Create, Update, Delete)
- All surgery endpoints (Create, Update, Delete)
- `UpdateBloodTypeEndpoint` - "Patient blood type updated"

### Key Features

- **Attribute-Based**: Endpoints are marked with `[ActionLog("description")]` attribute
- **Global Processor**: Single `ActionLogProcessor` registered globally via `c.Endpoints.Configurator`
- **Selective Logging**: Only logs endpoints with the attribute
- **Success-Only**: Only records successful requests (2xx status codes)
- **Queue-Based**: Fire-and-forget pattern using background service
- **Aggregate Root**: `ActionLogEntry` is a domain aggregate root (not an auditable entity)

## Database Seeding

### Seed Database with Fake Data

**Endpoint**: `POST /api/dev/seed`

- Seeds the database with realistic fake data for testing and presentation purposes
- Uses [Bogus](https://github.com/bchavez/Bogus) library for generating realistic test data
- Only available in Development environment
- Generates data for all aggregates: Patients, Doctors, HealthcareStaff, Laboratories, ImagingCenters, MedicalRecords
- Creates Identity users with configurable default password
- Returns seeding summary as downloadable markdown file
- **Authorization**: None (Development only - environment check)

**Request**:
```json
{
  "doctorCount": 20,
  "healthcareStaffCount": 15,
  "laboratoryCount": 5,
  "imagingCenterCount": 5,
  "patientCount": 100,
  "medicalRecordsPerPatientMin": 2,
  "medicalRecordsPerPatientMax": 10,
  "clearExistingData": false,
  "defaultPassword": "Test@123!"
}
```

**Request Parameters** (all optional):
- `doctorCount` (int, default: 20): Number of doctors to create
- `healthcareStaffCount` (int, default: 15): Number of healthcare staff to create
- `laboratoryCount` (int, default: 5): Number of laboratories to create
- `imagingCenterCount` (int, default: 5): Number of imaging centers to create
- `patientCount` (int, default: 100): Number of patients to create
- `medicalRecordsPerPatientMin` (int, default: 2): Minimum medical records per patient
- `medicalRecordsPerPatientMax` (int, default: 10): Maximum medical records per patient
- `clearExistingData` (bool, default: false): Whether to clear existing seeded data before seeding
- `defaultPassword` (string, default: "Test@123!"): Default password for all seeded users

**Success Response**: 
- Status: `200 OK`
- Content-Type: `text/markdown`
- Content-Disposition: `attachment; filename="SeedingSummary.md"`
- Body: Markdown file stream containing:
  - All user credentials (email, password, role, additional info)
  - Data statistics (counts, distributions, date ranges)
  - Sample IDs for testing
  - Seeding timestamp and options used

**Error Responses**:

**403 Forbidden** (Not Development Environment):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "This endpoint is only available in Development environment"
}
```

**500 Internal Server Error** (Seeding Failed):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred during database seeding"
}
```

### Generated Data

The seeding process generates:

**Practitioners**:
- Doctors with specialties (Cardiology, Pediatrics, Orthopedics, etc.) and license numbers
- Healthcare staff with organizations and departments
- Laboratories with lab names and license numbers
- Imaging centers with center names and license numbers

**Patients**:
- Patients with 11-digit national IDs and dates of birth (18-80 years old)
- Blood types (random distribution of ABO and Rh combinations)
- Medical attributes:
  - Allergies (0-5 per patient): Common allergies with severity levels
  - Chronic diseases (0-3 per patient): Common diseases with diagnosis dates
  - Medications (0-6 per patient): Common medications with dosages and date ranges
  - Surgeries (0-4 per patient): Common surgeries with dates and surgeon names

**Medical Records**:
- Records distributed across all record types (ConsultationNote, LaboratoryResult, ImagingReport, Prescription, Diagnosis, TreatmentPlan)
- Records linked to patients and practitioners
- Realistic medical content based on record type
- Historical dates (last 5 years)

### Key Features

- **DDD Compliant**: All entities created through domain factory methods
- **Realistic Data**: Uses Bogus for varied, realistic fake data
- **Identity Integration**: Creates ASP.NET Core Identity users with roles
- **Medical Attributes**: Patients include comprehensive medical history
- **Relationship Integrity**: Medical records properly reference patients and practitioners
- **Summary Document**: Auto-generates downloadable summary with all credentials
- **Configurable**: Customize counts, password, and other options via request

**Note**: This endpoint is only available in Development environment. In Production, this endpoint will return 403 Forbidden.

See [DatabaseSeedingPlan.md](DatabaseSeedingPlan.md) for detailed implementation documentation.

## Future Features

### Planned Features

- **Patient Reports**: Generate patient health reports
- **Practitioner Endpoints**: Additional practitioner-specific endpoints
- **Lab Results**: Enhanced laboratory test result management
- **Imaging Studies**: Enhanced imaging study record management

See [ImplementationPlan.md](ImplementationPlan.md) for detailed roadmap.
