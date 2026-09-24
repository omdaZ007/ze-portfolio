namespace ZE.ViewModels;

/// <summary>Read model for one orbiting project in the "Project Radar" section.</summary>
public class ProjectRadarViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? LiveUrl { get; init; }
    public string? GithubUrl { get; init; }
    public IReadOnlyList<string> Technologies { get; init; } = Array.Empty<string>();
}
