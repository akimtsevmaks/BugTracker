using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.HasKey(e => e.Id).HasName("projects_pkey");

        entity.ToTable("projects", "bugtracker");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.IsArchived).HasColumnName("is_archived");
        entity.Property(e => e.Name).HasColumnName("name");
        entity.Property(e => e.RepositoryUrl).HasColumnName("repository_url");
    }
}