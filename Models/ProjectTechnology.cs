namespace ZE.Models;

/// <summary>Many-to-many join table between <see cref="Project"/> and <see cref="Technology"/>.</summary>
public class ProjectTechnology
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int TechnologyId { get; set; }
    public Technology Technology { get; set; } = null!;
}
