namespace ZE.Models;

/// <summary>A technology/skill tag that can be attached to many projects.</summary>
public class Technology
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Font Awesome class (e.g. "fa-brands fa-html5") or a web image path.</summary>
    public string? IconClass { get; set; }

    public string? IconUrl { get; set; }

    public List<ProjectTechnology> Projects { get; set; } = new();
}
