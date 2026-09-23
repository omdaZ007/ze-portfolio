using Microsoft.AspNetCore.Mvc;
using ZE.Services;
using ZE.ViewModels;

namespace ZE.Controllers;

public class HomeController : Controller
{
    private readonly IContentService _content;
    private readonly IProjectService _projects;

    public HomeController(IContentService content, IProjectService projects)
    {
        _content = content;
        _projects = projects;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = new HomeViewModel
        {
            Skills = await _content.GetActiveSkillsAsync(cancellationToken),
            FeaturedProjects = await _projects.GetFeaturedAsync(4, cancellationToken),
            Founders = await _content.GetActiveFoundersAsync(cancellationToken),
            Services = await _content.GetActiveServicesAsync(cancellationToken),
            TotalPublishedProjects = await _content.CountPublishedProjectsAsync(cancellationToken)
        };

        return View(viewModel);
    }

    /// <summary>Friendly error page for 404 / 500 (no stack traces exposed).
    /// Accepts every HTTP method because UseStatusCodePagesWithReExecute forwards
    /// the original request method (e.g. a failed POST) to this action.</summary>
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS")]
    [Route("/Home/Error/{id?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? id)
    {
        var statusCode = id ?? HttpContext.Response.StatusCode;
        if (statusCode == 200 || statusCode == 0) statusCode = 500;
        ViewData["StatusCode"] = statusCode;
        return View();
    }
}
