namespace MedicalCenter.Core.Enums;

/// <summary>
/// Types of medical records in the system.
/// </summary>
public enum RecordType
{
    ConsultationNote = 1,
    LaboratoryResult = 2,
    ImagingReport = 3,
    Prescription = 4,
    Diagnosis = 5,
    TreatmentPlan = 6,
    Other = 99
}

