using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Data.Enums;
using Tracker.Services.Accounts;


var initialAdmin = args is ["initial-admin"];


var builder = WebApplication.CreateBuilder(initialAdmin ? [] : args);
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString(initialAdmin ? "InitialConnection" : "DefaultConnection");

builder.Services.AddDbContext<TrackerDbContext>(options =>
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

builder.Services.AddHealthChecks().AddCheck<Tracker.Health.DatabaseHealthCheck>("database_check");




var app = builder.Build();

if (initialAdmin)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<TrackerDbContext>();
    
    var created = await AdminInitialization.CreateIfMissingAsync(db, app.Configuration["ADMIN_PASSWORD"]);

    Console.WriteLine(created ? "Админ создан" : "Админ уже существует");
    return;
}


app.MapControllers();

app.Run();