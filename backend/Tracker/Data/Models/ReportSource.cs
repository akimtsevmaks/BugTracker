namespace Tracker.Data.Models;

public sealed class ReportSource
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? KeyHash { get; set; }
    public string? NormalizedName { get; set; }
    
    public Project Project { get; set; } = null!;
    public ICollection<Report> Reports { get; set; } = [];
}