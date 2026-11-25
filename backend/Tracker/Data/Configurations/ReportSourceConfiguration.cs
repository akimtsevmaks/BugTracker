using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class ReportSourceConfiguration : IEntityTypeConfiguration<ReportSource>
{
    public void Configure(EntityTypeBuilder<ReportSource> entity)
    {
        entity.HasKey(e => e.Id).HasName("report_sources_pkey");

        entity.ToTable("report_sources", "bugtracker");

        entity.HasIndex(e => e.KeyHash, "report_sources_key_hash_key").IsUnique();

        entity.HasIndex(e => new { e.ProjectId, e.Id }, "report_sources_project_id_id_key").IsUnique();

        entity.HasIndex(e => new { e.ProjectId, e.NormalizedName }, "report_sources_project_id_normalized_name_key").IsUnique();

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.KeyHash).HasColumnName("key_hash");
        entity.Property(e => e.Name).HasColumnName("name");
        entity.Property(e => e.NormalizedName)
            .HasComputedColumnSql("lower(btrim((name)::text))", true)
            .HasColumnName("normalized_name");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");

        entity.HasOne(d => d.Project).WithMany(p => p.ReportSources)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("report_sources_project_id_fkey");
    }
}