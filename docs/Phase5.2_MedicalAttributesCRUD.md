# Phase 5.2: Medical Attributes CRUD Implementation Plan

## Overview

This document outlines the implementation plan for CRUD operations on medical attributes (Allergies, ChronicDiseases, Medications, Surgeries). These attributes are entities within the Patient aggregate, so all operations must be scoped to a specific patient.

## Domain Design Principles

### Aggregate Boundary
- **Patient** is the aggregate root
- **Allergy**, **ChronicDisease**, **Medication**, and **Surgery** are entities within the Patient aggregate
- These entities cannot exist independently - they always belong to a Patient
- All operations on these entities must go through the Patient aggregate root

### Domain Rules
1. Medical attributes are managed by authorized providers (Doctor, HealthcareStaff, SystemAdmin)
2. Patients can only view their own attributes (read-only)
3. Each attribute type is managed independently (not in bulk)
4. All operations require a `patientId` to identify the aggregate

## API Design

### Route Structure

All endpoints follow the pattern: `/medical-attributes/patients/{patientId}/{attributeType}/{attributeId?}`

This structure:
- Clearly shows the hierarchical relationship (attributes belong to patients)
- Enforces the aggregate boundary (patientId is always required)
- Follows RESTful principles
- Makes the API self-documenting

### Endpoint Specifications

#### 1. Allergies

**List Allergies**
```
GET /medical-attributes/patients/{patientId}/allergies
Authorization: Bearer {token}
Policy: CanModifyMedicalAttributes

Query Parameters:
- None (all allergies for the patient)

Response: 200 OK
{
  "allergies": [
    {
      "id": "guid",
      "patientId": "guid",
      "name": "string",
      "severity": "string?",
      "notes": "string?",
      "createdAt": "datetime",
      "updatedAt": "datetime?"
    }
  ],
  "totalCount": 0
}
```

**Get Allergy**
```
GET /medical-attributes/patients/{patientId}/allergies/{id}
Authorization: Bearer {token}
Policy: CanModifyMedicalAttributes

Response: 200 OK
{
  "id": "guid",
  "patientId": "guid",
  "name": "string",
  "severity": "string?",
  "notes": "string?",
  "createdAt": "datetime",
  "updatedAt": "datetime?"
}
```

**Create Allergy**
```
POST /medical-attributes/patients/{patientId}/allergies
Authorization: Bearer {token}
Policy: CanModifyMedicalAttributes

Request Body:
{
  "name": "string (required, max 200)",
  "severity": "string? (max 50)",
  "notes": "string? (max 1000)"
}

Response: 201 Created
{
  "id": "guid",
  "patientId": "guid",
  "name": "string",
  "severity": "string?",
  "notes": "string?",
  "createdAt": "datetime"
}
```

**Update Allergy**
```
PUT /medical-attributes/patients/{patientId}/allergies/{id}
Authorization: Bearer {token}
Policy: CanModifyMedicalAttributes

Request Body:
{
  "severity": "string? (max 50)",
  "notes": "string? (max 1000)"
}

Response: 200 OK
{
  "id": "guid",
  "patientId": "guid",
  "name": "string",
  "severity": "string?",
  "notes": "string?",
  "updatedAt": "datetime?"
}
```

**Delete Allergy**
```
DELETE /medical-attributes/patients/{patientId}/allergies/{id}
Authorization: Bearer {token}
Policy: CanModifyMedicalAttributes

Response: 204 No Content
```

#### 2. Chronic Diseases

Same pattern as Allergies:
- `GET /medical-attributes/patients/{patientId}/chronic-diseases`
- `GET /medical-attributes/patients/{patientId}/chronic-diseases/{id}`
- `POST /medical-attributes/patients/{patientId}/chronic-diseases`
- `PUT /medical-attributes/patients/{patientId}/chronic-diseases/{id}`
- `DELETE /medical-attributes/patients/{patientId}/chronic-diseases/{id}`

**Create Request:**
```json
{
  "name": "string (required, max 200)",
  "diagnosisDate": "datetime (required, <= today)",
  "notes": "string? (max 1000)"
}
```

**Update Request:**
```json
{
  "notes": "string? (max 1000)"
}
```

#### 3. Medications

