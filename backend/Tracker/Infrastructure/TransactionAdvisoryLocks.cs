using Microsoft.EntityFrameworkCore;
using Tracker.Data;

namespace Tracker.Infrastructure;

internal static class TransactionAdvisoryLocks
{
    public static Task Exclusive(TrackerDbContext db, string key, CancellationToken ct = default) =>
        db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 67))", ct);

    public static Task Shared(TrackerDbContext db, string key, CancellationToken ct = default) =>
        db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock_shared(hashtextextended({key}, 67))", ct);
}