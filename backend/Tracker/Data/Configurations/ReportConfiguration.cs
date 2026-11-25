using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> entity)
    {
        entity.HasKey(e => e.Id).HasName("reports_pkey");

        entity.ToTable("reports", "bugtracker", table =>
        {
            table.HasCheckConstraint("reports_check", """
                                                      (sender_id IS NOT NULL AND external_source_id IS NULL AND external_id IS NULL)
                                                      OR (sender_id IS NULL AND external_source_id IS NOT NULL AND external_id IS NOT NULL)
                                                      """);

            table.HasCheckConstraint("reports_check1", """
                                                       (state = 'not_processed' AND handler_id IS NULL AND explanation IS NULL)
                                                       OR (state = 'in_processing' AND handler_id IS NOT NULL AND explanation IS NULL)
                                                       OR (state = 'processed' AND handler_id IS NOT NULL)
                                                       """);

            table.HasCheckConstraint("reports_version_check", "version > 0");
        });

        entity.HasIndex(e => new { e.ExternalSourceId, e.ExternalId }, "reports_external_source_id_external_id_idx")
            .IsUnique()
            .HasFilter("(external_source_id IS NOT NULL)");

        entity.HasIndex(e => new { e.ProjectId, e.ExternalSourceId }, "reports_project_id_external_source_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.HandlerId }, "reports_project_id_handler_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.Id }, "reports_project_id_id_key").IsUnique();

        entity.HasIndex(e => new { e.ProjectId, e.SenderId }, "reports_project_id_sender_id_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.Content).HasColumnName("content");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Explanation).HasColumnName("explanation");
        entity.Property(e => e.ExternalId).HasColumnName("external_id");
        entity.Property(e => e.ExternalSourceId).HasColumnName("external_source_id");
        entity.Property(e => e.HandlerId).HasColumnName("handler_id");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");
        entity.Property(e => e.SenderId).HasColumnName("sender_id");
        entity.Property(e => e.Version)
            .HasDefaultValue(1)
            .HasColumnName("version")
            .IsConcurrencyToken();
        entity.Property(e => e.State).HasColumnName("state");

        entity.HasOne(d => d.Project).WithMany(p => p.Reports)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("reports_project_id_fkey");

        entity.HasOne(d => d.ReportSource).WithMany(p => p.Reports)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.ExternalSourceId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("reports_project_id_external_source_id_fkey");

        entity.HasOne(d => d.Member).WithMany(p => p.ReportMembers)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.HandlerId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("reports_project_id_handler_id_fkey");

        entity.HasOne(d => d.MemberNavigation).WithMany(p => p.ReportMemberNavigations)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.SenderId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("reports_project_id_sender_id_fkey");
        
        entity.HasIndex(
            e => new { e.ProjectId, e.State, e.CreatedAt },
            "reports_project_id_state_created_at_idx");
    }
}