Same pattern:
- `GET /medical-attributes/patients/{patientId}/medications`
- `GET /medical-attributes/patients/{patientId}/medications/{id}`
- `POST /medical-attributes/patients/{patientId}/medications`
- `PUT /medical-attributes/patients/{patientId}/medications/{id}`
- `DELETE /medical-attributes/patients/{patientId}/medications/{id}`

**Create Request:**
```json
{
  "name": "string (required, max 200)",
  "dosage": "string? (max 100)",
  "startDate": "datetime (required)",
  "endDate": "datetime? (>= startDate)",
  "notes": "string? (max 1000)"
}
```

**Update Request:**
```json
{
  "dosage": "string? (max 100)",
  "endDate": "datetime? (>= startDate)",
  "notes": "string? (max 1000)"
}
```

#### 4. Surgeries

Same pattern:
- `GET /medical-attributes/patients/{patientId}/surgeries`
- `GET /medical-attributes/patients/{patientId}/surgeries/{id}`
- `POST /medical-attributes/patients/{patientId}/surgeries`
- `PUT /medical-attributes/patients/{patientId}/surgeries/{id}`
- `DELETE /medical-attributes/patients/{patientId}/surgeries/{id}`

**Create Request:**
```json
{
  "name": "string (required, max 200)",
  "date": "datetime (required, <= today)",
  "surgeon": "string? (max 200)",
  "notes": "string? (max 1000)"
}
```

**Update Request:**
```json
{
  "surgeon": "string? (max 200)",
  "notes": "string? (max 1000)"
}
```

## Implementation Steps

### Step 1: Clean Up Existing Code
1. Remove the old `UpdatePatientMedicalAttributesEndpoint` (bulk update approach)
2. Remove incorrectly structured endpoints (those without patientId in route)
3. Keep `GetSelfPatientEndpoint` and `GetSelfMedicalAttributesEndpoint` in Patients group (for patient self-view)

### Step 2: Create Base Structure
1. Ensure `MedicalAttributesGroup` exists and is properly configured
2. Create subdirectories for each attribute type:
   - `Allergies/`
   - `ChronicDiseases/`
   - `Medications/`
   - `Surgeries/`

### Step 3: Implement Allergies Endpoints
For each operation (List, Get, Create, Update, Delete):

1. **Create Request DTO**
   - Include `patientId` as route parameter (from route, not body)
   - Include attribute-specific fields
   - Use FluentValidation for validation

2. **Create Response DTO**
   - Include all relevant attribute properties
   - Include audit fields (CreatedAt, UpdatedAt)

3. **Create Endpoint**
   - Use `PatientByIdSpecification` to load patient with attributes
   - Verify patient exists (404 if not found)
   - Perform operation through Patient aggregate methods
   - Use Unit of Work for transaction management
   - Return appropriate HTTP status codes

4. **Create Validator**
   - Validate patientId (from route)
   - Validate attribute-specific rules
   - Use domain validation messages

### Step 4: Implement ChronicDiseases Endpoints
Follow the same pattern as Allergies, adapting for ChronicDisease-specific fields and validation rules.

### Step 5: Implement Medications Endpoints
Follow the same pattern, with special attention to:
- Date range validation (endDate >= startDate)
- Handling both startDate and endDate in updates

### Step 6: Implement Surgeries Endpoints
Follow the same pattern, with special attention to:
- Date validation (surgery date cannot be in future)

### Step 7: Error Handling
All endpoints should handle:
- **404**: Patient not found, or Attribute not found
- **400**: Validation errors (domain rules violated)
- **401**: Unauthorized
- **403**: Forbidden (policy check failed)
- **500**: Unexpected errors (with rollback)

## Domain Considerations

### Aggregate Consistency
- All operations must load the Patient aggregate with its medical attributes
- Changes are made through Patient aggregate methods
- Unit of Work ensures transaction boundaries
- Changes are committed atomically

### Validation
- **Route validation**: `patientId` must be a valid GUID
- **Domain validation**: Enforced by Patient aggregate methods
- **Input validation**: FluentValidation for DTOs (length limits, required fields)
- **Business rules**: Enforced by domain entities (e.g., dates, ranges)

### Authorization
- All endpoints require `CanModifyMedicalAttributes` policy
- Policy checks happen at the endpoint level (FastEndpoints `Policies()`)
- No additional authorization logic needed in handlers (policy handles it)

