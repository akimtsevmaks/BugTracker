using System;
using System.Collections.Generic;
using Tracker.ScaffoldPreview.Enums;

namespace Tracker.ScaffoldPreview;

public partial class Delivery
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }
    
    public required ChannelType Channel { get; init; }

    public string Destination { get; set; } = null!;
    
    public required DeliveryStatus Status { get; set; }

    public int Attempts { get; set; }

    public DateTime NextTryAt { get; set; }

    public string? LastError { get; set; }

    public virtual Notification Notification { get; set; } = null!;
}
