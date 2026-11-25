using System;
using System.Collections.Generic;
using Tracker.ScaffoldPreview.Enums;

namespace Tracker.ScaffoldPreview;

public partial class AdminChannel
{
    public Guid Id { get; set; }

    public Guid AdminId { get; set; }
    
    public required ChannelType Channel { get; init; }

    public string Destination { get; set; } = null!;

    public bool Enabled { get; set; }

    public virtual AdminAccount Admin { get; set; } = null!;
}
