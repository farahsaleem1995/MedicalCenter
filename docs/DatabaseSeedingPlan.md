# Database Seeding Plan with Bogus

## Overview

This document outlines the plan and implementation status for database seeding using [Bogus](https://github.com/bchavez/Bogus), a C# fake data generator library. The seeding system generates realistic test and presentation data for all domain aggregates while respecting DDD principles and the existing architecture.

**Status**: ✅ **Implemented** - All core functionality is complete and ready for use.

## Goals

1. **Testing**: Generate realistic test data for unit and integration tests
2. **Presentation**: Create demo data for showcasing the application
3. **Development**: Provide seed data for local development environments
4. **Maintainability**: Keep seeding logic organized and extensible

## Architecture Considerations

### Current Seeding Approach

The application currently uses **EF Core `HasData()`** for seeding initial data (roles, system admin) during migrations. This approach:
- ✅ Ensures data exists after migrations
- ✅ Works well for small, deterministic seed data
- ❌ Not ideal for large volumes of fake data
- ❌ Requires migrations for data changes

### Recommended Hybrid Approach

We'll use a **two-tier seeding strategy**:

1. **Migration-Based Seeding** (Existing)
   - Essential data: Roles, System Admin
   - Deterministic, always present
   - Uses `HasData()` in `ModelBuilderExtensions`

2. **Runtime Seeding** (New - Using Bogus)
   - Test/demo data: Patients, Doctors, Medical Records, etc.
   - Generated on-demand or via command/endpoint
   - Can be cleared and regenerated
   - Uses domain factory methods (respects DDD)

### Why Runtime Seeding for Fake Data?

- **Flexibility**: Can generate different volumes (10 vs 1000 patients)
- **Freshness**: Generate new data for each test run
- **Control**: Can seed/clear data without migrations
- **Realism**: Bogus generates more realistic, varied data
- **Performance**: No migration overhead for large datasets

## Domain-Driven Design Considerations

### Respecting Domain Boundaries

1. **Use Domain Factory Methods**: All entities must be created through their `Create()` methods
   - `Patient.Create()`, `Doctor.Create()`, `MedicalRecord.Create()`, etc.
   - Never bypass domain constructors or use reflection to set private properties

2. **Respect Aggregate Invariants**: 
   - Medical attributes (Allergies, Medications, etc.) must be added via Patient aggregate methods
   - Medical records must reference valid Patient and Practitioner IDs

3. **Domain Events**: Factory methods raise domain events - seeders should handle this appropriately
   - Domain events will be dispatched via `DomainEventDispatcherInterceptor`
   - Seeders don't need to manually dispatch events

4. **Value Objects**: Use proper value object creation
   - `BloodType.Create(BloodABO.A, BloodRh.Positive)`
   - `Practitioner.Create(fullName, email, role)`

5. **Audit Properties**: 
   - `CreatedAt` and `UpdatedAt` are set by `AuditableEntityInterceptor`
   - Seeders should set `CreatedAt` explicitly for realistic historical data
   - Consider using `IDateTimeProvider` for consistent time handling

## Seeder Organization

### Proposed Structure

```
src/MedicalCenter.Infrastructure/Data/Seeders/
├── ModelBuilderExtensions.cs          # Migration-based seeding (existing)
├── RoleSeeder.cs                       # Migration-based (existing)
├── SystemAdminSeeder.cs                # Migration-based (existing)
├── PasswordHashGenerator.cs             # Utility (existing)
│
├── Runtime/                            # New: Runtime seeding
│   ├── IDatabaseSeeder.cs              # Interface for all runtime seeders
│   ├── DatabaseSeeder.cs               # Main orchestrator
│   │
│   ├── PatientSeeder.cs                # Patient aggregate seeder
│   ├── DoctorSeeder.cs                 # Doctor aggregate seeder
│   ├── HealthcareStaffSeeder.cs         # HealthcareStaff aggregate seeder
│   ├── LaboratorySeeder.cs              # Laboratory aggregate seeder
│   ├── ImagingCenterSeeder.cs          # ImagingCenter aggregate seeder
│   ├── MedicalRecordSeeder.cs           # MedicalRecord aggregate seeder
│   │
│   └── Fakers/                          # Bogus faker configurations
│       ├── PatientFaker.cs              # Bogus rules for Patient
│       ├── DoctorFaker.cs              # Bogus rules for Doctor
│       ├── HealthcareStaffFaker.cs     # Bogus rules for HealthcareStaff
│       ├── LaboratoryFaker.cs           # Bogus rules for Laboratory
│       ├── ImagingCenterFaker.cs        # Bogus rules for ImagingCenter
│       ├── MedicalRecordFaker.cs        # Bogus rules for MedicalRecord
│       ├── MedicalAttributeFaker.cs     # Shared fakers for medical attributes
│       └── PractitionerFaker.cs          # Helper for Practitioner value object
```

## Implementation Strategy

### Phase 1: Infrastructure Setup

#### 1.1 Add Bogus NuGet Package

```xml
<PackageReference Include="Bogus" Version="35.6.5" />
```

Add to `MedicalCenter.Infrastructure.csproj`

#### 1.2 Create Seeder Interface

```csharp
public interface IDatabaseSeeder
{
    Task SeedAsync(MedicalCenterDbContext context, int count, CancellationToken cancellationToken = default);
    string EntityName { get; }
}
```

#### 1.3 Create Main Database Seeder Orchestrator

```csharp
public class DatabaseSeeder
{
    private readonly MedicalCenterDbContext _context;
    private readonly IEnumerable<IDatabaseSeeder> _seeders;
    private readonly ILogger<DatabaseSeeder> _logger;

    public async Task SeedAllAsync(SeedingOptions options, CancellationToken cancellationToken = default)
    {
        // Seed in dependency order:
        // 1. Practitioners (Doctors, HealthcareStaff, Labs, Imaging)
        // 2. Patients
        // 3. Medical Records (depends on Patients and Practitioners)
    }
}
```

### Phase 2: Practitioner Seeders

#### 2.1 Doctor Seeder

**Responsibilities:**
- Generate `Doctor` entities using `Doctor.Create()`
- Create corresponding `ApplicationUser` for Identity
- Create `ApplicationUserRole` relationships
- Generate realistic license numbers and specialties

**Bogus Configuration:**
- Use `faker.Name.FullName()` for names
- Use `faker.Internet.Email()` for emails
- Use `faker.Random.Replace("MD-####")` for license numbers
- Use medical specialties list: "Cardiology", "Pediatrics", "Orthopedics", etc.

**Estimated Properties:**
- FullName, Email, LicenseNumber, Specialty

#### 2.2 HealthcareStaff Seeder

**Responsibilities:**
- Generate `HealthcareStaff` entities
- Create Identity users and roles
- Generate organization names and departments

**Bogus Configuration:**
- Organization names: "City General Hospital", "Regional Medical Center", etc.
- Departments: "Emergency", "ICU", "Surgery", "Pediatrics", etc.

#### 2.3 Laboratory Seeder

**Responsibilities:**
- Generate `Laboratory` entities
- Create Identity users and roles
- Generate lab names and license numbers

**Bogus Configuration:**
- Lab names: "Advanced Diagnostics Lab", "Clinical Pathology Lab", etc.
- License numbers: "LAB-####"

#### 2.4 ImagingCenter Seeder

**Responsibilities:**
- Generate `ImagingCenter` entities
- Create Identity users and roles
- Generate center names and license numbers

**Bogus Configuration:**
- Center names: "Radiology Imaging Center", "MRI Diagnostic Center", etc.
- License numbers: "IMG-####"

### Phase 3: Patient Seeder

#### 3.1 Patient Seeder

**Responsibilities:**
- Generate `Patient` entities using `Patient.Create()`
- Create Identity users and roles
- Generate medical attributes (Allergies, ChronicDiseases, Medications, Surgeries)
- Generate blood types
- Generate realistic national IDs and dates of birth

**Bogus Configuration:**
- National IDs: Always 11 digit numeric value (e.g., `faker.Random.Replace("###########")` or `faker.Random.Long(10000000000, 99999999999).ToString()`)
- Date of Birth: `faker.Date.Between(DateTime.Now.AddYears(-80), DateTime.Now.AddYears(-18))`
- Blood Type: Random distribution of ABO and Rh combinations

**Medical Attributes Generation:**
- **Allergies**: 0-5 per patient
  - Common allergies: "Peanuts", "Penicillin", "Latex", "Shellfish", etc.
  - Severity: "Mild", "Moderate", "Severe"
  
- **Chronic Diseases**: 0-3 per patient
  - Common diseases: "Type 2 Diabetes", "Hypertension", "Asthma", "Arthritis", etc.
  - Diagnosis dates: In the past, before today
  
- **Medications**: 0-6 per patient
  - Common medications: "Metformin", "Lisinopril", "Atorvastatin", etc.
  - Dosage: "10mg daily", "5mg twice daily", etc.
  - Start dates: In the past
  - End dates: Some ongoing (null), some completed
  
- **Surgeries**: 0-4 per patient
  - Common surgeries: "Appendectomy", "Knee Replacement", "Cataract Surgery", etc.
  - Dates: In the past
  - Surgeons: Generate doctor names

**Implementation Notes:**
- Use Patient aggregate methods: `patient.AddAllergy()`, `patient.AddMedication()`, etc.
- Respect domain invariants (e.g., medication end date must be after start date)

### Phase 4: Medical Record Seeder

#### 4.1 Medical Record Seeder

**Responsibilities:**
- Generate `MedicalRecord` entities using `MedicalRecord.Create()`
- Reference existing Patients and Practitioners
- Generate realistic medical content
- Distribute records across different record types

**Dependencies:**
- Requires seeded Patients
- Requires seeded Practitioners (Doctors, HealthcareStaff, Labs, Imaging)

**Bogus Configuration:**
- Record Types: Distribute across all `RecordType` enum values
- Titles: Type-specific titles
  - ConsultationNote: "Follow-up Consultation", "Initial Visit", etc.
  - LaboratoryResult: "Complete Blood Count", "Lipid Panel", etc.
  - ImagingReport: "Chest X-Ray Report", "MRI Brain Scan", etc.
  - Prescription: "Prescription for [Medication]", etc.
  
- Content: Generate realistic medical content using `faker.Lorem.Paragraphs()`
- Practitioner: Random selection from seeded practitioners
- Patient: Random selection from seeded patients
- CreatedAt: Vary dates to show historical records

**Record Distribution:**
- Each patient should have 2-10 medical records
- Each practitioner should have created 5-30 records
- Mix of record types per patient

### Phase 5: Integration

#### 5.1 Seeding Options Configuration

```csharp
public class SeedingOptions
{
    public int DoctorCount { get; set; } = 20;
    public int HealthcareStaffCount { get; set; } = 15;
    public int LaboratoryCount { get; set; } = 5;
    public int ImagingCenterCount { get; set; } = 5;
    public int PatientCount { get; set; } = 100;
    public int MedicalRecordsPerPatientMin { get; set; } = 2;
    public int MedicalRecordsPerPatientMax { get; set; } = 10;
    public bool ClearExistingData { get; set; } = false;
}
```

#### 5.2 Seeding Endpoint/Command

**Option A: API Endpoint (✅ Implemented)**
- Development-only endpoint: `POST /api/dev/seed`
- Protected by environment check (only available in Development)
- Accepts `SeedingOptions` as request body
- Returns seeding summary as downloadable markdown file

**Option B: EF Core Migration Command**
- Create a custom migration command
- Run via `dotnet ef database seed`

**Option C: Console Command/Background Service**
- Create a one-time seeding service
- Run on application startup (development only)

**Implementation**: Option A (API Endpoint) has been implemented for flexibility and control.

#### 5.3 Update ModelBuilderExtensions

Keep existing migration-based seeders. Add comment indicating runtime seeders are separate.

## Data Generation Guidelines

### Realistic Data Principles

1. **Names**: Use locale-appropriate names (Bogus supports multiple locales)
2. **Emails**: Match email format to names (e.g., `firstname.lastname@example.com`)
3. **Dates**: Use realistic date ranges
   - Patients: 18-80 years old
   - Medical records: Last 5 years
   - Medications: Some ongoing, some completed
4. **Relationships**: Ensure referential integrity
   - Medical records reference existing patients and practitioners
5. **Variety**: Use Bogus to generate varied, realistic data
   - Not all patients should have the same number of allergies
   - Mix of active and inactive records
   - Variety in medical specialties

### Bogus Locale Configuration

Consider using locale-specific Bogus instances for more realistic data:
- `new Faker("en_US")` for US-style data
- `new Faker("ar")` for Arabic names (if applicable)
- Configure based on application requirements

## Testing Strategy

### Unit Tests for Seeders

1. **Test Faker Configurations**
   - Verify generated data meets domain constraints
   - Test edge cases (empty collections, null values)

2. **Test Seeder Logic**
   - Verify correct use of domain factory methods
   - Verify relationships are established correctly
   - Verify no domain invariants are violated

### Integration Tests

1. **Database Seeding Integration Tests**
   - Seed database with known counts
   - Verify all entities are created
   - Verify relationships are correct
   - Verify domain events are raised (if applicable)

2. **Test Data Cleanup**
   - Ensure seeders can run multiple times
   - Test clearing existing data option

## Seeding Summary Document

### Purpose

After seeding completes, generate a summary document that provides developers and testers with:
- Quick access to test user credentials
- Overview of what data was created
- Reference for testing different scenarios

### Document Format

The summary document should be generated in a human-readable format (JSON or Markdown) and include:

#### 1. User Credentials Section

For each created user, list:
- **Email**: User's email address
- **Password**: Test password (use consistent pattern like `Test@123!` for all seeded users, or generate and store unique passwords)
- **Role**: User role (Patient, Doctor, HealthcareStaff, etc.)
- **Full Name**: User's full name
- **User ID**: Guid for API testing
- **Role-Specific Info**:
  - Doctors: License Number, Specialty
  - HealthcareStaff: Organization Name, Department
  - Laboratories: Lab Name, License Number
  - ImagingCenters: Center Name, License Number
  - Patients: National ID, Date of Birth
  - SystemAdmins: Corporate ID, Department

#### 2. Data Summary Section

Statistics and overview:
- **Entity Counts**: Total number of each entity type created
  - Patients: X
  - Doctors: X
  - HealthcareStaff: X
  - Laboratories: X
  - ImagingCenters: X
  - Medical Records: X
- **Medical Attributes Summary**:
  - Total Allergies: X
  - Total Chronic Diseases: X
  - Total Medications: X
  - Total Surgeries: X
- **Medical Records Distribution**:
  - By Record Type (ConsultationNote: X, LaboratoryResult: X, etc.)
  - Average records per patient
- **Date Ranges**:
  - Patient date of birth range
  - Medical record creation date range
- **Sample Data**:
  - Sample Patient IDs for testing
  - Sample Doctor IDs for testing
  - Sample Medical Record IDs

#### 3. Metadata

- **Seeding Timestamp**: When the seeding was performed
- **Seeding Options**: The options used (counts, etc.)
- **Seeder Version**: Version or identifier of the seeder

### Implementation Notes

- Generate the summary document after all seeding operations complete
- **Return as file download**: The summary is returned as a downloadable markdown file (`SeedingSummary.md`) in the API response, not saved to disk
- Use a consistent password for all seeded users to simplify testing (e.g., `Test@123!`)
- The endpoint returns the summary as a stream with content type `text/markdown`
- Include a note that passwords are for development/testing only

### Example Summary Structure

```json
{
  "seedingTimestamp": "2025-01-15T10:30:00Z",
  "options": {
    "patientCount": 100,
    "doctorCount": 20,
    ...
  },
  "users": [
    {
      "email": "john.doe@example.com",
      "password": "Test@123!",
      "role": "Patient",
      "fullName": "John Doe",
      "userId": "guid-here",
      "nationalId": "12345678901",
      "dateOfBirth": "1985-05-15"
    },
    ...
  ],
  "summary": {
    "patients": 100,
    "doctors": 20,
    "medicalRecords": 450,
    "totalAllergies": 250,
    ...
  }
}
```

## Implementation Steps

### Step 1: Add Bogus Package
- [x] Add `Bogus` NuGet package to `MedicalCenter.Infrastructure.csproj`
- [x] Restore packages

### Step 2: Create Seeder Infrastructure
- [x] Create `IDatabaseSeeder` interface
- [x] Create `DatabaseSeeder` orchestrator class
- [x] Create `SeedingOptions` class
- [x] Create `Runtime/` folder structure
- [x] Create `SeedingSummaryGenerator` class

### Step 3: Implement Practitioner Seeders
- [x] Create `DoctorFaker` with Bogus rules
- [x] Implement `DoctorSeeder` using domain factory methods
- [x] Create Identity users and roles for doctors
- [x] Repeat for `HealthcareStaffSeeder`, `LaboratorySeeder`, `ImagingCenterSeeder`

### Step 4: Implement Patient Seeder
- [x] Create `PatientFaker` with Bogus rules
- [x] Create `MedicalAttributeFaker` for allergies, diseases, medications, surgeries
- [x] Implement `PatientSeeder` using domain factory methods
- [x] Generate medical attributes using Patient aggregate methods
- [x] Create Identity users and roles for patients

### Step 5: Implement Medical Record Seeder
- [x] Create `MedicalRecordFaker` with Bogus rules
- [x] Implement `MedicalRecordSeeder` using domain factory methods
- [x] Ensure proper patient and practitioner references
- [x] Distribute records by practitioner role and record type

### Step 6: Create Seeding Endpoint
- [x] Create development-only seeding endpoint
- [x] Add authorization/environment checks
- [x] Accept `SeedingOptions` and call `DatabaseSeeder`
- [x] Return summary document as file download stream

### Step 7: Create Seeding Summary Document
- [x] Implement summary document generation after seeding completes
- [x] Generate summary file `SeedingSummary.md` containing:
  - **User Credentials Section**: List all created users with their:
    - Email address
    - Password (for seeded users, use a default test password like `Test@123!` or generate and store)
    - Role
    - Full name
    - Additional role-specific information (e.g., license number for doctors, organization for healthcare staff)
  - **Data Summary Section**: Statistics about seeded data:
    - Total count per entity type (Patients, Doctors, HealthcareStaff, etc.)
    - Medical attributes summary (total allergies, medications, etc.)
    - Medical records count and distribution by type
    - Date range of generated data
  - **Quick Reference**: Sample user IDs for testing different roles
- [x] Return summary as file download stream (not saved to disk)
- [x] Include timestamp of when seeding was performed
- [x] Note: Passwords use consistent test password pattern (`Test@123!` by default)

### Step 8: Testing
- [ ] Write unit tests for fakers
- [ ] Write integration tests for seeders
- [ ] Test with various data volumes
- [ ] Verify domain events are handled correctly

### Step 9: Documentation
- [x] Document seeding endpoint usage
- [x] Add examples of seeding options
- [x] Document how to clear and reseed data
- [x] Document that summary is returned as file download

## Example Usage

### Via API Endpoint

```http
POST /api/dev/seed
Content-Type: application/json

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

**Response**: 
- Status: `200 OK`
- Content-Type: `text/markdown`
- Content-Disposition: `attachment; filename="SeedingSummary.md"`
- Body: Markdown file stream containing user credentials and data summary

The response is a downloadable file (`SeedingSummary.md`) that includes:
- All user credentials (email, password, role, additional info)
- Data statistics (counts, distributions, date ranges)
- Sample IDs for testing
- Seeding timestamp and options used

### Via Code

```csharp
var seeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
var summaryGenerator = serviceProvider.GetRequiredService<SeedingSummaryGenerator>();
var options = new SeedingOptions
{
    PatientCount = 100,
    DoctorCount = 20,
    // ... other options
};

await seeder.SeedAllAsync(options);

// Generate summary as stream
Stream summaryStream = await summaryGenerator.GenerateSummaryAsync(options);
// Use stream as needed (e.g., save to file, return as response, etc.)
```

## Security Considerations

1. **Development Only**: Seeding endpoints should only be available in Development environment
2. **Authorization**: Add special authorization requirement for seeding endpoints
3. **Data Privacy**: Generated data should be clearly fake (use Bogus' fake data generators)
4. **Clear Warnings**: Add warnings when clearing existing data

## Performance Considerations

1. **Batch Operations**: Use `AddRange()` for bulk inserts
2. **SaveChanges**: Call `SaveChangesAsync()` after each aggregate type (not after each entity)
3. **Transaction**: Consider wrapping entire seeding in a transaction
4. **Progress Tracking**: For large datasets, consider progress reporting

## Future Enhancements

1. **Custom Data Sets**: Allow importing custom data files (CSV, JSON)
2. **Template-Based Seeding**: Predefined templates (e.g., "Small Demo", "Large Test Dataset")
3. **Incremental Seeding**: Add more data without clearing existing
4. **Data Export**: Export seeded data for sharing between environments
5. **Realistic Medical Data**: Use medical terminology libraries for more realistic content

## References

- [Bogus GitHub Repository](https://github.com/bchavez/Bogus)
- [Bogus Documentation](https://github.com/bchavez/Bogus/wiki)
- Existing Seeders: `SystemAdminSeeder.cs`, `RoleSeeder.cs`
- Domain Entities: See `src/MedicalCenter.Core/Aggregates/`

## Implementation Status

### ✅ Completed

- Bogus package integrated
- All seeder infrastructure created
- All fakers implemented (Doctor, HealthcareStaff, Laboratory, ImagingCenter, Patient, MedicalRecord, MedicalAttributes)
- All seeders implemented with Identity integration
- Seeding endpoint created (`POST /api/dev/seed`)
- Summary generator implemented (returns as file stream)
- Services registered in DI container
- All code compiles and builds successfully

### 📝 Notes

- All seeders use domain factory methods - never bypass domain logic
- Respect aggregate boundaries - medical attributes belong to Patient aggregate
- Domain events are automatically dispatched via interceptors
- Audit properties (`CreatedAt`, `UpdatedAt`) are set by interceptors, but seeders may set `CreatedAt` for historical data
- Summary document is returned as a downloadable file stream, not saved to disk
- Default password for all seeded users: `Test@123!` (configurable via request)

