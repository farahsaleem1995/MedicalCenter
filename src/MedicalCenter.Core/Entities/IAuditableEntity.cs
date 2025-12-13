namespace MedicalCenter.Core.Entities;

/// <summary>
/// Interface for entities that require audit tracking.
/// Audit properties (CreatedAt, UpdatedAt) are set automatically by EF Core interceptor.
/// Not all entities implement this - e.g., ActionLog does not need audit tracking.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

