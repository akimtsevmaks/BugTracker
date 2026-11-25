using Tracker.Data.Enums;

namespace Tracker.Data.Models;

public sealed class Bug
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? SourceReportId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ReproductionSteps { get; set; } = null!;
    public required BugPriority Priority { get; set; }
    public required BugState State { get; set; }
    public DateOnly? Deadline { get; set; }
    public string? ReleaseUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Version { get; set; }
    
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Attempt> Attempts { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<History> Histories { get; set; } = [];
    public Member? Member { get; set; }
    public Member MemberNavigation { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = [];
    public Project Project { get; set; } = null!;
    public Report? Report { get; set; }
}