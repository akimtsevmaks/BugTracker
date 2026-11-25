namespace Tracker.Data.Models;

public sealed class Attachment
{
    public Guid Id { get; set; }
    public Guid? BugId { get; set; }
    public Guid? ReportId { get; set; }
    public Guid? CommentId { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public byte[] Bytes { get; set; } = null!;
    
    public Bug? Bug { get; set; }
    public Comment? Comment { get; set; }
    public Report? Report { get; set; }
}