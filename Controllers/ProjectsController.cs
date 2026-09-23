using Microsoft.AspNetCore.Mvc;
using ZE.Services;
using ZE.ViewModels;

namespace ZE.Controllers;

public class ProjectsController : Controller
{
    private const int PageSize = 9;
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects) => _projects = projects;

    [HttpGet("/projects")]
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _projects.SearchAsync(q, status: null, page: page,
            pageSize: PageSize, publishedOnly: true, cancellationToken);

        return View(model);
    }

    [HttpGet("/projects/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var project = await _projects.GetBySlugAsync(slug, cancellationToken);
        if (project is null) return NotFound();

        var related = await _projects.GetFeaturedAsync(6, cancellationToken);
        related.RemoveAll(p => p.Id == project.Id);

        return View(new ProjectDetailsViewModel
        {
            Project = project,
            RelatedProjects = related.Take(2).ToList()
        });
    }
}
