namespace Tracker.Data.Models;

public sealed class AdminAccount
{
    public Guid Id { get; set; }
    public string Login { get; set; } = null!;
    public string NormalizedLogin { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    
    public ICollection<AdminChannel> AdminChannels { get; set; } = [];
}