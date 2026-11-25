using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tracker.ScaffoldPreview;

public partial class ScaffoldedTrackerDbContext : DbContext
{
    public ScaffoldedTrackerDbContext(DbContextOptions<ScaffoldedTrackerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminAccount> AdminAccounts { get; set; }

    public virtual DbSet<AdminChannel> AdminChannels { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<Bug> Bugs { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportSource> ReportSources { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("bugtracker", "attempt_result", new[] { "accepted", "returned" })
            .HasPostgresEnum("bugtracker", "bug_priority", new[] { "low", "medium", "high" })
            .HasPostgresEnum("bugtracker", "bug_state", new[] { "waiting", "in_work", "on_review", "closed" })
            .HasPostgresEnum("bugtracker", "channel_type", new[] { "vk", "telegram", "gmail", "max" })
            .HasPostgresEnum("bugtracker", "delivery_status", new[] { "pending", "sending", "sent", "failed" })
            .HasPostgresEnum("bugtracker", "history_property", new[] { "state", "assignee", "priority", "deadline", "release_url" })
            .HasPostgresEnum("bugtracker", "report_state", new[] { "not_processed", "in_processing", "processed" });

        modelBuilder.Entity<AdminAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_accounts_pkey");

            entity.ToTable("admin_accounts", "bugtracker");

            entity.HasIndex(e => e.NormalizedLogin, "admin_accounts_normalized_login_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.NormalizedLogin)
                .HasComputedColumnSql("lower(btrim((login)::text))", true)
                .HasColumnName("normalized_login");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        });

        modelBuilder.Entity<AdminChannel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_channels_pkey");

            entity.ToTable("admin_channels", "bugtracker");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.Destination).HasColumnName("destination");
            entity.Property(e => e.Enabled).HasColumnName("enabled");

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminChannels)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("admin_channels_admin_id_fkey");
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attachments_pkey");

            entity.ToTable("attachments", "bugtracker");

            entity.HasIndex(e => e.BugId, "attachments_bug_id_idx");

            entity.HasIndex(e => e.CommentId, "attachments_comment_id_idx");

            entity.HasIndex(e => e.ReportId, "attachments_report_id_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
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
        });

        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attempts_pkey");

            entity.ToTable("attempts", "bugtracker");

            entity.HasIndex(e => new { e.BugId, e.CreatedAt }, "attempts_bug_id_created_at_idx");

            entity.HasIndex(e => e.BugId, "attempts_bug_id_idx")
                .IsUnique()
                .HasFilter("(result IS NULL)");

