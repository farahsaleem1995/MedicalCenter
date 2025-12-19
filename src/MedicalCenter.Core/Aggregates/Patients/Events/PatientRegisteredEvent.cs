using MedicalCenter.Core.SharedKernel.Events;

namespace MedicalCenter.Core.Aggregates.Patients.Events;

/// <summary>
/// Domain event raised when a patient is registered.
/// </summary>
public class PatientRegisteredEvent(
    Guid patientId,
    string fullName,
    string email,
    string nationalId,
    DateTime dateOfBirth) : DomainEventBase
{
    public Guid PatientId { get; } = patientId;
    public string FullName { get; } = fullName;
    public string Email { get; } = email;
    public string NationalId { get; } = nationalId;
    public DateTime DateOfBirth { get; } = dateOfBirth;
}

