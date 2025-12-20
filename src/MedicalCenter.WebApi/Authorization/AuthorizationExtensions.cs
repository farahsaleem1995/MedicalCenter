using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.WebApi.Authorization.Handlers;
using MedicalCenter.WebApi.Authorization.Requirements;

namespace MedicalCenter.WebApi.Authorization;

/// <summary>
/// Extension methods for configuring authorization policies.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, CanManageAdminsHandler>();
        services.AddScoped<IAuthorizationHandler, CanViewActionLogHandler>();
        services.AddScoped<IAuthorizationHandler, CanAccessPHIHandler>();

        services.AddAuthorization(options =>
        {
            // Basic role policies (evaluated from JWT token)
            options.AddPolicy(AuthorizationPolicies.RequirePatient, policy =>
                policy.RequireRole(UserRole.Patient.ToString()));

            options.AddPolicy(AuthorizationPolicies.RequireDoctor, policy =>
                policy.RequireRole(UserRole.Doctor.ToString()));

            options.AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
                policy.RequireRole(UserRole.SystemAdmin.ToString()));

            // Composite role policies (evaluated from JWT token)
            options.AddPolicy(AuthorizationPolicies.RequirePractitioner, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString()));

            options.AddPolicy(AuthorizationPolicies.RequirePatientOrPractitioner, policy =>
                policy.RequireRole(
                    UserRole.Patient.ToString(),
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString()));

            options.AddPolicy(AuthorizationPolicies.CanViewMedicalAttributes, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.SystemAdmin.ToString()));
            
            options.AddPolicy(AuthorizationPolicies.CanModifyMedicalAttributes, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.SystemAdmin.ToString()));

            options.AddPolicy(AuthorizationPolicies.CanViewRecords, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString()));
            
            options.AddPolicy(AuthorizationPolicies.CanModifyRecords, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString()));

            options.AddPolicy(AuthorizationPolicies.CanViewEncounters, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString(),
                    UserRole.SystemAdmin.ToString()));

            options.AddPolicy(AuthorizationPolicies.CanViewAllPatients, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.SystemAdmin.ToString()));
            
            // Claims-based policies (require database lookups via authorization handlers)
            options.AddPolicy(AuthorizationPolicies.CanManageAdmins, policy =>
                policy.Requirements.Add(new CanManageAdminsRequirement()));

            options.AddPolicy(AuthorizationPolicies.CanViewActionLog, policy =>
                policy.Requirements.Add(new CanViewActionLogRequirement()));

            options.AddPolicy(AuthorizationPolicies.CanAccessPHI, policy =>
                policy.Requirements.Add(new CanAccessPHIRequirement()));
        });

        return services;
    }
}

