using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Delivery
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public string Destination { get; set; } = null!;

    public int Attempts { get; set; }

    public DateTime NextTryAt { get; set; }

    public string? LastError { get; set; }

    public virtual Notification Notification { get; set; } = null!;
}
