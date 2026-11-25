using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> entity)
    {
        entity.HasKey(e => e.Id).HasName("deliveries_pkey");

        entity.ToTable("deliveries", "bugtracker", table =>
        {
            table.HasCheckConstraint("deliveries_attempts_check", "attempts >= 0");
        });

        entity.HasIndex(e => new { e.NextTryAt, e.Id }, "deliveries_next_try_at_id_idx").HasFilter("(status = ANY (ARRAY['pending'::delivery_status, 'sending'::delivery_status]))");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.Attempts).HasColumnName("attempts");
        entity.Property(e => e.Destination).HasColumnName("destination");
        entity.Property(e => e.LastError).HasColumnName("last_error");
        entity.Property(e => e.NextTryAt).HasColumnName("next_try_at");
        entity.Property(e => e.NotificationId).HasColumnName("notification_id");
        entity.Property(e => e.Channel).HasColumnName("channel");
        entity.Property(e => e.Status).HasColumnName("status");

        entity.HasOne(d => d.Notification).WithMany(p => p.Deliveries)
            .HasForeignKey(d => d.NotificationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("deliveries_notification_id_fkey");
        
        entity.HasAlternateKey(e => new { e.NotificationId, e.Channel })
            .HasName("deliveries_notification_id_channel_key");
    }
}