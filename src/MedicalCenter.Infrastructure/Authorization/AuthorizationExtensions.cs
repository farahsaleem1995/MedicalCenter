using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MedicalCenter.Core.Common;

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

            options.AddPolicy(AuthorizationPolicies.CanViewAllPatients, policy =>
                policy.RequireRole(
                    UserRole.Doctor.ToString(),
                    UserRole.HealthcareStaff.ToString(),
                    UserRole.SystemAdmin.ToString()));
        });

        return services;
    }
}

