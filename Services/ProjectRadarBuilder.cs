using ZE.Models;
using ZE.ViewModels;

namespace ZE.Services;

/// <summary>Builds the "Project Radar" orbit read models from published projects.</summary>
public static class ProjectRadarBuilder
{
    public static ProjectRadarViewModel Build(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Slug = project.Slug,
        ShortDescription = project.ShortDescription,
        ImageUrl = project.ImageUrl,
        LiveUrl = project.LiveUrl,
        GithubUrl = project.GithubUrl,
        Technologies = project.Technologies
            .Select(t => t.Technology?.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
    };
}
