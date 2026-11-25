using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> entity)
    {
        entity.HasKey(e => e.Id).HasName("comments_pkey");

        entity.ToTable("comments", "bugtracker");

        entity.HasIndex(e => new { e.BugId, e.CreatedAt }, "comments_bug_id_created_at_idx");

        entity.HasIndex(e => new { e.ProjectId, e.AuthorId }, "comments_project_id_author_id_idx");

        entity.HasIndex(e => new { e.ProjectId, e.BugId }, "comments_project_id_bug_id_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.AuthorId).HasColumnName("author_id");
        entity.Property(e => e.BugId).HasColumnName("bug_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.IsPinned).HasColumnName("is_pinned");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");
        entity.Property(e => e.Text).HasColumnName("text");

        entity.HasOne(d => d.Member).WithMany(p => p.Comments)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.AuthorId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("comments_project_id_author_id_fkey");

        entity.HasOne(d => d.Bug).WithMany(p => p.Comments)
            .HasPrincipalKey(p => new { p.ProjectId, p.Id })
            .HasForeignKey(d => new { d.ProjectId, d.BugId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("comments_project_id_bug_id_fkey");
    }
}