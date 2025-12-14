# Medical Center System - Features Documentation

## Overview

This document provides a comprehensive overview of all implemented features in the Medical Center Automation System.

## Authentication & Authorization

### User Registration

**Endpoint**: `POST /patients`

- Patient self-registration
- Creates both Identity user and Patient aggregate
- Returns JWT token and refresh token
- Validates email uniqueness and password strength

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

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe"
}
```

### User Login

**Endpoint**: `POST /auth/login`

- Authenticates user credentials
- Returns JWT token and refresh token
- Includes user role in response

**Request**:
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response**:
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

### Get Current User Information

**Endpoint**: `GET /auth/self`

- Returns authenticated user's generic information
- Available to all authenticated users
- Does not include sensitive information like `IsActive` status

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "Patient"
}
```

### Logout

**Endpoint**: `POST /auth/logout`

- Invalidates all refresh tokens for the authenticated user
- Requires authentication
- Returns 204 No Content on success

### User Roles

- **SystemAdmin**: System administrators with full access
- **Patient**: Patients receiving care
- **Doctor**: Medical doctors
- **HealthcareStaff**: Hospital/clinic staff
- **LabUser**: Laboratory technicians
- **ImagingUser**: Imaging technicians

## Patient Features

### Get Own Patient Data

**Endpoint**: `GET /patients/self`

- Returns authenticated patient's medical and patient-specific information
- Includes blood type and medical attributes summary
- Does not include generic user information (use `/auth/self` for that)
- Requires `RequirePatient` policy

**Response**:
```json
{
  "nationalId": "123456789",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "bloodType": "A+",
  "allergies": [...],
  "chronicDiseases": [...],
  "medications": [...],
  "surgeries": [...]
}
```

**Note**: Generic user information (ID, email, full name, role) should be retrieved from `/auth/self` endpoint.

### Get Medical Attributes

**Endpoint**: `GET /patients/self/medical-attributes`

- Returns all medical attributes for authenticated patient
- Includes allergies, chronic diseases, medications, surgeries
- Requires `RequirePatient` policy

## Medical Attributes Management

All medical attributes endpoints require `CanModifyMedicalAttributes` policy (Doctor, HealthcareStaff, SystemAdmin).

### Allergies

#### List Allergies
**Endpoint**: `GET /patients/{patientId}/allergies`

#### Create Allergy
**Endpoint**: `POST /patients/{patientId}/allergies`

**Request**:
```json
{
  "name": "Peanuts",
  "severity": "Severe",
  "notes": "Causes anaphylaxis"
}
```

#### Update Allergy
**Endpoint**: `PUT /patients/{patientId}/allergies/{allergyId}`

#### Delete Allergy
**Endpoint**: `DELETE /patients/{patientId}/allergies/{allergyId}`

### Chronic Diseases

#### List Chronic Diseases
**Endpoint**: `GET /patients/{patientId}/chronic-diseases`

#### Create Chronic Disease
**Endpoint**: `POST /patients/{patientId}/chronic-diseases`

**Request**:
```json
{
  "name": "Diabetes Type 2",
  "diagnosisDate": "2020-01-15T00:00:00Z",
  "notes": "Controlled with medication"
}
```

#### Update Chronic Disease
**Endpoint**: `PUT /patients/{patientId}/chronic-diseases/{chronicDiseaseId}`

#### Delete Chronic Disease
**Endpoint**: `DELETE /patients/{patientId}/chronic-diseases/{chronicDiseaseId}`

### Medications

#### List Medications
**Endpoint**: `GET /patients/{patientId}/medications`

#### Create Medication
**Endpoint**: `POST /patients/{patientId}/medications`

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

#### Update Medication
**Endpoint**: `PUT /patients/{patientId}/medications/{medicationId}`

#### Delete Medication
**Endpoint**: `DELETE /patients/{patientId}/medications/{medicationId}`

### Surgeries

#### List Surgeries
**Endpoint**: `GET /patients/{patientId}/surgeries`

