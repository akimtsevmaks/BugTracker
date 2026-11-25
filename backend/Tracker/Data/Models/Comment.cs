namespace Tracker.Data.Models;

public sealed class Comment
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid BugId { get; set; }
    public Guid AuthorId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Attachment> Attachments { get; set; } = [];
    public Bug Bug { get; set; } = null!;
    public Member Member { get; set; } = null!;
}