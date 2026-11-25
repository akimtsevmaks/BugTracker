using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string RepositoryUrl { get; set; } = null!;

    public bool IsArchived { get; set; }

    public virtual ICollection<Bug> Bugs { get; set; } = new List<Bug>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<ReportSource> ReportSources { get; set; } = new List<ReportSource>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
