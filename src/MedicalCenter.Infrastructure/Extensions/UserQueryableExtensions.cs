using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenter.Infrastructure.Extensions;

public static class UserQueryableExtensions {
    public static IQueryable<ApplicationUser> IncludeRoles(this IQueryable<ApplicationUser> query)
    {
        return query.Include(u => u.Roles)
            .ThenInclude(r => r.Role);
    }

    public static IQueryable<ApplicationUser> IncludeDomainEntities(this IQueryable<ApplicationUser> query)
    {
        return query
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .Include(u => u.HealthcareStaff)
            .Include(u => u.Laboratory)
            .Include(u => u.ImagingCenter)
            .Include(u => u.SystemAdmin);
    }

    public static IQueryable<ApplicationUser> WhereActive(this IQueryable<ApplicationUser> query)
    {
        return query.Where(u =>
            // Check if the user's specific domain entity is active
            (u.Patient != null && u.Patient.IsActive) ||
            (u.Doctor != null && u.Doctor.IsActive) ||
            (u.HealthcareStaff != null && u.HealthcareStaff.IsActive) ||
            (u.Laboratory != null && u.Laboratory.IsActive) ||
            (u.ImagingCenter != null && u.ImagingCenter.IsActive) ||
            (u.SystemAdmin != null && u.SystemAdmin.IsActive));
    }

    public static IQueryable<ApplicationUser> WhereInactive(this IQueryable<ApplicationUser> query)
    {
        return query.Where(u =>
            // Check if the user's specific domain entity is inactive
            (u.Patient != null && !u.Patient.IsActive) ||
            (u.Doctor != null && !u.Doctor.IsActive) ||
            (u.HealthcareStaff != null && !u.HealthcareStaff.IsActive) ||
            (u.Laboratory != null && !u.Laboratory.IsActive) ||
            (u.ImagingCenter != null && !u.ImagingCenter.IsActive) ||
            (u.SystemAdmin != null && !u.SystemAdmin.IsActive));
    }

    public static IQueryable<ApplicationUser> WhereInRole(this IQueryable<ApplicationUser> query, UserRole role)
    {
        return query.Where(u => u.Roles.Any(r => r.Role.NormalizedName == role.ToString().ToUpper()));
    }

    public static IQueryable<ApplicationUser> WhereNationalIdContains(this IQueryable<ApplicationUser> query, string nationalId)
    {
        var searchTerm = nationalId.Trim().ToLower();
        return query.Where(u =>
            (u.Patient != null && u.Patient.NationalId.ToLower().Contains(searchTerm)) ||
            (u.Doctor != null && u.Doctor.NationalId.ToLower().Contains(searchTerm)) ||
            (u.HealthcareStaff != null && u.HealthcareStaff.NationalId.ToLower().Contains(searchTerm)) ||
            (u.Laboratory != null && u.Laboratory.NationalId.ToLower().Contains(searchTerm)) ||
            (u.ImagingCenter != null && u.ImagingCenter.NationalId.ToLower().Contains(searchTerm)) ||
            (u.SystemAdmin != null && u.SystemAdmin.NationalId.ToLower().Contains(searchTerm)));
    }
}