using Tracker.Data.Enums;

namespace Tracker.Data.Models;

public sealed class Attempt
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid BugId { get; set; }
    public Guid SubmittedById { get; set; }
    public required string[] Links { get; init; }
    public Guid? ReviewerId { get; set; }
    public AttemptResult? Result { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    public Bug Bug { get; set; } = null!;
    public Member? Member { get; set; }
    public Member MemberNavigation { get; set; } = null!;
}