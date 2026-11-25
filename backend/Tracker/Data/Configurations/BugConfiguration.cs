using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class BugConfiguration : IEntityTypeConfiguration<Bug>
{
    public void Configure(EntityTypeBuilder<Bug> entity)
    {
        entity.HasKey(e => e.Id).HasName("bugs_pkey");

        entity.ToTable("bugs", "bugtracker", table =>
        {
            table.HasCheckConstraint("bugs_check", """
                                                   (state = 'waiting' AND assignee_id IS NULL AND release_url IS NULL)
                                                   OR (state IN ('in_work', 'on_review') AND assignee_id IS NOT NULL AND release_url IS NULL)
                                                   OR (state = 'closed' AND assignee_id IS NOT NULL AND release_url IS NOT NULL)
                                                   """);

            table.HasCheckConstraint("bugs_version_check", "version > 0");
        });

        entity.HasIndex(e => new { e.ProjectId, e.AuthorId }, "bugs_project_id_author_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.Id }, "bugs_project_id_id_key").IsUnique();

        entity.HasIndex(e => new { e.ProjectId, e.SourceReportId }, "bugs_project_id_source_report_id_idx");

        entity.HasIndex(e => e.SourceReportId, "bugs_source_report_id_key").IsUnique();

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.AssigneeId).HasColumnName("assignee_id");
        entity.Property(e => e.AuthorId).HasColumnName("author_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Deadline).HasColumnName("deadline");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");
        entity.Property(e => e.ReleaseUrl).HasColumnName("release_url");
        entity.Property(e => e.ReproductionSteps).HasColumnName("reproduction_steps");
        entity.Property(e => e.SourceReportId).HasColumnName("source_report_id");
        entity.Property(e => e.Title).HasColumnName("title");
        entity.Property(e => e.Version)
            .HasDefaultValue(1)
            .HasColumnName("version")
            .IsConcurrencyToken();
        entity.Property(e => e.Priority).HasColumnName("priority");
        entity.Property(e => e.State).HasColumnName("state");

        entity.HasOne(d => d.Project).WithMany(p => p.Bugs)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("bugs_project_id_fkey");

        entity.HasOne(d => d.Member).WithMany(p => p.BugMembers)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.AssigneeId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("bugs_project_id_assignee_id_fkey");

        entity.HasOne(d => d.MemberNavigation).WithMany(p => p.BugMemberNavigations)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.AuthorId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("bugs_project_id_author_id_fkey");

        entity.HasOne(d => d.Report).WithMany(p => p.Bugs)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.SourceReportId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("bugs_project_id_source_report_id_fkey");
        
        entity.HasIndex(
            e => new { e.ProjectId, e.AssigneeId, e.State, e.Deadline },
            "bugs_project_id_assignee_id_state_deadline_idx");

        entity.HasIndex(
            e => new { e.ProjectId, e.State, e.CreatedAt },
            "bugs_project_id_state_created_at_idx");
    }
}