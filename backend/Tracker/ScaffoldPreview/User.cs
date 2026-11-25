using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class User
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string NormalizedLogin { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
