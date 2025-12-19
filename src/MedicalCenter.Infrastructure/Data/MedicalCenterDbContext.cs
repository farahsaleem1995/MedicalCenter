using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.Doctors;
using MedicalCenter.Core.Aggregates.HealthcareStaff;
using MedicalCenter.Core.Aggregates.Laboratories;
using MedicalCenter.Core.Aggregates.ImagingCenters;
using MedicalCenter.Core.Aggregates.SystemAdmins;
using MedicalCenter.Infrastructure.Data.Configurations;
using MedicalCenter.Infrastructure.Data.Interceptors;
using MedicalCenter.Infrastructure.Data.Seeders;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for the Medical Center application.
/// Integrates ASP.NET Core Identity with domain entities.
/// </summary>
public class MedicalCenterDbContext(
    DbContextOptions<MedicalCenterDbContext> options,
    AuditableEntityInterceptor auditableEntityInterceptor,
    DomainEventDispatcherInterceptor domainEventDispatcherInterceptor) : IdentityDbContext<
        ApplicationUser, 
        ApplicationRole, 
        Guid,
        IdentityUserClaim<Guid>,
        ApplicationUserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>(options)
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor = auditableEntityInterceptor;
    private readonly DomainEventDispatcherInterceptor _domainEventDispatcherInterceptor = domainEventDispatcherInterceptor;

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<HealthcareStaff> HealthcareStaff { get; set; } = null!;
    public DbSet<Laboratory> Laboratories { get; set; } = null!;
    public DbSet<ImagingCenter> ImagingCenters { get; set; } = null!;
    public DbSet<SystemAdmin> SystemAdmins { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from current assembly
        // This will automatically pick up all IEntityTypeConfiguration<T> implementations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedicalCenterDbContext).Assembly);

        // Configure Identity tables to use Guid as primary key
        IdentityConfiguration.ConfigureIdentityTables(modelBuilder);

        // Seed initial data using ModelBuilder extensions
        modelBuilder.SeedData();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor, _domainEventDispatcherInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}

