# Endpoint Authorization Summary

This document lists all API endpoints and their authorization policies to ensure proper security.

## Authentication Endpoints (Public)

| Endpoint | Method | Authorization | Notes |
|----------|--------|---------------|-------|
| `/patients` | POST | `AllowAnonymous` | Patient self-registration |
| `/auth/login` | POST | `AllowAnonymous` | User login |
| `/auth/refresh-token` | POST | `AllowAnonymous` | Token refresh |

## Patient Self Endpoints

| Endpoint | Method | Authorization | Notes |
|----------|--------|---------------|-------|
| `/patients/self` | GET | `RequirePatient` | Get own patient data |
| `/patients/self/medical-attributes` | GET | `RequirePatient` | Get own medical attributes |

## Admin Endpoints

| Endpoint | Method | Authorization | Notes |
|----------|--------|---------------|-------|
| `/users` | GET | `RequireAdmin` | List users (paginated) |
| `/users/{id}` | GET | `RequireAdmin` | Get user by ID |
| `/users` | POST | `RequireAdmin` | Create user |
| `/users/{id}` | PUT | `RequireAdmin` | Update user |
| `/users/{id}` | DELETE | `RequireAdmin` | Deactivate user |
| `/users/{id}/password` | PUT | `RequireAdmin` | Change user password |

## Medical Attributes Endpoints (Provider Access)

All endpoints under `/patients/{patientId}/` require `CanModifyMedicalAttributes` policy, which allows:
- Doctor
- HealthcareStaff
- SystemAdmin

### Allergies
| Endpoint | Method | Authorization |
|----------|--------|---------------|
| `/patients/{patientId}/allergies` | GET | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/allergies` | POST | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/allergies/{allergyId}` | PUT | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/allergies/{allergyId}` | DELETE | `CanModifyMedicalAttributes` |

### Chronic Diseases
| Endpoint | Method | Authorization |
|----------|--------|---------------|
| `/patients/{patientId}/chronic-diseases` | GET | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/chronic-diseases` | POST | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/chronic-diseases/{chronicDiseaseId}` | PUT | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/chronic-diseases/{chronicDiseaseId}` | DELETE | `CanModifyMedicalAttributes` |

### Medications
| Endpoint | Method | Authorization |
|----------|--------|---------------|
| `/patients/{patientId}/medications` | GET | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/medications` | POST | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/medications/{medicationId}` | PUT | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/medications/{medicationId}` | DELETE | `CanModifyMedicalAttributes` |

### Surgeries
| Endpoint | Method | Authorization |
|----------|--------|---------------|
| `/patients/{patientId}/surgeries` | GET | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/surgeries` | POST | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/surgeries/{surgeryId}` | PUT | `CanModifyMedicalAttributes` |
| `/patients/{patientId}/surgeries/{surgeryId}` | DELETE | `CanModifyMedicalAttributes` |

## Authorization Policies

### RequirePatient
- **Allowed Roles**: `Patient`
- **Use Case**: Patient accessing their own data

### RequireAdmin
- **Allowed Roles**: `SystemAdmin`
- **Use Case**: System administration operations

### CanModifyMedicalAttributes
- **Allowed Roles**: `Doctor`, `HealthcareStaff`, `SystemAdmin`
- **Use Case**: Providers modifying patient medical attributes

### RequireProvider
- **Allowed Roles**: `Doctor`, `HealthcareStaff`, `LabUser`, `ImagingUser`
- **Use Case**: General provider operations

### RequirePatientOrProvider
- **Allowed Roles**: `Patient`, `Doctor`, `HealthcareStaff`, `LabUser`, `ImagingUser`
- **Use Case**: Operations accessible to both patients and providers

## Security Notes

1. **Patient Self Endpoints**: Only patients can access `/patients/self/*` endpoints
2. **Medical Attributes**: Providers can modify medical attributes for any patient via `/patients/{patientId}/*` endpoints
3. **Admin Operations**: Only system administrators can manage users
4. **Public Endpoints**: Only authentication and registration endpoints are public

## Fixed Issues

- ✅ Added `RequirePatient` policy to `/patients/self` endpoint
- ✅ Added `RequirePatient` policy to `/patients/self/medical-attributes` endpoint

