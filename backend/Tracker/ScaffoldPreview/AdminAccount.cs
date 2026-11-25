using System;
using System.Collections.Generic;

namespace Tracker.ScaffoldPreview;

public partial class AdminAccount
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string NormalizedLogin { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<AdminChannel> AdminChannels { get; set; } = new List<AdminChannel>();
}
