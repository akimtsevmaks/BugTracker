using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Comment
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid BugId { get; set; }

    public Guid AuthorId { get; set; }

    public string Text { get; set; } = null!;

    public bool IsPinned { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual Bug Bug { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
