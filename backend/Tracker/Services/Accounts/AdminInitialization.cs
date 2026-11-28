using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Data.Models;
using Tracker.Infrastructure;

namespace Tracker.Services.Accounts;

public sealed class AdminInitialization
{
    public static async Task<bool> CreateIfMissingAsync(
        TrackerDbContext db,
        string? initialPassword,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlAsync($"SET LOCAL ROLE bugtracker_owner", ct);
        await TransactionAdvisoryLocks.Exclusive(db, "initialization:admin", ct);

        var isAdminExists = await db.AdminAccounts.AnyAsync(ct);

        if (isAdminExists)
        {
            await transaction.CommitAsync(ct);
            return false;
        }

        if (initialPassword is not { Length: >= 8 and <= 128 })
        {
            throw new InvalidOperationException("пароль админа 8-128 символов");
        }

        var admin = new AdminAccount
        {
            Login = "admin",
            DisplayName = "Администратор"
        };

        var hasher = new PasswordHasher<object>();
        admin.PasswordHash = hasher.HashPassword(admin, initialPassword);

        db.AdminAccounts.Add(admin);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        
        return true;
    }
}