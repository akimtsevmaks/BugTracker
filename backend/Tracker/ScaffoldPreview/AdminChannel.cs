using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class AdminChannel
{
    public Guid Id { get; set; }

    public Guid AdminId { get; set; }

    public string Destination { get; set; } = null!;

    public bool Enabled { get; set; }

    public virtual AdminAccount Admin { get; set; } = null!;
}
