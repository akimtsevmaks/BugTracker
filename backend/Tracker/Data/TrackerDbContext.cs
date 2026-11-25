using Microsoft.EntityFrameworkCore;
using Tracker.Data.Models;

namespace Tracker.Data;

public sealed class TrackerDbContext(DbContextOptions<TrackerDbContext> options) : DbContext(options)
{
    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();
    public DbSet<AdminChannel> AdminChannels => Set<AdminChannel>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<History> Histories => Set<History>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportSource> ReportSources => Set<ReportSource>();
    public DbSet<User> Users => Set<User>();

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("bugtracker", "attempt_result", [ "accepted", "returned" ])
            .HasPostgresEnum("bugtracker", "bug_priority", [ "low", "medium", "high" ])
            .HasPostgresEnum("bugtracker", "bug_state", [ "waiting", "in_work", "on_review", "closed" ])
            .HasPostgresEnum("bugtracker", "channel_type", [ "vk", "telegram", "gmail", "max" ])
            .HasPostgresEnum("bugtracker", "delivery_status", [ "pending", "sending", "sent", "failed" ])
            .HasPostgresEnum("bugtracker", "history_property", [ "state", "assignee", "priority", "deadline", "release_url" ])
            .HasPostgresEnum("bugtracker", "report_state", [ "not_processed", "in_processing", "processed" ]);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrackerDbContext).Assembly);
    }
}