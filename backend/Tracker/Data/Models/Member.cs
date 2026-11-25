namespace Tracker.Data.Models;

public sealed class Member
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public int Roles { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Attempt> AttemptMemberNavigations { get; set; } = [];
    public ICollection<Attempt> AttemptMembers { get; set; } = [];
    public ICollection<Bug> BugMemberNavigations { get; set; } = [];
    public ICollection<Bug> BugMembers { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<History> Histories { get; set; } = [];
    public Project Project { get; set; } = null!;
    public ICollection<Report> ReportMemberNavigations { get; set; } = [];
    public ICollection<Report> ReportMembers { get; set; } = [];
    public User User { get; set; } = null!;
}