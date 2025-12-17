using FastEndpoints;
using FluentValidation;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Aggregates.Patient.Specifications;
using MedicalCenter.Core.Common;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// Validator for update medication request.
/// </summary>
public class UpdateMedicationEndpointValidator : Validator<UpdateMedicationRequest>
{
    public UpdateMedicationEndpointValidator(IRepository<Patient> patientRepository)
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.MedicationId)
            .NotEmpty()
            .WithMessage("Medication ID is required.");

        RuleFor(x => x.Dosage)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Dosage))
            .WithMessage("Dosage cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");

        // Validate EndDate against existing medication's StartDate
        RuleFor(x => x.EndDate)
            .MustAsync(async (request, endDate, cancellation) =>
            {
                if (!endDate.HasValue)
                    return true; // EndDate is optional

                var specification = new PatientByIdSpecification(request.PatientId);
                var patient = await patientRepository.FirstOrDefaultAsync(specification, cancellation);
                
                if (patient == null)
                    return false; // Patient not found - will be caught by endpoint handler

                var medication = patient.Medications.FirstOrDefault(m => m.Id == request.MedicationId);
                if (medication == null)
                    return false; // Medication not found - will be caught by endpoint handler

                return endDate.Value >= medication.StartDate;
            })
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date cannot be before the medication start date.");
    }
}

