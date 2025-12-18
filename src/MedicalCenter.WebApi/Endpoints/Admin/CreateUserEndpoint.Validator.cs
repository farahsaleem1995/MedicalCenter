using FastEndpoints;
using FluentValidation;

namespace MedicalCenter.WebApi.Endpoints.Admin;

public class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(role => role != MedicalCenter.Core.SharedKernel.UserRole.Patient)
            .WithMessage("Patient role is not allowed. Use the registration endpoint for patient accounts.");

        // Doctor validation
        When(x => x.Role == MedicalCenter.Core.SharedKernel.UserRole.Doctor, () =>
        {
            RuleFor(x => x.LicenseNumber)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(x => x.Specialty)
                .NotEmpty()
                .MaximumLength(200);
        });

        // HealthcareStaff validation
        When(x => x.Role == MedicalCenter.Core.SharedKernel.UserRole.HealthcareStaff, () =>
        {
            RuleFor(x => x.OrganizationName)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x.Department)
                .NotEmpty()
                .MaximumLength(200);
        });

        // LabUser validation
        When(x => x.Role == MedicalCenter.Core.SharedKernel.UserRole.LabUser, () =>
        {
            RuleFor(x => x.LabName)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x.LicenseNumber)
                .NotEmpty()
                .MaximumLength(100);
        });

        // ImagingUser validation
        When(x => x.Role == MedicalCenter.Core.SharedKernel.UserRole.ImagingUser, () =>
        {
            RuleFor(x => x.CenterName)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x.LicenseNumber)
                .NotEmpty()
                .MaximumLength(100);
        });

        // SystemAdmin validation
        When(x => x.Role == MedicalCenter.Core.SharedKernel.UserRole.SystemAdmin, () =>
        {
            RuleFor(x => x.CorporateId)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(x => x.Department)
                .NotEmpty()
                .MaximumLength(200);
        });
    }
}

