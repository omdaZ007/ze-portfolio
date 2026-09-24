using ZE.Models;

namespace ZE.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Skill> Skills { get; init; } = Array.Empty<Skill>();
    public IReadOnlyList<Project> FeaturedProjects { get; init; } = Array.Empty<Project>();
    public IReadOnlyList<ProjectDnaViewModel> DnaProjects { get; init; } = Array.Empty<ProjectDnaViewModel>();
    public IReadOnlyList<ProjectRadarViewModel> RadarProjects { get; init; } = Array.Empty<ProjectRadarViewModel>();
    public WebsiteBrainViewModel Brain { get; init; } = new();
    public IReadOnlyList<Founder> Founders { get; init; } = Array.Empty<Founder>();
    public IReadOnlyList<Service> Services { get; init; } = Array.Empty<Service>();
    public ContactFormViewModel ContactForm { get; init; } = new();
    public int TotalPublishedProjects { get; init; }
}
