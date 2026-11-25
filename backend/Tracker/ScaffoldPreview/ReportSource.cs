using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class ReportSource
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? KeyHash { get; set; }

    public string? NormalizedName { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
