namespace Tracker.Data.Models;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BugId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Bug Bug { get; set; } = null!;
    public ICollection<Delivery> Deliveries { get; set; } = [];
    public User User { get; set; } = null!;
}