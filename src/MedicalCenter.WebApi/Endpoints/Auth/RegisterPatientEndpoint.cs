using FastEndpoints;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Patient registration endpoint - allows patients to self-register.
/// </summary>
[Command]
public class RegisterPatientEndpoint(
    IIdentityService identityService,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<RegisterPatientRequest>
{
    public override void Configure()
    {
        Post("/patients");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Register a new patient";
            s.Description = "Allows patients to self-register. Email confirmation is required before login.";
            s.Responses[204] = "Registration successful";
            s.Responses[400] = "Validation error";
            s.Responses[409] = "User already exists";
        });
    }

    public override async Task HandleAsync(RegisterPatientRequest req, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Step 1: Create Identity user (generic user creation)
            var createUserResult = await identityService.CreateUserAsync(
                req.Email,
                req.Password,
                UserRole.Patient,
                requireEmailConfirmation: true, // Patients require email confirmation
                ct);

            if (createUserResult.IsFailure)
            {
                int statusCode = createUserResult.Error!.Code.ToStatusCode();
                await unitOfWork.RollbackTransactionAsync(ct);
                ThrowError(createUserResult.Error.Message, statusCode);
                return;
            }

            Guid userId = createUserResult.Value;

            // Step 2: Create Patient entity with patient-specific details
            Patient patient = new Patient(userId, req.FullName, req.Email, req.NationalId, req.DateOfBirth);
            await patientRepository.AddAsync(patient, ct);
            await unitOfWork.SaveChangesAsync(ct);

            // Commit transaction
            await unitOfWork.CommitTransactionAsync(ct);

            await Send.NoContentAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

}

