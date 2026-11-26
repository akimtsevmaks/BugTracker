using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Tracker.Data;

namespace Tracker.Health;

public sealed class DatabaseHealthCheck(TrackerDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        var hasAdmin = await db.AdminAccounts.AnyAsync(ct);
        return hasAdmin ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}