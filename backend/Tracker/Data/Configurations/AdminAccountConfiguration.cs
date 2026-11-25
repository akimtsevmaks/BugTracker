using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracker.Data.Models;

namespace Tracker.Data.Configurations;

public sealed class AdminAccountConfiguration : IEntityTypeConfiguration<AdminAccount>
{
    public void Configure(EntityTypeBuilder<AdminAccount> entity)
    {
        entity.HasKey(e => e.Id).HasName("admin_accounts_pkey");

        entity.ToTable("admin_accounts", "bugtracker");

        entity.HasIndex(e => e.NormalizedLogin, "admin_accounts_normalized_login_key").IsUnique();

        entity.Property(e => e.Id)
            .HasDefaultValueSql("uuidv7()")
            .HasColumnName("id").Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        entity.Property(e => e.DisplayName).HasColumnName("display_name");
        entity.Property(e => e.Login).HasColumnName("login");
        entity.Property(e => e.NormalizedLogin)
            .HasComputedColumnSql("lower(btrim((login)::text))", true)
            .HasColumnName("normalized_login");
        entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
    }
}