#### Create Surgery
**Endpoint**: `POST /patients/{patientId}/surgeries`

**Request**:
```json
{
  "name": "Appendectomy",
  "date": "2015-06-10T00:00:00Z",
  "surgeon": "Dr. Smith",
  "notes": "Laparoscopic procedure"
}
```

#### Update Surgery
**Endpoint**: `PUT /patients/{patientId}/surgeries/{surgeryId}`

#### Delete Surgery
**Endpoint**: `DELETE /patients/{patientId}/surgeries/{surgeryId}`

## Admin Features

All admin endpoints require `RequireAdmin` policy (SystemAdmin only).

### User Management

#### List Users
**Endpoint**: `GET /users`

- Paginated list of all users
- Optional filtering by role and active status
- Supports pagination with `pageNumber` and `pageSize` query parameters

**Query Parameters**:
- `pageNumber` (optional, default: 1)
- `pageSize` (optional, default: 10)
- `role` (optional): Filter by user role
- `isActive` (optional): Filter by active status

**Response**:
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
**Endpoint**: `GET /users/{id}`

- Returns detailed user information
- Includes provider-specific details if applicable
- **Note**: Only system admins can see `IsActive` status. This field is not exposed in non-admin endpoints.

#### Create User
**Endpoint**: `POST /users`

- Creates provider users (Doctor, HealthcareStaff, LabUser, ImagingUser)
- Cannot create patients (use registration endpoint)
- Validates email uniqueness

**Request**:
```json
{
  "email": "doctor@example.com",
  "password": "SecurePass123!",
  "fullName": "Dr. Jane Smith",
  "role": "Doctor",
  "specialization": "Cardiology"
}
```

#### Update User
**Endpoint**: `PUT /users/{id}`

- Updates user information
- Cannot change email or role
- Updates provider-specific details

#### Delete User (Deactivate)
**Endpoint**: `DELETE /users/{id}`

- Soft-deletes user (sets `IsActive` to false)
- Does not permanently delete user data

#### Change Password
**Endpoint**: `PUT /users/{id}/password`

- Changes user password
- Validates password strength

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
  "currentPage": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true,
  "isFirstPage": true,
  "isLastPage": false,
  "firstPage": 1,
  "lastPage": 5,
  "fromItem": 1,
  "toItem": 10
}
```

## Validation

### Request Validation

All endpoints use FluentValidation for request validation:

- **Email**: Valid email format, uniqueness check
- **Password**: Minimum 8 characters, complexity requirements
- **Required Fields**: Non-nullable fields validated
- **Date Ranges**: Start date before end date
- **Enum Values**: Valid enum values

### Validation Error Response

```json
{
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

## API Documentation

### Swagger/OpenAPI

- **Swagger UI**: Available at `/swagger` when running
- **OpenAPI Spec**: Available at `/openapi/v1.json`
- **Endpoint Groups**: Organized by feature (Admin, Auth, Patients, Allergies, etc.)

## Error Responses

### Standard Error Format

```json
{
  "error": "Error message",
  "statusCode": 400
}
```

### HTTP Status Codes

- **200 OK**: Successful request
- **201 Created**: Resource created
- **400 Bad Request**: Validation error
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

## Security Features

### Authentication

- JWT Bearer token authentication
- Refresh token mechanism
- Token expiration and renewal

### Authorization

- Role-based access control (RBAC)
- Policy-based authorization
- Resource-based authorization (users can only access their own data)
- **Sensitive Information**: `IsActive` status is only exposed to system administrators through admin endpoints

### Password Security

- Minimum 8 characters
- Requires uppercase, lowercase, digit, and special character
- Password hashing via ASP.NET Core Identity

## Future Features

### Planned Features

- **Medical Records**: Create and manage medical records
- **Encounters**: Track patient-provider interactions
- **Action Logging**: Comprehensive audit trail
- **Patient Reports**: Generate patient health reports
- **Provider Endpoints**: Complete provider-specific endpoints

See [ImplementationPlan.md](ImplementationPlan.md) for detailed roadmap.

