namespace ZE.ViewModels;

/// <summary>Read model for one featured project inside the "Project DNA" section.</summary>
public class ProjectDnaViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? LiveUrl { get; init; }
    public string? GithubUrl { get; init; }
    public IReadOnlyList<ProjectDnaNode> Nodes { get; init; } = Array.Empty<ProjectDnaNode>();
}

/// <summary>One strand/node of a project's DNA (e.g. FRONTEND, DATABASE).</summary>
public class ProjectDnaNode
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    /// <summary>"right" or "left" — which side of the central spine the card sits on.</summary>
    public string Side { get; init; } = "right";
    public IReadOnlyList<string> Technologies { get; init; } = Array.Empty<string>();
}
