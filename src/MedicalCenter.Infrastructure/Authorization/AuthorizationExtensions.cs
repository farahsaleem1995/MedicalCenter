using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MedicalCenter.Core.Enums;

namespace MedicalCenter.Infrastructure.Authorization;

/// <summary>
/// Extension methods for configuring authorization policies.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Basic role policies
            options.AddPolicy(AuthorizationPolicies.RequirePatient, policy =>
                policy.RequireRole(UserRole.Patient.ToString()));

            options.AddPolicy(AuthorizationPolicies.RequireDoctor, policy =>
                policy.RequireRole(UserRole.Doctor.ToString()));

            options.AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
                policy.RequireRole(UserRole.SystemAdmin.ToString()));

            // Composite role policies
            options.AddPolicy(AuthorizationPolicies.RequireProvider, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString()));

            options.AddPolicy(AuthorizationPolicies.RequirePatientOrProvider, policy =>
                policy.RequireRole(
                    UserRole.Patient.ToString(),
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.LabUser.ToString(),
                    UserRole.ImagingUser.ToString()));

            // Claims-based policies
            options.AddPolicy(AuthorizationPolicies.CanModifyMedicalAttributes, policy =>
                policy.RequireClaim("role", "Doctor", "HealthcareStaff", "SystemAdmin"));

            options.AddPolicy(AuthorizationPolicies.CanCreateRecords, policy =>
                policy.RequireClaim("role", "Doctor", "HealthcareStaff", "LabUser", "ImagingUser"));

            options.AddPolicy(AuthorizationPolicies.CanViewAllPatients, policy =>
                policy.RequireClaim("role", "Doctor", "HealthcareStaff", "SystemAdmin"));
        });

        return services;
    }
}