## File Structure

```
src/MedicalCenter.WebApi/Endpoints/MedicalAttributes/
├── MedicalAttributesGroup.cs
├── Allergies/
│   ├── ListAllergiesEndpoint.cs
│   ├── ListAllergiesEndpoint.Request.cs
│   ├── ListAllergiesEndpoint.Response.cs
│   ├── GetAllergyEndpoint.cs
│   ├── GetAllergyEndpoint.Request.cs
│   ├── GetAllergyEndpoint.Response.cs
│   ├── CreateAllergyEndpoint.cs
│   ├── CreateAllergyEndpoint.Request.cs
│   ├── CreateAllergyEndpoint.Response.cs
│   ├── CreateAllergyEndpoint.Validator.cs
│   ├── UpdateAllergyEndpoint.cs
│   ├── UpdateAllergyEndpoint.Request.cs
│   ├── UpdateAllergyEndpoint.Response.cs
│   ├── UpdateAllergyEndpoint.Validator.cs
│   ├── DeleteAllergyEndpoint.cs
│   └── DeleteAllergyEndpoint.Request.cs
├── ChronicDiseases/
│   └── [Same structure as Allergies]
├── Medications/
│   └── [Same structure as Allergies]
└── Surgeries/
    └── [Same structure as Allergies]
```

## Testing Strategy

### Domain Tests (Classical School)
Focus on testing domain behavior, not implementation:

1. **Patient Aggregate Tests**
   - Verify adding/updating/removing attributes maintains aggregate invariants
   - Verify domain rules are enforced (date validations, etc.)
   - Test edge cases (null values, empty strings, etc.)

2. **Entity Tests**
   - Verify entity creation rules
   - Verify entity update rules
   - Verify entity validation

### Integration Tests (Future)
- Test full request/response cycle
- Test authorization policies
- Test transaction boundaries
- Test error scenarios

## Common Patterns

### Endpoint Handler Pattern

```csharp
public override async Task HandleAsync(Request req, CancellationToken ct)
{
    await unitOfWork.BeginTransactionAsync(ct);
    
    try
    {
        // 1. Load patient aggregate
        var specification = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(specification, ct);
        
        if (patient == null)
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            ThrowError("Patient not found", 404);
            return;
        }
        
        // 2. Perform operation through aggregate
        // (var result = patient.Add/Update/Remove...)
        
        // 3. Save changes
        await patientRepository.UpdateAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await unitOfWork.CommitTransactionAsync(ct);
        
        // 4. Return response
        Response = new Response { ... };
    }
    catch (ArgumentException ex)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        ThrowError(ex.Message, 400);
    }
    catch (InvalidOperationException ex)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        ThrowError(ex.Message, 404);
    }
    catch
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        throw;
    }
}
```

### Route Parameter Extraction

FastEndpoints automatically binds route parameters. For `{patientId}` in route:
- It will be available in the Request DTO
- Ensure Request DTO has `public Guid PatientId { get; set; }` property
- FastEndpoints will bind it from the route

## Migration from Current Implementation

1. **Delete** old bulk update endpoint
2. **Refactor** existing endpoints to include `patientId` in route
3. **Update** all request DTOs to include `patientId` from route
4. **Update** all handlers to load patient first, then operate on attributes
5. **Test** each endpoint individually

## Success Criteria

- [ ] All CRUD operations work for Allergies
- [ ] All CRUD operations work for ChronicDiseases
- [ ] All CRUD operations work for Medications
- [ ] All CRUD operations work for Surgeries
- [ ] All endpoints require `patientId` in route
- [ ] All endpoints use `CanModifyMedicalAttributes` policy
- [ ] All operations go through Patient aggregate
- [ ] All transactions are properly managed
- [ ] All validation rules are enforced
- [ ] All error scenarios are handled
- [ ] Old bulk update endpoint is removed
- [ ] Patient self-view endpoints remain in Patients group

## Notes

- This design respects aggregate boundaries
- All operations are scoped to a patient (enforced by route structure)
- Each attribute type is managed independently
- The API clearly expresses the domain model structure
- Authorization is handled at the policy level
- Domain rules are enforced by the aggregate root

