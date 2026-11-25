using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Member
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public int Roles { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Attempt> AttemptMemberNavigations { get; set; } = new List<Attempt>();

    public virtual ICollection<Attempt> AttemptMembers { get; set; } = new List<Attempt>();

    public virtual ICollection<Bug> BugMemberNavigations { get; set; } = new List<Bug>();

    public virtual ICollection<Bug> BugMembers { get; set; } = new List<Bug>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<Report> ReportMemberNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportMembers { get; set; } = new List<Report>();

    public virtual User User { get; set; } = null!;
}
