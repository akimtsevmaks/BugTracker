using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> entity)
    {
        entity.HasKey(e => e.Id).HasName("channels_pkey");

        entity.ToTable("channels", "bugtracker", table =>
        {
            table.HasCheckConstraint("channels_check",
                "NOT enabled OR destination ~ '[^[:space:]]'");
        });

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.Destination).HasColumnName("destination");
        entity.Property(e => e.Enabled).HasColumnName("enabled");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.Type).HasColumnName("channel");

        entity.HasOne(d => d.User).WithMany(p => p.Channels)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("channels_user_id_fkey");
        
        entity.HasAlternateKey(e => new { e.UserId, e.Type })
            .HasName("channels_user_id_channel_key");
    }
}