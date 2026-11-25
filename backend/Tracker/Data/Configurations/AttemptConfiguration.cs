using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> entity)
    {
        entity.HasKey(e => e.Id).HasName("attempts_pkey");

        entity.ToTable("attempts", "bugtracker", table =>
        {
            table.HasCheckConstraint("attempts_check", """
                                                       (result IS NULL AND reviewer_id IS NULL AND reviewed_at IS NULL AND reason IS NULL)
                                                       OR
                                                       (result IS NOT NULL AND reviewer_id IS NOT NULL AND reviewed_at IS NOT NULL
                                                        AND ((result = 'accepted' AND reason IS NULL)
                                                             OR (result = 'returned' AND reason IS NOT NULL)))
                                                       """);

            table.HasCheckConstraint("attempts_check1",
                "reviewed_at IS NULL OR reviewed_at >= created_at");

            table.HasCheckConstraint("attempts_links_check", """
                                                             cardinality(links) BETWEEN 1 AND 20
                                                             AND array_ndims(links) = 1
                                                             AND array_position(links, NULL::bugtracker.nonempty) IS NULL
                                                             """);
        });

        entity.HasIndex(e => new { e.BugId, e.CreatedAt }, "attempts_bug_id_created_at_idx");

        entity.HasIndex(e => e.BugId, "attempts_bug_id_idx")
            .IsUnique()
            .HasFilter("(result IS NULL)");

        entity.HasIndex(e => new { e.ProjectId, e.BugId }, "attempts_project_id_bug_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.ReviewerId }, "attempts_project_id_reviewer_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.SubmittedById }, "attempts_project_id_submitted_by_id_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.BugId).HasColumnName("bug_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");
        entity.Property(e => e.Reason).HasColumnName("reason");
        entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
        entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
        entity.Property(e => e.SubmittedById).HasColumnName("submitted_by_id");
        entity.Property(e => e.Result).HasColumnName("result");
        entity.Property(e => e.Links)
            .HasColumnName("links")
            .HasColumnType("bugtracker.nonempty[]");

        entity.HasOne(d => d.Bug).WithMany(p => p.Attempts)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.BugId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attempts_project_id_bug_id_fkey");

        entity.HasOne(d => d.Member).WithMany(p => p.AttemptMembers)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.ReviewerId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attempts_project_id_reviewer_id_fkey");

        entity.HasOne(d => d.MemberNavigation).WithMany(p => p.AttemptMemberNavigations)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.SubmittedById })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attempts_project_id_submitted_by_id_fkey");
    }
}