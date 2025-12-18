# Core (Domain) Layer Reorganization Plan

> **Status**: TEMPORARY - To be removed after implementation  
> **Created**: December 17, 2025  
> **Last Updated**: December 17, 2025  
> **Author**: Development Team  
> **Decisions**: ✅ All design decisions finalized - Ready for implementation

## Executive Summary

This document proposes a reorganization of the `MedicalCenter.Core` project to better express domain concepts and align with Domain-Driven Design (DDD) principles as outlined by Eric Evans. The current structure has grown organically and presents opportunities for improvement in terms of expressiveness, maintainability, and adherence to DDD tactical patterns.

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Identified Issues](#identified-issues)
3. [Proposed Structure](#proposed-structure)
4. [Detailed Changes](#detailed-changes)
5. [Migration Strategy](#migration-strategy)
6. [Impact Analysis](#impact-analysis)
7. [Benefits](#benefits)
8. [Design Decisions](#design-decisions-resolved)
9. [Architectural Notes](#architectural-notes)

---

## Current State Analysis

### Current Folder Structure

```
MedicalCenter.Core/
├── Aggregates/
│   ├── Doctor.cs
│   ├── HealthcareEntity.cs
│   ├── ImagingCenter.cs
│   ├── Laboratory.cs
│   ├── MedicalRecord/
│   │   ├── MedicalRecord.cs
│   │   ├── Practitioner.cs (value object)
│   │   ├── RecordType.cs (enum)
│   │   └── Specifications/
│   │       ├── MedicalRecordByIdSpecification.cs
│   │       └── MedicalRecordsByPatientSpecification.cs
│   └── Patient/
│       ├── Patient.cs
│       ├── Allergy.cs
│       ├── BloodABO.cs (enum)
│       ├── BloodRh.cs (enum)
│       ├── BloodType.cs (value object)
│       ├── ChronicDisease.cs
│       ├── Medication.cs
│       ├── Surgery.cs
│       └── Specifications/
│           ├── ActivePatientsSpecification.cs
│           ├── PatientByIdSpecification.cs
│           └── PatientsByNationalIdSpecification.cs
├── Common/
│   ├── Attachment.cs (value object)
│   ├── BaseEntity.cs
│   ├── Error.cs
│   ├── ErrorCodes.cs
│   ├── IAggregateRoot.cs
│   ├── IAuditableEntity.cs
│   ├── IRepository.cs
│   ├── IUnitOfWork.cs
│   ├── PaginatedList.cs
│   ├── PaginationMetadata.cs
│   ├── ProviderType.cs (enum)
│   ├── Result.cs
│   ├── ResultExtensions.cs
│   ├── User.cs (abstract base class)
│   ├── UserRole.cs (enum)
│   └── ValueObject.cs
├── Repositories/ (empty)
├── ValueObjects/ (empty)
└── Services/
    ├── IFileStorageService.cs
    ├── IIdentityService.cs
    ├── IMedicalRecordQueryService.cs
    ├── ITokenProvider.cs
    └── IUserQueryService.cs
```

### Domain Model Overview

The current domain model includes:

| Aggregate Root | Description | Child Entities/Value Objects |
|----------------|-------------|------------------------------|
| **Patient** | Patients receiving care | Allergy, ChronicDisease, Medication, Surgery, BloodType |
| **Doctor** | Medical doctors | - |
| **HealthcareStaff** | Hospital/clinic staff (renamed from HealthcareEntity) | - |
| **Laboratory** | Lab service providers | - |
| **ImagingCenter** | Imaging service providers | - |
| **MedicalRecord** | Medical records | Practitioner (VO), Attachment (VO) |

---

## Identified Issues

### 1. Inconsistent Aggregate Organization

**Problem**: Some aggregates have dedicated folders (`Patient/`, `MedicalRecord/`) while others are flat files (`Doctor.cs`, `HealthcareEntity.cs`).

**Impact**: 
- Inconsistent navigation experience for developers
- Makes it harder to understand aggregate boundaries
- Related types (enums, value objects) are scattered

### 2. Mixed Concerns in Common Folder

**Problem**: The `Common/` folder mixes multiple concerns:

| Category | Files |
|----------|-------|
| DDD Building Blocks | `BaseEntity.cs`, `IAggregateRoot.cs`, `ValueObject.cs`, `IAuditableEntity.cs` |
| Result Pattern | `Result.cs`, `Error.cs`, `ErrorCodes.cs`, `ResultExtensions.cs` |
| Infrastructure Contracts | `IRepository.cs`, `IUnitOfWork.cs` |
| Pagination | `PaginatedList.cs`, `PaginationMetadata.cs` |
| Domain Concepts | `User.cs`, `UserRole.cs`, `ProviderType.cs`, `Attachment.cs` |

**Impact**:
- Violates Single Responsibility Principle at the folder level
- Hard to distinguish between framework abstractions and domain concepts
- New developers may struggle to understand what belongs where

### 3. Empty/Unused Folders

**Problem**: `Repositories/` and `ValueObjects/` folders are empty.

**Impact**:
- Creates confusion about intended usage
- Suggests incomplete organization strategy

### 4. Misplaced Value Objects

**Problem**: `Attachment` value object is in `Common/` but is specifically used by `MedicalRecord` aggregate.

**Impact**:
- Weakens aggregate encapsulation
- Makes the relationship between Attachment and MedicalRecord less obvious

### 5. Practitioner Types Lack Grouping

**Problem**: Doctor, HealthcareEntity, Laboratory, and ImagingCenter are all practitioners that inherit from User but are scattered in the root of `Aggregates/`.

**Impact**:
- The conceptual relationship between these types is not reflected in the structure
- No clear "Practitioner" sub-domain organization

### 6. No Domain Events Infrastructure

**Problem**: No folder or base class for domain events exists.

**Impact**:
- Architecture documentation mentions domain events as a planned feature
- No clear home for event definitions when implemented

### 7. Services Folder Organization

**Observation**: The `Services/` folder contains various service interfaces:
- Infrastructure service interfaces (`IFileStorageService`, `ITokenProvider`)
- Query service interfaces (`IMedicalRecordQueryService`, `IUserQueryService`)
- Identity service interface (`IIdentityService`)

**Decision**: Keep flat structure intentionally.
- All service interfaces remain directly under `Services/`
- Avoids "infrastructure" terminology in the Core (domain) layer
- No CQRS separation needed - FastEndpoints handles Command/Query patterns at endpoint level
- Simple and pragmatic approach that works with current architecture

---

## Proposed Structure

### New Folder Structure

```
MedicalCenter.Core/
│
├── Abstractions/                           # DDD Building Blocks & Framework Contracts
│   ├── BaseEntity.cs
│   ├── IAggregateRoot.cs
│   ├── IAuditableEntity.cs
│   └── ValueObject.cs
│
├── Primitives/                             # Result Pattern, Error Handling & Pagination
│   ├── Error.cs
│   ├── ErrorCodes.cs
│   ├── Result.cs
│   ├── ResultExtensions.cs
│   └── Pagination/
│       ├── PaginatedList.cs
│       └── PaginationMetadata.cs
│
├── SharedKernel/                           # Shared Domain Concepts (Ubiquitous Language)
│   ├── User.cs                             # Abstract user base class
│   ├── UserRole.cs                         # User role enumeration
│   ├── ProviderType.cs                     # Healthcare provider types
│   ├── Attachment.cs                       # Shared attachment value object
│   ├── IRepository.cs                      # Repository pattern interface (domain concept)
│   ├── IUnitOfWork.cs                      # Unit of Work pattern interface (domain concept)
│   └── Events/                             # Domain Event Base Types
│       ├── IDomainEvent.cs                 # Marker interface for all domain events
│       ├── IDomainEventHandler.cs          # Handler interface for domain events
│       ├── IHasDomainEvents.cs             # Interface for entities with domain events collection
│       ├── IEventDispatcher.cs             # Interface for dispatching domain events
│       └── DomainEventBase.cs              # Abstract base class for domain events
│
├── Aggregates/                             # Core Domain Model (Aggregates & Entities)
│   │
│   ├── Patients/                           # Patient Bounded Context / Sub-domain
│   │   ├── Patient.cs                      # Aggregate Root
│   │   ├── Entities/
│   │   │   ├── Allergy.cs
│   │   │   ├── ChronicDisease.cs
│   │   │   ├── Medication.cs
│   │   │   └── Surgery.cs
│   │   ├── ValueObjects/
│   │   │   └── BloodType.cs
│   │   ├── Enums/
│   │   │   ├── BloodABO.cs
│   │   │   └── BloodRh.cs
│   │   ├── Specifications/
│   │   │   ├── ActivePatientsSpecification.cs
│   │   │   ├── PatientByIdSpecification.cs
│   │   │   └── PatientsByNationalIdSpecification.cs
│   │   └── Events/                         # Patient-specific domain events
│   │       ├── PatientRegisteredEvent.cs   # (future)
│   │       └── MedicalAttributeAddedEvent.cs # (future)
│   │
│   ├── Doctors/                            # Doctor Aggregate
│   │   ├── Doctor.cs                       # Aggregate Root
│   │   ├── Specifications/                 # (future)
│   │   └── Events/                         # (future)
│   │
│   ├── HealthcareStaff/                    # HealthcareStaff Aggregate (renamed from HealthcareEntity)
│   │   ├── HealthcareStaff.cs              # Aggregate Root
│   │   ├── Specifications/                 # (future)
│   │   └── Events/                         # (future)
│   │
│   ├── Laboratories/                       # Laboratory Aggregate
│   │   ├── Laboratory.cs                   # Aggregate Root
│   │   ├── Specifications/                 # (future)
│   │   └── Events/                         # (future)
│   │
│   ├── ImagingCenters/                     # ImagingCenter Aggregate
│   │   ├── ImagingCenter.cs                # Aggregate Root
│   │   ├── Specifications/                 # (future)
│   │   └── Events/                         # (future)
│   │
│   └── MedicalRecords/                     # MedicalRecord Aggregate
│       ├── MedicalRecord.cs                # Aggregate Root
│       ├── ValueObjects/
│       │   └── Practitioner.cs             # Practitioner snapshot
│       ├── Enums/
│       │   └── RecordType.cs
│       ├── Specifications/
│       │   ├── MedicalRecordByIdSpecification.cs
│       │   └── MedicalRecordsByPatientSpecification.cs
│       └── Events/                         # MedicalRecord-specific domain events
│           └── MedicalRecordCreatedEvent.cs # (future)
│
├── Services/                               # Domain Service Interfaces
│   ├── IFileStorageService.cs
│   ├── IIdentityService.cs
│   └── ITokenProvider.cs
│
└── Queries/                                # Query Service Interfaces
    ├── IMedicalRecordQueryService.cs
    └── IUserQueryService.cs
```

### Namespace Changes

| Current Namespace | New Namespace |
|-------------------|---------------|
| `MedicalCenter.Core.Common` | Split into multiple namespaces |
| `MedicalCenter.Core.Common.BaseEntity` | `MedicalCenter.Core.Abstractions` |
| `MedicalCenter.Core.Common.Result` | `MedicalCenter.Core.Primitives` |
| `MedicalCenter.Core.Common.User` | `MedicalCenter.Core.SharedKernel` |
| `MedicalCenter.Core.Common.Attachment` | `MedicalCenter.Core.SharedKernel` |
| `MedicalCenter.Core.Common.IRepository` | `MedicalCenter.Core.SharedKernel` |
| `MedicalCenter.Core.Common.IUnitOfWork` | `MedicalCenter.Core.SharedKernel` |
| `MedicalCenter.Core.Common.Pagination` | `MedicalCenter.Core.Primitives.Pagination` |
| `MedicalCenter.Core.Aggregates.Patient` | `MedicalCenter.Core.Aggregates.Patients` |
| `MedicalCenter.Core.Aggregates.MedicalRecord` | `MedicalCenter.Core.Aggregates.MedicalRecords` |
| `MedicalCenter.Core.Aggregates.Doctor` | `MedicalCenter.Core.Aggregates.Doctors` |
| `MedicalCenter.Core.Aggregates.HealthcareEntity` | `MedicalCenter.Core.Aggregates.HealthcareStaff` |
| `MedicalCenter.Core.Services` | `MedicalCenter.Core.Services` (unchanged - flat structure) |

---

## Detailed Changes

### Phase 1: Create New Folder Structure

1. **Create `Abstractions/` folder**
   - Move: `BaseEntity.cs`, `IAggregateRoot.cs`, `IAuditableEntity.cs`, `ValueObject.cs`
   - Update namespace: `MedicalCenter.Core.Abstractions`

2. **Create `Primitives/` folder**
   - Move: `Error.cs`, `ErrorCodes.cs`, `Result.cs`, `ResultExtensions.cs`
   - Create `Pagination/` subfolder: Move `PaginatedList.cs`, `PaginationMetadata.cs`
   - Update namespace: `MedicalCenter.Core.Primitives` (and `MedicalCenter.Core.Primitives.Pagination` for pagination types)

3. **Create `SharedKernel/` folder**
   - Move: `User.cs`, `UserRole.cs`, `ProviderType.cs`, `Attachment.cs`
   - Move: `IRepository.cs`, `IUnitOfWork.cs` (repositories are domain concepts)
   - Update namespace: `MedicalCenter.Core.SharedKernel`

4. **Create `Aggregates/` folder with sub-domains**
   - Restructure current `Aggregates/Patient/` → `Aggregates/Patients/`
   - Restructure current `Aggregates/MedicalRecord/` → `Aggregates/MedicalRecords/`
   - Create `Aggregates/Practitioners/` with subfolders for each practitioner type

5. **Add domain event base types to `SharedKernel/Events/`**
   - Add `IDomainEvent.cs` marker interface
   - Add `DomainEventBase.cs` abstract base class
   - Aggregate-specific events will live in `Aggregates/{Aggregate}/Events/` folders

6. **Keep `Services/` folder flat**
   - No subfolders - all service interfaces directly under `Services/`
   - Maintains simplicity and avoids infrastructure terminology in Core layer

### Phase 2: Patient Aggregate Reorganization

**From:**
```
Aggregates/Patient/
├── Patient.cs
├── Allergy.cs
├── BloodABO.cs
├── BloodRh.cs
├── BloodType.cs
├── ChronicDisease.cs
├── Medication.cs
├── Surgery.cs
└── Specifications/
```

**To:**
```
Aggregates/Patients/
├── Patient.cs                    # Aggregate Root
├── Entities/
│   ├── Allergy.cs
│   ├── ChronicDisease.cs
│   ├── Medication.cs
│   └── Surgery.cs
├── ValueObjects/
│   └── BloodType.cs
├── Enums/
│   ├── BloodABO.cs
│   └── BloodRh.cs
├── Specifications/
│   ├── ActivePatientsSpecification.cs
│   ├── PatientByIdSpecification.cs
│   └── PatientsByNationalIdSpecification.cs
└── Events/                       # (future)
```

### Phase 3: Practitioner Aggregates Reorganization

**From:**
```
Aggregates/
├── Doctor.cs
├── HealthcareEntity.cs
├── ImagingCenter.cs
└── Laboratory.cs
```

**To:**
```
Aggregates/
├── Doctors/
│   ├── Doctor.cs
│   └── Events/                   # (future)
├── HealthcareStaff/              # Renamed from HealthcareEntity for clearer domain language
│   ├── HealthcareStaff.cs       # Renamed from HealthcareEntity.cs
│   └── Events/                   # (future)
├── Laboratories/
│   ├── Laboratory.cs
│   └── Events/                   # (future)
└── ImagingCenters/
    ├── ImagingCenter.cs
    └── Events/                   # (future)
```

### Phase 4: Medical Record Aggregate Reorganization

**From:**
```
Aggregates/MedicalRecord/
├── MedicalRecord.cs
├── Practitioner.cs
├── RecordType.cs
└── Specifications/
```

**To:**
```
Aggregates/MedicalRecords/
├── MedicalRecord.cs
├── ValueObjects/
│   └── Practitioner.cs
├── Enums/
│   └── RecordType.cs
├── Specifications/
│   ├── MedicalRecordByIdSpecification.cs
│   └── MedicalRecordsByPatientSpecification.cs
└── Events/                       # (future)
```

### Phase 5: Remove Empty/Legacy Folders

- Delete: `Repositories/` (empty)
- Delete: `ValueObjects/` (empty)
- Delete: `Common/` (after all files moved)

---

## Migration Strategy

### Step-by-Step Approach

#### Step 1: Add New Files with New Namespaces (Non-Breaking)
1. Create all new folders
2. Copy files to new locations with updated namespaces
3. Add `global using` directives or explicit `using` statements in consuming projects

#### Step 2: Update Internal References
1. Update all internal references within Core project
2. Run build to ensure no compile errors
3. Run all tests

#### Step 3: Update External References
1. Update Infrastructure project references
2. Update WebAPI project references
3. Run full solution build and tests

#### Step 4: Remove Old Files
1. Delete original files from old locations
2. Delete empty folders
3. Final build and test verification

### Namespace Aliasing (Transition Period)

During migration, consider using namespace aliasing in consuming projects:

```csharp
// In Infrastructure project
global using MedicalCenter.Core.Abstractions;
global using MedicalCenter.Core.Primitives;
global using MedicalCenter.Core.SharedKernel;
global using MedicalCenter.Core.Domain.Patients;
global using MedicalCenter.Core.Domain.Practitioners.Doctor;
global using MedicalCenter.Core.Domain.MedicalRecords;
```

---

## Impact Analysis

### Files Affected

| Project | Estimated Files | Impact Level |
|---------|-----------------|--------------|
| MedicalCenter.Core | ~30 files | High (source of changes) |
| MedicalCenter.Infrastructure | ~15 files | Medium (namespace updates) |
| MedicalCenter.WebAPI | ~25 files | Medium (namespace updates) |
| MedicalCenter.Tests | ~20 files | Medium (namespace updates) |

### Breaking Changes

1. **Namespace Changes**: All consuming code must update `using` statements
2. **File Locations**: Any file-path-based references must be updated
3. **No API Changes**: Public contracts remain unchanged

### Risk Assessment

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Merge conflicts | Medium | Coordinate with team, perform during quiet period |
| Missing references | Low | Use compiler errors as guide |
| Test failures | Low | Run all tests after each step |

---

## Benefits

### 1. Clearer Domain Expression

The new structure makes the domain model self-documenting:

```
Aggregates/
├── Patients/           → "Patient is an aggregate with entities, value objects, and specs"
├── Doctors/            → "Doctor is an aggregate root"
├── HealthcareStaff/    → "HealthcareStaff is an aggregate root (renamed from HealthcareEntity)"
├── Laboratories/       → "Laboratory is an aggregate root"
├── ImagingCenters/     → "ImagingCenter is an aggregate root"
└── MedicalRecords/     → "MedicalRecord is an aggregate with value objects"
```

### 2. Better Separation of Concerns

| Folder | Purpose |
|--------|---------|
| `Abstractions/` | DDD building blocks - framework-level (BaseEntity, IAggregateRoot, ValueObject) |
| `Primitives/` | Result pattern & pagination - cross-cutting application concerns |
| `SharedKernel/` | Shared domain concepts - business-level (User, IRepository, IUnitOfWork, event base types) |
| `Aggregates/` | Core domain model - the heart of the system (events inline with aggregates) |
| `Services/` | Domain service interfaces (mix of infrastructure-adjacent and pure domain services) |
| `Queries/` | Query service interfaces (read-side) |

### 3. Improved Developer Experience

- **Discoverability**: New developers can navigate by domain concepts
- **Consistency**: All aggregates follow the same internal structure
- **Encapsulation**: Each aggregate's types are clearly grouped together

### 4. Future-Ready

- **Domain Events**: Base types in SharedKernel, aggregate-specific events inline with each aggregate
- **Bounded Contexts**: Structure supports future extraction - each aggregate is self-contained
- **Cross-Cutting Concerns**: Command/Query patterns can be introduced at endpoint level without architectural changes

### 5. Alignment with DDD Terminology

The folder names use DDD ubiquitous language:
- `Aggregates/` not `Models/`
- `Entities/` not `Classes/`
- `ValueObjects/` not `Types/`
- `Specifications/` not `Queries/`
- `SharedKernel/` not `Common/`

---

## Design Decisions (Resolved)

The following questions have been resolved with confirmed decisions:

### 1. Query Services Location ✅ DECIDED: Option A

**Decision**: Keep query services in Core under `Services/` (flat structure).

**Rationale**: 
- Maintains current architecture simplicity
- No separate Application/UseCases layer planned
- FastEndpoints organization at the endpoint level is sufficient
- Command/Query cross-cutting concerns will be introduced through endpoints, not through architectural layers

### 2. Attachment as Shared Concept ✅ DECIDED: Option A

**Decision**: Keep `Attachment` in `SharedKernel/Attachments/`.

**Rationale**:
- Attachments are anticipated to be used by future Encounter aggregate
- Keeps it available for multiple aggregates as a shared domain concept

### 3. User Base Class Location ✅ DECIDED: Option A

**Decision**: Keep `User` in `SharedKernel/`.

**Rationale**:
- User contains domain logic (IsActive, Role) making it a shared kernel concept
- Multiple aggregates (Patient, Doctor, HealthcareEntity, etc.) inherit from User
- Aligns with DDD SharedKernel pattern

### 4. Events Folder Organization ✅ DECIDED: Hybrid (Option C + SharedKernel)

**Decision**: 
- **Base types** (`IDomainEvent`, `DomainEventBase`) → `SharedKernel/Events/`
- **Aggregate-specific events** → Inline with aggregates: `Aggregates/Patients/Events/`, `Aggregates/MedicalRecords/Events/`

**Rationale**:
- Base types are shared abstractions used by all events - belong in SharedKernel
- Events are conceptually tied to their aggregates - should live alongside them
- Maximum cohesion: all Patient-related code (entities, value objects, specs, events) in one place
- Easy to find and maintain events for a specific aggregate

### 5. IRepository Location ✅ DECIDED: SharedKernel

**Decision**: Move `IRepository<T>` and `IUnitOfWork` to `SharedKernel/` instead of `Abstractions/`.

**Rationale**:
- Repositories are a domain concept (DDD pattern), not just a framework abstraction
- Aligns with DDD purist approach where repository pattern is part of the domain model
- Keeps domain contracts together in SharedKernel
- Maintains consistency: all domain-related interfaces in SharedKernel

### 6. Pagination Location ✅ DECIDED: Primitives

**Decision**: Move `Pagination/` from `SharedKernel/` to `Primitives/Pagination/`.

**Rationale**:
- Pagination is an application/infrastructure concern, not a domain concept
- Similar to Result pattern - a cross-cutting technical concern
- Keeps SharedKernel focused on true domain concepts (User, Repository, Events)
- Aligns with separation: Primitives = technical patterns, SharedKernel = domain concepts

### 7. HealthcareEntity Naming ✅ DECIDED: Rename to HealthcareStaff

**Decision**: Rename `HealthcareEntity` aggregate to `HealthcareStaff` for clearer domain language.

**Rationale**:
- "HealthcareStaff" better expresses the ubiquitous language - these are staff members
- "HealthcareEntity" is too generic and technical
- Aligns with domain expert terminology
- Requires updating class name, folder name, table name, and EF configuration

### 8. Services Folder Documentation ✅ DECIDED: Document Service Types

**Decision**: Keep flat `Services/` structure but document the distinction between service types.

**Rationale**:
- Infrastructure-adjacent services (`IFileStorageService`, `ITokenProvider`) are currently in Services/
- Future domain services (e.g., `IPricingService`, `IAppointmentSchedulingService`) will also go here
- Documenting the mix prevents confusion and clarifies intent
- Flat structure maintains simplicity while accommodating both types

---

## Architectural Notes

### No Application Layer / CQRS Separation

This project intentionally does **not** introduce a separate Application layer or formal CQRS pattern:

- **FastEndpoints organization** at the presentation layer is sufficient for current needs
- **Command/Query terminology** will be introduced through endpoint naming and cross-cutting concerns at the endpoint level
- **Query services remain in Core** as simple read-side interfaces without full CQRS infrastructure

This decision keeps the architecture simple while leaving room for future evolution if needed.

---

## Implementation Checklist

- [ ] Create `Abstractions/` folder and move files
- [ ] Create `Primitives/` folder and move files
- [ ] Create `SharedKernel/` folder and move files
- [ ] Create `Aggregates/Patients/` structure (including `Events/` subfolder)
- [ ] Create `Aggregates/Doctors/` structure (including `Events/` subfolder)
- [ ] Create `Aggregates/HealthcareStaff/` structure (including `Events/` subfolder) - rename from HealthcareEntity
- [ ] Create `Aggregates/Laboratories/` structure (including `Events/` subfolder)
- [ ] Create `Aggregates/ImagingCenters/` structure (including `Events/` subfolder)
- [ ] Create `Aggregates/MedicalRecords/` structure (including `Events/` subfolder)
- [ ] Create `SharedKernel/Events/` folder with base types (`IDomainEvent`, `IDomainEventHandler`, `IHasDomainEvents`, `IEventDispatcher`, `DomainEventBase`)
- [ ] Create `Queries/` folder and move query services
- [ ] Update all namespaces in Core project
- [ ] Rename `HealthcareEntity` class to `HealthcareStaff` and update all references
- [ ] Update `HealthcareEntityConfiguration.cs` to rename table from "HealthcareEntities" to "HealthcareStaff" and add EF migration
- [ ] Update MedicalCenter.Infrastructure references
- [ ] Update MedicalCenter.WebAPI references
- [ ] Update test project references
- [ ] Remove empty/legacy folders
- [ ] Update Architecture.md documentation
- [ ] Run full test suite
- [ ] Delete this temporary plan document

---

## Appendix: File Movement Checklist

### From Common/ to Abstractions/
- [ ] `BaseEntity.cs`
- [ ] `IAggregateRoot.cs`
- [ ] `IAuditableEntity.cs`
- [ ] `ValueObject.cs`

### From Common/ to Primitives/
- [ ] `Error.cs`
- [ ] `ErrorCodes.cs`
- [ ] `Result.cs`
- [ ] `ResultExtensions.cs`
- [ ] `PaginatedList.cs` → `Primitives/Pagination/`
- [ ] `PaginationMetadata.cs` → `Primitives/Pagination/`

### From Common/ to SharedKernel/
- [ ] `User.cs`
- [ ] `UserRole.cs`
- [ ] `ProviderType.cs`
- [ ] `Attachment.cs`
- [ ] `IRepository.cs` → `SharedKernel/` (repository is a domain concept)
- [ ] `IUnitOfWork.cs` → `SharedKernel/` (unit of work is a domain concept)

### New Files in SharedKernel/Events/
- [ ] Create `IDomainEvent.cs` (new file - marker interface)
- [ ] Create `IDomainEventHandler.cs` (new file - handler interface)
- [ ] Create `IHasDomainEvents.cs` (new file - interface for entities with domain events collection)
- [ ] Create `IEventDispatcher.cs` (new file - interface for dispatching domain events)
- [ ] Create `DomainEventBase.cs` (new file - abstract base class)

### Reorganize Aggregates/ (restructure existing folder)
- [ ] `Patient/Patient.cs` → `Aggregates/Patients/Patient.cs`
- [ ] `Patient/Allergy.cs` → `Aggregates/Patients/Entities/Allergy.cs`
- [ ] `Patient/ChronicDisease.cs` → `Aggregates/Patients/Entities/ChronicDisease.cs`
- [ ] `Patient/Medication.cs` → `Aggregates/Patients/Entities/Medication.cs`
- [ ] `Patient/Surgery.cs` → `Aggregates/Patients/Entities/Surgery.cs`
- [ ] `Patient/BloodType.cs` → `Aggregates/Patients/ValueObjects/BloodType.cs`
- [ ] `Patient/BloodABO.cs` → `Aggregates/Patients/Enums/BloodABO.cs`
- [ ] `Patient/BloodRh.cs` → `Aggregates/Patients/Enums/BloodRh.cs`
- [ ] `Patient/Specifications/*` → `Aggregates/Patients/Specifications/`
- [ ] `Doctor.cs` → `Aggregates/Doctors/Doctor.cs`
- [ ] `HealthcareEntity.cs` → `Aggregates/HealthcareStaff/HealthcareStaff.cs` (rename class and folder)
- [ ] `Laboratory.cs` → `Aggregates/Laboratories/Laboratory.cs`
- [ ] `ImagingCenter.cs` → `Aggregates/ImagingCenters/ImagingCenter.cs`
- [ ] `MedicalRecord/MedicalRecord.cs` → `Aggregates/MedicalRecords/MedicalRecord.cs`
- [ ] `MedicalRecord/Practitioner.cs` → `Aggregates/MedicalRecords/ValueObjects/Practitioner.cs`
- [ ] `MedicalRecord/RecordType.cs` → `Aggregates/MedicalRecords/Enums/RecordType.cs`
- [ ] `MedicalRecord/Specifications/*` → `Aggregates/MedicalRecords/Specifications/`

### Services (No Changes)
- [ ] `IFileStorageService.cs` → Keep in `Services/` (no change)
- [ ] `ITokenProvider.cs` → Keep in `Services/` (no change)
- [ ] `IIdentityService.cs` → Keep in `Services/` (no change)

### New Queries/ Folder
- [ ] `IMedicalRecordQueryService.cs` → `Queries/IMedicalRecordQueryService.cs`
- [ ] `IUserQueryService.cs` → `Queries/IUserQueryService.cs`

---

## Additional Notes

### Service Types in Services/ Folder

The `Services/` folder intentionally contains a mix of service interface types:

- **Infrastructure-adjacent services**: `IFileStorageService`, `ITokenProvider`, `IIdentityService`
  - These are technical services needed by the domain but implemented in Infrastructure
  - They abstract infrastructure concerns from the domain layer

- **Domain services** (future): `IPricingService`, `IAppointmentSchedulingService`, etc.
  - These represent domain operations that don't naturally fit within a single aggregate
  - They encapsulate business logic that spans multiple aggregates or requires external coordination

Both types belong in `Services/` as they define contracts used by the domain layer. The distinction is documented here for clarity, but no structural separation is needed.

---

*This document is temporary and should be deleted after the reorganization is complete.*

