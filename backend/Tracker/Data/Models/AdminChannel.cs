using Tracker.Data.Enums;

namespace Tracker.Data.Models;

public sealed class AdminChannel
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public required ChannelType Channel { get; init; }
    public string Destination { get; set; } = null!;
    public bool Enabled { get; set; }
    
    public AdminAccount Admin { get; set; } = null!;
}