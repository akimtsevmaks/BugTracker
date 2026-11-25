using System;
using System.Collections.Generic;
using Tracker.ScaffoldPreview.Enums;

namespace Tracker.ScaffoldPreview;

public partial class Attempt
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid BugId { get; set; }

    public Guid SubmittedById { get; set; }

    public Guid? ReviewerId { get; set; }
    
    public AttemptResult? Result { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Bug Bug { get; set; } = null!;

    public virtual Member? Member { get; set; }

    public virtual Member MemberNavigation { get; set; } = null!;
}
