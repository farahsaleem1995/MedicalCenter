# Medical Center System - Features Documentation

## Overview

This document provides a comprehensive overview of all implemented features in the Medical Center Automation System.

## Authentication & Authorization

### User Registration

**Endpoint**: `POST /auth/patients`

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

**Response**:
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
- Requires `CanViewMedicalAttributes` policy

**Response**:
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

#### Create Allergy
**Endpoint**: `POST /patients/{patientId}/allergies`

- Creates a new allergy for a specific patient
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "name": "Peanuts",
  "severity": "Severe",
  "notes": "Causes anaphylaxis"
}
```

**Response**:
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

#### Update Allergy
**Endpoint**: `PUT /patients/{patientId}/allergies/{allergyId}`

- Updates an existing allergy (name cannot be changed)
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "severity": "Moderate",
  "notes": "Updated notes"
}
```

**Response**:
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

#### Delete Allergy
**Endpoint**: `DELETE /patients/{patientId}/allergies/{allergyId}`

- Deletes an existing allergy
- Requires `CanModifyMedicalAttributes` policy
- Returns 204 No Content on success

### Chronic Diseases

#### List Chronic Diseases
**Endpoint**: `GET /patients/{patientId}/chronic-diseases`

- Returns all chronic diseases for a specific patient
- Requires `CanViewMedicalAttributes` policy

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "name": "Diabetes Type 2",
  "diagnosisDate": "2020-01-15T00:00:00Z",
  "notes": "Controlled with medication"
}
```

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "notes": "Updated notes"
}
```

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy
- Returns 204 No Content on success

### Medications

#### List Medications
**Endpoint**: `GET /patients/{patientId}/medications`

- Returns all medications for a specific patient
- Requires `CanViewMedicalAttributes` policy

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy

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

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "dosage": "200mg daily",
  "endDate": "2024-12-31T00:00:00Z",
  "notes": "Updated notes"
}
```

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy
- Returns 204 No Content on success

### Surgeries

#### List Surgeries
**Endpoint**: `GET /patients/{patientId}/surgeries`

- Returns all surgeries for a specific patient
- Requires `CanViewMedicalAttributes` policy

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "name": "Appendectomy",
  "date": "2015-06-10T00:00:00Z",
  "surgeon": "Dr. Smith",
  "notes": "Laparoscopic procedure"
}
```

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy

**Request**:
```json
{
  "surgeon": "Dr. Johnson",
  "notes": "Updated notes"
}
```

**Response**:
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
- Requires `CanModifyMedicalAttributes` policy
- Returns 204 No Content on success

### Blood Type

#### Update Blood Type
**Endpoint**: `PUT /patients/{patientId}/blood-type`

- Updates the blood type for a specific patient
- Requires `CanModifyMedicalAttributes` policy
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

**Response**:
```json
{
  "patientId": "550e8400-e29b-41d4-a716-446655440001",
  "bloodType": "A+"
}
```

**Note**: When blood type is cleared, `bloodType` will be `null` in the response.

## Admin Features

All admin endpoints require `RequireAdmin` policy (SystemAdmin only).

### User Management

#### List Users
**Endpoint**: `GET /admin/users`

- Paginated list of all users
- Optional filtering by role and active status
- Supports pagination with `pageNumber` and `pageSize` query parameters
- Includes deactivated users (admin override)
- **Note**: All SystemAdmin users can view and retrieve SystemAdmin users in the list. Filtering by SystemAdmin role requires `CanManageAdmins` policy (Super Admin only).

**Query Parameters**:
- `pageNumber` (optional, default: 1, minimum: 1)
- `pageSize` (optional, default: 10, minimum: 1, maximum: 100)
- `role` (optional): Filter by user role (Doctor, HealthcareStaff, LabUser, ImagingUser, Patient, SystemAdmin)
- `isActive` (optional): Filter by active status (true, false)

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
**Endpoint**: `GET /admin/users/{id}`