            entity.HasIndex(e => new { e.ProjectId, e.BugId }, "attempts_project_id_bug_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.ReviewerId }, "attempts_project_id_reviewer_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.SubmittedById }, "attempts_project_id_submitted_by_id_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.BugId).HasColumnName("bug_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.SubmittedById).HasColumnName("submitted_by_id");

            entity.HasOne(d => d.Bug).WithMany(p => p.Attempts)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.BugId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("attempts_project_id_bug_id_fkey");

            entity.HasOne(d => d.Member).WithMany(p => p.AttemptMembers)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.ReviewerId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("attempts_project_id_reviewer_id_fkey");

            entity.HasOne(d => d.MemberNavigation).WithMany(p => p.AttemptMemberNavigations)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.SubmittedById })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("attempts_project_id_submitted_by_id_fkey");
        });

        modelBuilder.Entity<Bug>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bugs_pkey");

            entity.ToTable("bugs", "bugtracker");

            entity.HasIndex(e => new { e.ProjectId, e.AuthorId }, "bugs_project_id_author_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.Id }, "bugs_project_id_id_key").IsUnique();

            entity.HasIndex(e => new { e.ProjectId, e.SourceReportId }, "bugs_project_id_source_report_id_idx");

            entity.HasIndex(e => e.SourceReportId, "bugs_source_report_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.AssigneeId).HasColumnName("assignee_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ReleaseUrl).HasColumnName("release_url");
            entity.Property(e => e.ReproductionSteps).HasColumnName("reproduction_steps");
            entity.Property(e => e.SourceReportId).HasColumnName("source_report_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.Project).WithMany(p => p.Bugs)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bugs_project_id_fkey");

            entity.HasOne(d => d.Member).WithMany(p => p.BugMembers)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.AssigneeId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bugs_project_id_assignee_id_fkey");

            entity.HasOne(d => d.MemberNavigation).WithMany(p => p.BugMemberNavigations)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.AuthorId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bugs_project_id_author_id_fkey");

            entity.HasOne(d => d.Report).WithMany(p => p.Bugs)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.SourceReportId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bugs_project_id_source_report_id_fkey");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("channels_pkey");

            entity.ToTable("channels", "bugtracker");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Destination).HasColumnName("destination");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Channels)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("channels_user_id_fkey");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");

            entity.ToTable("comments", "bugtracker");

            entity.HasIndex(e => new { e.BugId, e.CreatedAt }, "comments_bug_id_created_at_idx");

            entity.HasIndex(e => new { e.ProjectId, e.AuthorId }, "comments_project_id_author_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.BugId }, "comments_project_id_bug_id_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
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
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("deliveries_pkey");

            entity.ToTable("deliveries", "bugtracker");

            entity.HasIndex(e => new { e.NextTryAt, e.Id }, "deliveries_next_try_at_id_idx").HasFilter("(status = ANY (ARRAY['pending'::delivery_status, 'sending'::delivery_status]))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Attempts).HasColumnName("attempts");
            entity.Property(e => e.Destination).HasColumnName("destination");
            entity.Property(e => e.LastError).HasColumnName("last_error");
            entity.Property(e => e.NextTryAt).HasColumnName("next_try_at");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");

            entity.HasOne(d => d.Notification).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("deliveries_notification_id_fkey");
        });

        modelBuilder.Entity<History>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("history_pkey");

            entity.ToTable("history", "bugtracker");

            entity.HasIndex(e => new { e.BugId, e.CreatedAt, e.Id }, "history_bug_id_created_at_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.ActorId }, "history_project_id_actor_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.BugId }, "history_project_id_bug_id_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.BugId).HasColumnName("bug_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.OperationId).HasColumnName("operation_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Reason).HasColumnName("reason");

            entity.HasOne(d => d.Member).WithMany(p => p.Histories)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.ActorId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("history_project_id_actor_id_fkey");

            entity.HasOne(d => d.Bug).WithMany(p => p.Histories)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.BugId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("history_project_id_bug_id_fkey");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("members_pkey");

            entity.ToTable("members", "bugtracker");

            entity.HasIndex(e => new { e.ProjectId, e.Id }, "members_project_id_id_key").IsUnique();

            entity.HasIndex(e => new { e.ProjectId, e.UserId }, "members_project_id_user_id_key").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.IsActive }, "members_user_id_is_active_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
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
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications", "bugtracker");

            entity.HasIndex(e => e.BugId, "notifications_bug_id_idx");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt, e.Id }, "notifications_user_id_created_at_id_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
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
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projects_pkey");

            entity.ToTable("projects", "bugtracker");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsArchived).HasColumnName("is_archived");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.RepositoryUrl).HasColumnName("repository_url");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reports_pkey");

            entity.ToTable("reports", "bugtracker");

            entity.HasIndex(e => new { e.ExternalSourceId, e.ExternalId }, "reports_external_source_id_external_id_idx")
                .IsUnique()
                .HasFilter("(external_source_id IS NOT NULL)");

            entity.HasIndex(e => new { e.ProjectId, e.ExternalSourceId }, "reports_project_id_external_source_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.HandlerId }, "reports_project_id_handler_id_idx");

            entity.HasIndex(e => new { e.ProjectId, e.Id }, "reports_project_id_id_key").IsUnique();

            entity.HasIndex(e => new { e.ProjectId, e.SenderId }, "reports_project_id_sender_id_idx");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.ExternalId).HasColumnName("external_id");
            entity.Property(e => e.ExternalSourceId).HasColumnName("external_source_id");
            entity.Property(e => e.HandlerId).HasColumnName("handler_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");

            entity.HasOne(d => d.Project).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reports_project_id_fkey");

            entity.HasOne(d => d.ReportSource).WithMany(p => p.Reports)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.ExternalSourceId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reports_project_id_external_source_id_fkey");

            entity.HasOne(d => d.Member).WithMany(p => p.ReportMembers)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.HandlerId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reports_project_id_handler_id_fkey");

            entity.HasOne(d => d.MemberNavigation).WithMany(p => p.ReportMemberNavigations)
                .HasPrincipalKey(p => new { p.ProjectId, p.Id })
                .HasForeignKey(d => new { d.ProjectId, d.SenderId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reports_project_id_sender_id_fkey");
        });

        modelBuilder.Entity<ReportSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("report_sources_pkey");

            entity.ToTable("report_sources", "bugtracker");

            entity.HasIndex(e => e.KeyHash, "report_sources_key_hash_key").IsUnique();

            entity.HasIndex(e => new { e.ProjectId, e.Id }, "report_sources_project_id_id_key").IsUnique();

            entity.HasIndex(e => new { e.ProjectId, e.NormalizedName }, "report_sources_project_id_normalized_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
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
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "bugtracker");

            entity.HasIndex(e => e.NormalizedLogin, "users_normalized_login_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.NormalizedLogin)
                .HasComputedColumnSql("lower(btrim((login)::text))", true)
                .HasColumnName("normalized_login");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
