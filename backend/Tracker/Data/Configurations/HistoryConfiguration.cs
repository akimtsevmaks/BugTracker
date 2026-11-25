using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class HistoryConfiguration : IEntityTypeConfiguration<History>
{
    public void Configure(EntityTypeBuilder<History> entity)
    {
        entity.HasKey(e => e.Id).HasName("history_pkey");

        entity.ToTable("history", "bugtracker", table =>
        {
            table.HasCheckConstraint("history_check",
                "old_value IS DISTINCT FROM new_value");

            table.HasCheckConstraint("history_check1", """
                                                       property NOT IN ('priority', 'deadline')
                                                       OR (reason IS NOT NULL AND reason ~ '[^[:space:]]')
                                                       """);
        });

        entity.HasIndex(e => new { e.BugId, e.CreatedAt, e.Id }, "history_bug_id_created_at_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.ActorId }, "history_project_id_actor_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.BugId }, "history_project_id_bug_id_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.ActorId).HasColumnName("actor_id");
        entity.Property(e => e.BugId).HasColumnName("bug_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.NewValue).HasColumnName("new_value");
        entity.Property(e => e.OldValue).HasColumnName("old_value");
        entity.Property(e => e.OperationId).HasColumnName("operation_id");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");
        entity.Property(e => e.Reason).HasColumnName("reason");
        entity.Property(e => e.Property).HasColumnName("property");

        entity.HasOne(d => d.Member).WithMany(p => p.Histories)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.ActorId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("history_project_id_actor_id_fkey");

        entity.HasOne(d => d.Bug).WithMany(p => p.Histories)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.BugId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("history_project_id_bug_id_fkey");
        
        entity.HasAlternateKey(e => new { e.BugId, e.OperationId, e.Property })
            .HasName("history_bug_id_operation_id_property_key");
    }
}