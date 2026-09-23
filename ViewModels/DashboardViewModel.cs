using ZE.Models;

namespace ZE.ViewModels;

/// <summary>Admin dashboard overview statistics.</summary>
public class DashboardViewModel
{
    public int TotalProjects { get; init; }
    public int PublishedProjects { get; init; }
    public int FeaturedProjects { get; init; }
    public int DraftProjects { get; init; }
    public int TotalSkills { get; init; }
    public int TotalServices { get; init; }
    public int TotalTechnologies { get; init; }
    public int TotalMessages { get; init; }
    public int UnreadMessages { get; init; }
    public IReadOnlyList<Project> RecentProjects { get; init; } = Array.Empty<Project>();
    public IReadOnlyList<ContactMessage> RecentMessages { get; init; } = Array.Empty<ContactMessage>();
}
