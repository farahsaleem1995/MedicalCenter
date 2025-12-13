# Patient Medical Attributes – Domain Model Draft

This document defines how core, long-term patient medical attributes are modeled in the Domain Layer.

Medical attributes represent **long-term, curated medical information** that defines the health profile of a patient.  
They are **not tied to a medical record or encounter**, but are part of the **Patient aggregate** itself.

---

# 1. Why Medical Attributes Are Part of the Patient Aggregate

Medical attributes such as:

- Blood Type  
- Allergies  
- Chronic Diseases  
- Current Medications  
- Past Surgeries  
- Family History

…represent information that:

- persists across all encounters  
- changes rarely  
- must be explicitly validated  
- cannot be inferred automatically from medical records  
- must be auditable and reviewed by authorized providers  

Therefore, they belong inside the **Patient aggregate root**.

Medical records can *suggest* such updates, but only a clinician can approve and apply them.

---

# 2. Domain Model Structure

## Patient Aggregate Overview

```csharp
public class Patient : User
{
    public string NationalId { get; private set; }
    public DateTime DateOfBirth { get; private set; }

    private readonly List<Allergy> _allergies = new();
    public IReadOnlyCollection<Allergy> Allergies => _allergies.AsReadOnly();

    private readonly List<ChronicDisease> _chronicDiseases = new();
    public IReadOnlyCollection<ChronicDisease> ChronicDiseases => _chronicDiseases.AsReadOnly();

    private readonly List<Medication> _medications = new();
    public IReadOnlyCollection<Medication> Medications => _medications.AsReadOnly();

    private readonly List<Surgery> _surgeries = new();
    public IReadOnlyCollection<Surgery> Surgeries => _surgeries.AsReadOnly();

    public BloodType? BloodType { get; private set; }

    // Methods to add/remove/update attributes with domain rules
}
```

---

# 3. Medical Attribute Entities / Value Objects

## Blood Type (Value Object)

```csharp
public class BloodType : ValueObject
{
    public string Group { get; private set; }  // A, B, AB, O
    public string Rhesus { get; private set; } // + or -

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Group;
        yield return Rhesus;
    }
}
```

## Allergy

```csharp
public class Allergy : BaseEntity
{
    public string Substance { get; private set; }
    public string Reaction { get; private set; }
    public string Severity { get; private set; }

    public DateTime RecordedAt { get; private set; }
    public Guid RecordedBy { get; private set; }
}
```

## Chronic Disease

```csharp
public class ChronicDisease : BaseEntity
{
    public string Name { get; private set; }
    public string Notes { get; private set; }

    public DateTime DiagnosedAt { get; private set; }
    public Guid DiagnosedBy { get; private set; }
}
```

## Medication

```csharp
public class Medication : BaseEntity
{
    public string Name { get; private set; }
    public string Dosage { get; private set; }
    public string Frequency { get; private set; }

    public DateTime PrescribedAt { get; private set; }
    public Guid PrescribedBy { get; private set; }
}
```

## Surgery

```csharp
public class Surgery : BaseEntity
{
    public string Name { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string Notes { get; private set; }

    public Guid PerformedBy { get; private set; }
}
```

---

# 4. EF Core Mapping

```csharp
modelBuilder.Entity<Patient>().HasMany(p => p.Allergies).WithOne().HasForeignKey("PatientId");
modelBuilder.Entity<Patient>().HasMany(p => p.ChronicDiseases).WithOne().HasForeignKey("PatientId");
modelBuilder.Entity<Patient>().HasMany(p => p.Medications).WithOne().HasForeignKey("PatientId");
modelBuilder.Entity<Patient>().HasMany(p => p.Surgeries).WithOne().HasForeignKey("PatientId");

modelBuilder.Entity<Patient>().OwnsOne(p => p.BloodType);
```

---

# 5. Domain Rules

- Only authorized providers can modify medical attributes  
- Patients cannot modify blood type or chronic conditions  
- All updates must have audit metadata  
- No automatic inference from uploaded records  

---

