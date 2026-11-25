using Tracker.Data.Enums;

namespace Tracker.Data.Models;

public sealed class Channel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required ChannelType Type { get; init; }
    public string Destination { get; set; } = null!;
    public bool Enabled { get; set; }
    
    public User User { get; set; } = null!;
}