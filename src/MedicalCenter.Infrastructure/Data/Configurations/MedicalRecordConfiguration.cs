using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.MedicalRecord;
using MedicalCenter.Core.Aggregates.Patient;
using MedicalCenter.Core.Common;
using MedicalCenter.Infrastructure.Identity;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for MedicalRecord aggregate.
/// </summary>
public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");

        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.PatientId)
            .IsRequired();

        builder.Property(mr => mr.PractitionerId)
            .IsRequired();

        builder.Property(mr => mr.RecordType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(mr => mr.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(mr => mr.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(mr => mr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(mr => mr.CreatedAt)
            .IsRequired();

        builder.Property(mr => mr.UpdatedAt);

        // Configure Practitioner as owned entity
        builder.OwnsOne(mr => mr.Practitioner, practitioner =>
        {
            practitioner.ToTable("MedicalRecords"); // Store in same table
            practitioner.Property(p => p.FullName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("PractitionerFullName");
            practitioner.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("PractitionerEmail");
            practitioner.Property(p => p.Role)
                .IsRequired()
                .HasConversion<int>()
                .HasColumnName("PractitionerRole");
        });

        // Configure Attachments as owned entity collection
        builder.OwnsMany(mr => mr.Attachments, attachment =>
        {
            attachment.ToTable("MedicalRecordAttachments");
            attachment.WithOwner().HasForeignKey("MedicalRecordId");
            
            attachment.Property(a => a.FileId)
                .IsRequired()
                .HasColumnName("FileId");

            attachment.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("FileName");

            attachment.Property(a => a.FileSize)
                .IsRequired()
                .HasColumnName("FileSize");

            attachment.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("ContentType");

            attachment.Property(a => a.UploadedAt)
                .IsRequired()
                .HasColumnName("UploadedAt");

            // Index on FileId for faster lookups
            attachment.HasIndex(a => a.FileId);
        });

        // Relationships
        // Patient relationship (navigation property to Patient aggregate)
        builder.HasOne(mr => mr.Patient)
            .WithMany()
            .HasForeignKey(mr => mr.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(mr => mr.PatientId);

        // Practitioner relationship (no navigation property, just FK - User entities are not aggregates)
        builder.HasIndex(mr => mr.PractitionerId);

        // Indexes
        builder.HasIndex(mr => new { mr.PatientId, mr.IsActive });
        builder.HasIndex(mr => new { mr.PractitionerId, mr.IsActive });
        builder.HasIndex(mr => new { mr.RecordType, mr.IsActive });

        // Global query filter: Only return active records (soft delete)
        builder.HasQueryFilter(mr => mr.IsActive);
    }
}
