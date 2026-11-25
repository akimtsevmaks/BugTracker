using System;
using System.Collections.Generic;
using Tracker.ScaffoldPreview.Enums;

namespace Tracker.ScaffoldPreview;

public partial class Report
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

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<Bug> Bugs { get; set; } = new List<Bug>();

    public virtual Member? Member { get; set; }

    public virtual Member? MemberNavigation { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ReportSource? ReportSource { get; set; }
}
