using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> entity)
    {
        entity.HasKey(e => e.Id).HasName("attachments_pkey");

        entity.ToTable("attachments", "bugtracker", table =>
        {
            table.HasCheckConstraint("attachments_bytes_check",
                "octet_length(bytes) <= 3145728");

            table.HasCheckConstraint("attachments_check",
                "num_nonnulls(bug_id, report_id, comment_id) = 1");
        });

        entity.HasIndex(e => e.BugId, "attachments_bug_id_idx");

        entity.HasIndex(e => e.CommentId, "attachments_comment_id_idx");

        entity.HasIndex(e => e.ReportId, "attachments_report_id_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.BugId).HasColumnName("bug_id");
        entity.Property(e => e.Bytes).HasColumnName("bytes");
        entity.Property(e => e.CommentId).HasColumnName("comment_id");
        entity.Property(e => e.ContentType).HasColumnName("content_type");
        entity.Property(e => e.FileName).HasColumnName("file_name");
        entity.Property(e => e.ReportId).HasColumnName("report_id");

        entity.HasOne(d => d.Bug).WithMany(p => p.Attachments)
            .HasForeignKey(d => d.BugId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attachments_bug_id_fkey");

        entity.HasOne(d => d.Comment).WithMany(p => p.Attachments)
            .HasForeignKey(d => d.CommentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attachments_comment_id_fkey");

        entity.HasOne(d => d.Report).WithMany(p => p.Attachments)
            .HasForeignKey(d => d.ReportId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("attachments_report_id_fkey");
    }
}