using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Attachment
{
    public Guid Id { get; set; }

    public Guid? BugId { get; set; }

    public Guid? ReportId { get; set; }

    public Guid? CommentId { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public byte[] Bytes { get; set; } = null!;

    public virtual Bug? Bug { get; set; }

    public virtual Comment? Comment { get; set; }

    public virtual Report? Report { get; set; }
}
