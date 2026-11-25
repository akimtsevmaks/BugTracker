using Tracker.Data.Enums;

namespace Tracker.Data.Models;

public sealed class History
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid BugId { get; set; }
    public Guid ActorId { get; set; }
    public Guid OperationId { get; set; }
    public required HistoryProperty Property { get; init; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Bug Bug { get; set; } = null!;
    public Member Member { get; set; } = null!;
}