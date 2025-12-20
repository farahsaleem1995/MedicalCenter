using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Infrastructure;
using MedicalCenter.WebApi.Extensions;
using MedicalCenter.WebApi.Attributes;
using MedicalCenter.WebApi.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Admin;

/// <summary>
/// Admin endpoint to create users (non-patients).
/// </summary>
[ActionLog("System administrator created a new user account")]
public class CreateUserEndpoint(
    IIdentityService identityService,
    IAuthorizationService authorizationService,
    IRepository<Doctor> doctorRepository,
    IRepository<HealthcareStaff> healthcareStaffRepository,
    IRepository<Laboratory> laboratoryRepository,
    IRepository<ImagingCenter> imagingCenterRepository,
    IRepository<SystemAdmin> systemAdminRepository,
    IUnitOfWork unitOfWork)
    : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("/users");
        Group<AdminGroup>();
        Policies(AuthorizationPolicies.RequireAdmin);
        Summary(s =>
        {
            s.Summary = "Create a new user (non-patient)";
            s.Description = "Allows system admin to create users of practitioner types (Doctor, HealthcareStaff, LabUser, ImagingUser). SystemAdmin accounts can only be created by Super Administrators.";
            s.Responses[200] = "User created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden - Admin access required or insufficient privileges for SystemAdmin creation";
            s.Responses[409] = "User already exists";
        });
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        // Business rule: SystemAdmin can only be created by Super Admins
        if (req.Role == UserRole.SystemAdmin)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(
                User, 
                AuthorizationPolicies.CanManageAdmins);
            
            if (!authorizationResult.Succeeded)
            {
                ThrowError("Only Super Administrators can create SystemAdmin accounts.", 403);
                return;
            }
        }

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Step 1: Create Identity user (generic user creation)
            // Only patients require email confirmation, admin-created users don't
            var createUserResult = await identityService.CreateUserAsync(
                req.Email,
                req.Password,
                req.Role,
                requireEmailConfirmation: false,
                ct);

            if (createUserResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                int statusCode = createUserResult.Error!.Code.ToStatusCode();
                ThrowError(createUserResult.Error.Message, statusCode);
                return;
            }

            Guid userId = createUserResult.Value;

            // Step 2: Create domain entity based on role
            switch (req.Role)
            {
                case UserRole.Doctor:
                    var doctor = new Doctor(userId, req.FullName, req.Email, req.LicenseNumber!, req.Specialty!);
                    await doctorRepository.AddAsync(doctor, ct);
                    break;

                case UserRole.HealthcareStaff:
                    var healthcareStaff = new HealthcareStaff(userId, req.FullName, req.Email, req.OrganizationName!, req.Department!);
                    await healthcareStaffRepository.AddAsync(healthcareStaff, ct);
                    break;

                case UserRole.LabUser:
                    var laboratory = new Laboratory(userId, req.FullName, req.Email, req.LabName!, req.LicenseNumber!);
                    await laboratoryRepository.AddAsync(laboratory, ct);
                    break;

                case UserRole.ImagingUser:
                    var imagingCenter = new ImagingCenter(userId, req.FullName, req.Email, req.CenterName!, req.LicenseNumber!);
                    await imagingCenterRepository.AddAsync(imagingCenter, ct);
                    break;

                case UserRole.SystemAdmin:
                    var systemAdmin = new SystemAdmin(userId, req.FullName, req.Email, req.CorporateId!, req.Department!);
                    await systemAdminRepository.AddAsync(systemAdmin, ct);
                    break;

                default:
                    await unitOfWork.RollbackTransactionAsync(ct);
                    ThrowError("Invalid role. Only practitioner roles and SystemAdmin are allowed.", 400);
                    return;
            }

            await unitOfWork.SaveChangesAsync(ct);

            // Commit transaction
            await unitOfWork.CommitTransactionAsync(ct);

            await Send.OkAsync(new CreateUserResponse
            {
                UserId = userId,
                Email = req.Email,
                FullName = req.FullName,
                Role = req.Role.ToString()
            }, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

}

