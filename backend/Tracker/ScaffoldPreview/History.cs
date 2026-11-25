using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class History
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid BugId { get; set; }

    public Guid ActorId { get; set; }

    public Guid OperationId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Bug Bug { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
