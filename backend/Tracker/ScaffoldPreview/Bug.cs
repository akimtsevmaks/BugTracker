using System;
using System.Collections.Generic;
using Tracker.ScaffoldPreview.Enums;

namespace Tracker.ScaffoldPreview;

public partial class Bug
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

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual Member? Member { get; set; }

    public virtual Member MemberNavigation { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Project Project { get; set; } = null!;

    public virtual Report? Report { get; set; }
}
