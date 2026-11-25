using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BugId { get; set; }

    public string Text { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Bug Bug { get; set; } = null!;

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual User User { get; set; } = null!;
}
