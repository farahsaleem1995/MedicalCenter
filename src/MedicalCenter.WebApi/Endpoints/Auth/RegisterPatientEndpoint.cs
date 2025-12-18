using FastEndpoints;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure;
using MedicalCenter.WebApi.Extensions;

namespace MedicalCenter.WebApi.Endpoints.Auth;

/// <summary>
/// Patient registration endpoint - allows patients to self-register.
/// </summary>
public class RegisterPatientEndpoint(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings)
    : Endpoint<RegisterPatientRequest, RegisterPatientResponse>
{
    public override void Configure()
    {
        Post("/patients");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Register a new patient";
            s.Description = "Allows patients to self-register and receive authentication tokens";
            s.Responses[200] = "Registration successful";
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
            Patient patient = CreatePatientWithId(req.FullName, req.Email, req.NationalId, req.DateOfBirth, userId);
            await patientRepository.AddAsync(patient, ct);
            await unitOfWork.SaveChangesAsync(ct);

            // Step 3: Generate tokens
            string token = tokenProvider.GenerateAccessToken(patient);
            string refreshToken = tokenProvider.GenerateRefreshToken();

            // Step 4: Save refresh token
            DateTime expiryDate = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationInDays);
            var saveResult = await identityService.SaveRefreshTokenAsync(
                refreshToken,
                patient.Id,
                expiryDate,
                ct);

            if (saveResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                ThrowError("An error occurred during registration. Please try again later.", 500);
                return;
            }

            // Commit transaction
            await unitOfWork.CommitTransactionAsync(ct);

            await Send.OkAsync(new RegisterPatientResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = patient.Id,
                Email = patient.Email,
                FullName = patient.FullName
            }, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private static Patient CreatePatientWithId(string fullName, string email, string nationalId, DateTime dateOfBirth, Guid id)
    {
        // Use reflection to set the protected Id property
        Patient patient = Patient.Create(fullName, email, nationalId, dateOfBirth);
        System.Reflection.PropertyInfo? idProperty = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));
        idProperty?.SetValue(patient, id);
        return patient;
    }
}

