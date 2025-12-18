using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MedicalCenter.Core.Abstractions;

namespace MedicalCenter.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit properties (CreatedAt, UpdatedAt)
/// for entities implementing IAuditableEntity interface.
/// Only affects entities that implement IAuditableEntity - other entities are not modified.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            var auditableEntity = (IAuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                auditableEntity.UpdatedAt = utcNow;
            }
        }
    }
}

