using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.HasKey(e => e.Id).HasName("notifications_pkey");

        entity.ToTable("notifications", "bugtracker");

        entity.HasIndex(e => e.BugId, "notifications_bug_id_idx");

        entity.HasIndex(e => new { e.UserId, e.CreatedAt, e.Id }, "notifications_user_id_created_at_id_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.BugId).HasColumnName("bug_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.IsRead).HasColumnName("is_read");
        entity.Property(e => e.Text).HasColumnName("text");
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.HasOne(d => d.Bug).WithMany(p => p.Notifications)
            .HasForeignKey(d => d.BugId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("notifications_bug_id_fkey");

        entity.HasOne(d => d.User).WithMany(p => p.Notifications)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("notifications_user_id_fkey");
    }
}