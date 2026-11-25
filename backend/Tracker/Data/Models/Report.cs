using Tracker.Data.Enums;

namespace Tracker.Data.Models;

public sealed class Report
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? SenderId { get; set; }
    public Guid? ExternalSourceId { get; set; }
    public string? ExternalId { get; set; }
    public string Content { get; set; } = null!;
    public required ReportState State { get; set; }
    public Guid? HandlerId { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Version { get; set; }
    
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Bug> Bugs { get; set; } = [];
    public Member? Member { get; set; }
    public Member? MemberNavigation { get; set; }
    public Project Project { get; set; } = null!;
    public ReportSource? ReportSource { get; set; }
}