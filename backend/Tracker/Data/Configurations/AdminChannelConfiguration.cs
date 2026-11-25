using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class AdminChannelConfiguration : IEntityTypeConfiguration<AdminChannel>
{
    public void Configure(EntityTypeBuilder<AdminChannel> entity)
    {
        entity.HasKey(e => e.Id).HasName("admin_channels_pkey");

        entity.ToTable("admin_channels", "bugtracker", table =>
        {
            table.HasCheckConstraint("admin_channels_check",
                "NOT enabled OR destination ~ '[^[:space:]]'");
        });

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.AdminId).HasColumnName("admin_id");
        entity.Property(e => e.Channel).HasColumnName("channel");
        entity.Property(e => e.Destination).HasColumnName("destination");
        entity.Property(e => e.Enabled).HasColumnName("enabled");

        entity.HasOne(d => d.Admin).WithMany(p => p.AdminChannels)
            .HasForeignKey(d => d.AdminId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("admin_channels_admin_id_fkey");
        
        entity.HasAlternateKey(e => new { e.AdminId, e.Channel })
            .HasName("admin_channels_admin_id_channel_key");
    }
}