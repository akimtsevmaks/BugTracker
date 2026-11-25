using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Channel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Destination { get; set; } = null!;

    public bool Enabled { get; set; }

    public virtual User User { get; set; } = null!;
}
