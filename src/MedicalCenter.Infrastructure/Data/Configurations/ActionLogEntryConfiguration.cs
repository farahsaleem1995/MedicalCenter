using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedicalCenter.Core.Aggregates.ActionLogs;

namespace MedicalCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for ActionLogEntry aggregate.
/// </summary>
public class ActionLogEntryConfiguration : IEntityTypeConfiguration<ActionLogEntry>
{
    public void Configure(EntityTypeBuilder<ActionLogEntry> builder)
    {
        builder.ToTable("ActionLogs");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ActionName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(e => e.Payload)
            .HasMaxLength(10000); // Max 10KB
        
        builder.Property(e => e.ExecutedAt)
            .IsRequired();
        
        // Indexes for query performance
        builder.HasIndex(e => e.ExecutedAt)
            .IsDescending();
        
        builder.HasIndex(e => new { e.UserId, e.ExecutedAt })
            .IsDescending();
        
        builder.HasIndex(e => new { e.ActionName, e.ExecutedAt })
            .IsDescending();
    }
}

