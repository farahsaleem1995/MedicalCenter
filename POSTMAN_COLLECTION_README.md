# Medical Center API - Postman Collection

This Postman collection provides a complete set of pre-configured API requests for testing the Medical Center API with real, valid test data.

## Files

- **MedicalCenter_API.postman_collection.json** - Main Postman collection with all endpoints
- **MedicalCenter_API.postman_environment.json** - Environment variables for easy configuration

## Setup Instructions

### 1. Import into Postman

1. Open Postman
2. Click **Import** button (top left)
3. Select both JSON files:
   - `MedicalCenter_API.postman_collection.json`
   - `MedicalCenter_API.postman_environment.json`
4. Both will be imported automatically

### 2. Configure Environment

1. Select the **"Medical Center API - Local"** environment from the environment dropdown (top right)
2. Update `baseUrl` if your API runs on a different port:
   - Default: `https://localhost:7000`
   - Change if needed (e.g., `http://localhost:5000`)

### 3. Authentication Flow

The collection uses Bearer token authentication. Tokens are automatically saved to environment variables after successful authentication.

**Recommended Testing Flow:**

1. **Register Patient** (`Auth > Register Patient`)
   - Creates a new patient account
   - Automatically saves `accessToken`, `refreshToken`, and `patientId` to environment
   - Pre-filled with: `john.doe@example.com` / `SecurePass123!`

2. **Login** (`Auth > Login`)
   - Use existing credentials
   - Updates tokens in environment

3. **Refresh Token** (`Auth > Refresh Token`)
   - Refresh expired access tokens
   - Uses saved `refreshToken` from environment

## Collection Structure

### Auth
- **Register Patient** - Self-registration for patients
- **Login** - Authenticate and receive tokens
- **Refresh Token** - Refresh access token

### Admin (Requires SystemAdmin role)
- **Create User** - Create Doctor, HealthcareStaff, LabUser, or ImagingUser
- **List Users** - List users with optional filters
- **Get User** - Get user details by ID
- **Update User** - Update user information
- **Delete User** - Soft delete (deactivate) user
- **Change Password** - Admin change user password

### Patients
- **Get Self** - Get authenticated patient's information
- **Get Self Medical Attributes** - Get all medical attributes

### Allergies
- **Create Allergy** - Add new allergy
- **List Allergies** - Get all allergies for patient
- **Update Allergy** - Update existing allergy
- **Delete Allergy** - Remove allergy

### Chronic Diseases
- **Create Chronic Disease** - Add chronic disease record
- **List Chronic Diseases** - Get all chronic diseases
- **Update Chronic Disease** - Update chronic disease
- **Delete Chronic Disease** - Remove chronic disease

### Medications
- **Create Medication** - Add medication record
- **List Medications** - Get all medications
- **Update Medication** - Update medication
- **Delete Medication** - Remove medication

### Surgeries
- **Create Surgery** - Add surgery record
- **List Surgeries** - Get all surgeries
- **Update Surgery** - Update surgery
- **Delete Surgery** - Remove surgery

## Pre-populated Test Data

All requests include realistic, valid test data:

- **Passwords**: All meet Identity requirements (8+ chars, digit, lowercase, uppercase, non-alphanumeric)
- **Emails**: Valid email format
- **Dates**: Valid ISO 8601 format dates
- **Names**: Realistic medical names and terms

## Automatic Token Management

The collection includes **Test Scripts** that automatically:
- Save `accessToken` and `refreshToken` after login/register/refresh
- Save created entity IDs (patientId, doctorId, allergyId, etc.) for use in subsequent requests

## Environment Variables

The collection uses these environment variables:

| Variable | Description | Auto-populated |
|----------|-------------|----------------|
| `baseUrl` | API base URL | No (manual) |
| `accessToken` | JWT access token | Yes |
| `refreshToken` | Refresh token | Yes |
| `patientId` | Patient user ID | Yes |
| `patientEmail` | Patient email | No (default value) |
| `doctorId` | Doctor user ID | Yes (after create) |
| `healthcareStaffId` | Healthcare staff ID | Yes (after create) |
| `labUserId` | Lab user ID | Yes (after create) |
| `imagingUserId` | Imaging user ID | Yes (after create) |
| `allergyId` | Allergy record ID | Yes (after create) |
| `chronicDiseaseId` | Chronic disease ID | Yes (after create) |
| `medicationId` | Medication ID | Yes (after create) |
| `surgeryId` | Surgery ID | Yes (after create) |

## Testing Tips

1. **Start with Auth**: Always register/login first to get tokens
2. **Use Environment Variables**: IDs are automatically saved - no need to copy/paste
3. **Check Responses**: Successful requests (200) automatically save IDs to environment
4. **Update baseUrl**: If your API runs on a different port, update the environment variable
5. **Admin Endpoints**: Require SystemAdmin role - ensure you're logged in as admin

## Password Requirements

All passwords in the collection meet Identity requirements:
- Minimum 8 characters
- At least one digit (0-9)
- At least one lowercase letter (a-z)
- At least one uppercase letter (A-Z)
- At least one non-alphanumeric character (!@#$%^&*)

Example passwords used:
- `SecurePass123!`
- `DoctorPass123!`
- `NursePass123!`
- `LabPass123!`
- `ImagingPass123!`

## Troubleshooting

### 401 Unauthorized
- Ensure you've logged in and token is saved
- Check that `accessToken` is set in environment
- Try refreshing the token

### 403 Forbidden
- Admin endpoints require SystemAdmin role
- Medical attribute endpoints require appropriate permissions

### 404 Not Found
- Check that IDs in URLs match created entities
- Ensure entities were created successfully

### 409 Conflict
- Email already exists - use a different email
- Update the email in the request body

### Connection Errors
- Verify API is running
- Check `baseUrl` in environment matches your API URL
- Ensure HTTPS certificate is trusted (or use HTTP if configured)

## Notes

- All dates use ISO 8601 format: `YYYY-MM-DDTHH:mm:ssZ`
- UserRole enum values: 1=SystemAdmin, 2=Doctor, 3=HealthcareStaff, 4=LabUser, 5=ImagingUser, 6=Patient
- All DELETE operations are soft deletes (deactivation)
- Medical attributes require patient ID in URL path

