using MedicalCenter.Core.Common;
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
            .Include(u => u.HealthcareEntity)
            .Include(u => u.Laboratory)
            .Include(u => u.ImagingCenter);
    }

    public static IQueryable<ApplicationUser> WhereActive(this IQueryable<ApplicationUser> query)
    {
        return query.Where(u =>
            // SystemAdmin users (no domain entity relationship) are always active
            (u.Patient == null && u.Doctor == null && u.HealthcareEntity == null &&
             u.Laboratory == null && u.ImagingCenter == null) ||
            // Otherwise, check if the user's specific domain entity is active
            (u.Patient != null && u.Patient.IsActive) ||
            (u.Doctor != null && u.Doctor.IsActive) ||
            (u.HealthcareEntity != null && u.HealthcareEntity.IsActive) ||
            (u.Laboratory != null && u.Laboratory.IsActive) ||
            (u.ImagingCenter != null && u.ImagingCenter.IsActive));
    }

    public static IQueryable<ApplicationUser> WhereInactive(this IQueryable<ApplicationUser> query)
    {
        return query.Where(u =>
            // SystemAdmin users are always active, so exclude them from inactive filter
            // Check if the user's specific domain entity is inactive
            (u.Patient != null && !u.Patient.IsActive) ||
            (u.Doctor != null && !u.Doctor.IsActive) ||
            (u.HealthcareEntity != null && !u.HealthcareEntity.IsActive) ||
            (u.Laboratory != null && !u.Laboratory.IsActive) ||
            (u.ImagingCenter != null && !u.ImagingCenter.IsActive));
    }

    public static IQueryable<ApplicationUser> WhereInRole(this IQueryable<ApplicationUser> query, UserRole role)
    {
        return query.Where(u => u.Roles.Any(r => r.Role.NormalizedName == role.ToString().ToUpper()));
    }
}