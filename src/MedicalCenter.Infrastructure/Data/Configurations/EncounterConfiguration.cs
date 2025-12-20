using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.Encounters;
using MedicalCenter.Core.Aggregates.Patients;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for Encounter aggregate.
/// Encounters are NOT auditable - they only have OccurredOn (when the medical event occurred).
/// </summary>
public class EncounterConfiguration : IEntityTypeConfiguration<Encounter>
{
    public void Configure(EntityTypeBuilder<Encounter> builder)
    {
        builder.ToTable("Encounters");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PatientId)
            .IsRequired();

        builder.Property(e => e.OccurredOn)
            .IsRequired()
            .HasColumnName("OccurredOn"); // Explicit column name (NOT CreatedAt)

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        // Configure Practitioner as owned entity
        builder.OwnsOne(e => e.Practitioner, practitioner =>
        {
            practitioner.ToTable("Encounters"); // Store in same table
            practitioner.Property(p => p.FullName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("PractitionerFullName");
            practitioner.Property(p => p.Role)
                .IsRequired()
                .HasConversion<int>()
                .HasColumnName("PractitionerRole");
        });

        // Relationships
        // Patient relationship (navigation property to Patient aggregate)
        builder.HasOne(e => e.Patient)
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.PatientId);

        // Indexes for query performance
        builder.HasIndex(e => new { e.PatientId, e.OccurredOn });
        builder.HasIndex(e => e.OccurredOn);
        // Note: Index on PractitionerEmail is not directly possible via HasIndex with owned entities
        // The index will be created via SQL in the migration if needed for performance
    }
}

