namespace Tracker.Data.Models;

public sealed class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string RepositoryUrl { get; set; } = null!;
    public bool IsArchived { get; set; }
    
    public ICollection<Bug> Bugs { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<ReportSource> ReportSources { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
}