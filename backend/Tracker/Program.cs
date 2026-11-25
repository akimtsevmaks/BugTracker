using Microsoft.EntityFrameworkCore;
using Tracker.ScaffoldPreview;
using Tracker.ScaffoldPreview.Enums;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ScaffoldedTrackerDbContext>(options =>
    options.UseNpgsql(connectionString, postgres =>
    {
        postgres.MapEnum<AttemptResult>("attempt_result", "bugtracker");
        postgres.MapEnum<BugPriority>("bug_priority", "bugtracker");
        postgres.MapEnum<BugState>("bug_state", "bugtracker");
        postgres.MapEnum<ChannelType>("channel_type", "bugtracker");
        postgres.MapEnum<DeliveryStatus>("delivery_status", "bugtracker");
        postgres.MapEnum<HistoryProperty>("history_property", "bugtracker");
        postgres.MapEnum<ReportState>("report_state", "bugtracker");
    }));

var app = builder.Build();

app.Run();