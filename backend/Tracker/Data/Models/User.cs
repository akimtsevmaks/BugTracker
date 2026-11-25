namespace Tracker.Data.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public string Login { get; set; } = null!;
    public string NormalizedLogin { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    
    public ICollection<Channel> Channels { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}