- Returns detailed user information
- Includes role-specific details if applicable
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
- **Note**: Creating SystemAdmin users requires `CanManageAdmins` policy (Super Admin only)

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
- **Note**: Updating SystemAdmin users (including CorporateId and Department) requires `CanManageAdmins` policy (Super Admin only).

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
- Returns 204 No Content on success
- **Note**: Deactivating SystemAdmin users requires `CanManageAdmins` policy (Super Admin only).

#### Change Password
**Endpoint**: `PUT /admin/users/{id}/password`

- Changes user password (admin can change without current password)
- Validates password strength
- Returns 204 No Content on success
- **Note**: Changing SystemAdmin passwords requires `CanManageAdmins` policy (Super Admin only).

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
- Requires `CanModifyRecords` policy
- Maximum file size: 10MB
- Allowed content types: PDF, images (JPEG, PNG), documents (DOC, DOCX, XLS, XLSX)

**Request**: Multipart form data
- `file`: IFormFile (required)

**Response**:
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
- Requires `CanModifyRecords` policy

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

**Response**:
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
- Requires `CanViewRecords` policy

**Query Parameters**:
- `pageNumber` (optional, default: 1, minimum: 1)
- `pageSize` (optional, default: 10, minimum: 1, maximum: 100)
- `patientId` (optional): Filter by patient ID
- `recordType` (optional): Filter by record type
- `dateFrom` (optional): Filter records from this date
- `dateTo` (optional): Filter records to this date

**Response**:
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
- Only the practitioner can view the record
- Requires `CanViewRecords` policy

**Response**:
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
- Only the practitioner can modify the record
- Attachments cannot be modified through this endpoint (use add/remove attachment endpoints)
- Requires `CanModifyRecords` policy

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
- Only the creator can delete the record
- Requires `CanModifyRecords` policy
- Returns 204 No Content on success

### Add Attachment to Record

**Endpoint**: `POST /api/records/{recordId}/attachments`

- Uploads a file attachment and adds it to an existing medical record
- Only the practitioner of the record can add attachments
- Requires `CanModifyRecords` policy
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
- Only the creator of the record can remove attachments
- The file itself is not deleted from storage (only the reference is removed)
- Requires `CanModifyRecords` policy
- Returns 204 No Content on success

### Download Attachment

**Endpoint**: `GET /api/records/{recordId}/attachments/{attachmentId}/download`

- Downloads a file attachment from a medical record
- Practitioners can download attachments from records they created
- Patients can download attachments from their own records
- Requires `CanViewRecords` or `RequirePatient` policy

**Response**: File stream with appropriate Content-Type header

### Patient View Records

**Endpoint**: `GET /api/patients/self/records`

- Lists all medical records for the authenticated patient
- Supports filtering by record type and date range
- Requires `RequirePatient` policy

**Query Parameters**:
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
  ]
}
```

**Endpoint**: `GET /api/patients/self/records/{recordId}`

- Gets a specific medical record for the authenticated patient
- Requires `RequirePatient` policy
- Returns 403 if record does not belong to patient

**Response**: Same as Get Medical Record

### Business Rules

- **Only Practitioner Can Modify**: Only the practitioner who created a record can modify or delete it
- **Only Practitioner Can Manage Attachments**: Only the practitioner can add or remove attachments from a record
- **Attachments Can Be Removed**: Attachments can be removed from records by the practitioner, but the file itself remains in storage for audit purposes
- **Soft Delete**: Records are soft-deleted (IsActive = false), attachment metadata is deleted, but files are kept
- **Multiple Attachments**: Records can have multiple attachments (up to 10 per record, configurable)
- **File Storage**: Files are stored separately from database records; removing an attachment from a record does not delete the file

## Future Features

### Planned Features

- **Encounters**: Track patient-provider interactions (requires domain events infrastructure)
- **Action Logging**: Comprehensive audit trail
- **Patient Reports**: Generate patient health reports
- **Practitioner Endpoints**: Additional practitioner-specific endpoints
- **Lab Results**: Enhanced laboratory test result management
- **Imaging Studies**: Enhanced imaging study record management

See [ImplementationPlan.md](ImplementationPlan.md) for detailed roadmap.
