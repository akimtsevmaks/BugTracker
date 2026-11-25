using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> entity)
    {
        entity.HasKey(e => e.Id).HasName("members_pkey");

        entity.ToTable("members", "bugtracker", table =>
        {
            table.HasCheckConstraint("members_check", "NOT is_active OR roles > 0");
            table.HasCheckConstraint("members_roles_check", "roles BETWEEN 0 AND 7");
        });

        entity.HasIndex(e => new { e.ProjectId, e.Id }, "members_project_id_id_key").IsUnique();

        entity.HasIndex(e => new { e.ProjectId, e.UserId }, "members_project_id_user_id_key").IsUnique();

        entity.HasIndex(e => new { e.UserId, e.IsActive }, "members_user_id_is_active_idx");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ProjectId).HasColumnName("project_id");
        entity.Property(e => e.Roles).HasColumnName("roles");
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.HasOne(d => d.Project).WithMany(p => p.Members)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("members_project_id_fkey");

        entity.HasOne(d => d.User).WithMany(p => p.Members)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("members_user_id_fkey");
    }
}