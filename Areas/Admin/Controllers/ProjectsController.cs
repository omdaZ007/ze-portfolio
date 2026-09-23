using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;
using ZE.Services;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class ProjectsController : AdminControllerBase
{
    private const int PageSize = 8;

    private readonly ApplicationDbContext _db;
    private readonly IProjectService _projectService;
    private readonly IFileStorageService _storage;

    public ProjectsController(ApplicationDbContext db, IProjectService projectService,
        IFileStorageService storage)
    {
        _db = db;
        _projectService = projectService;
        _storage = storage;
    }

    // ------------------------------------------------------------------ LIST
    public async Task<IActionResult> Index(string? q, string? status, int page = 1,
        CancellationToken cancellationToken = default)
    {
        var model = await _projectService.SearchAsync(q, status, page, PageSize,
            publishedOnly: false, cancellationToken);

        ViewBag.Stats = new
        {
            Total = await _db.Projects.CountAsync(cancellationToken),
            Published = await _db.Projects.CountAsync(p => p.IsPublished, cancellationToken),
            Drafts = await _db.Projects.CountAsync(p => !p.IsPublished, cancellationToken)
        };

        return View(model);
    }

    // ----------------------------------------------------------------- CREATE
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var vm = new ProjectFormViewModel
        {
            AvailableTechnologies = await LoadTechnologiesAsync(cancellationToken)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTechnologies = await LoadTechnologiesAsync(cancellationToken);
            return View(model);
        }

        var project = new Project
        {
            Title = model.Title.Trim(),
            Slug = await EnsureUniqueSlugAsync(model.Slug, model.Title, cancellationToken),
            ShortDescription = model.ShortDescription.Trim(),
            Description = model.Description.Trim(),
            LiveUrl = string.IsNullOrWhiteSpace(model.LiveUrl) ? null : model.LiveUrl.Trim(),
            GithubUrl = string.IsNullOrWhiteSpace(model.GithubUrl) ? null : model.GithubUrl.Trim(),
            IsFeatured = model.IsFeatured,
            IsPublished = model.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        var imageUrl = await _storage.SaveImageAsync(model.ImageFile, "projects");
        project.ImageUrl = imageUrl ?? FallbackImage();

        await AttachTechnologiesAsync(project, model.SelectedTechnologyIds);

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        Success($"“{project.Title}” created" + (project.IsPublished ? " and published." : " as a draft."));
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------- EDIT
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        if (project is null) return NotFound();

        var vm = new ProjectFormViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Slug = project.Slug,
            ShortDescription = project.ShortDescription,
            Description = project.Description,
            LiveUrl = project.LiveUrl,
            GithubUrl = project.GithubUrl,
            IsFeatured = project.IsFeatured,
            IsPublished = project.IsPublished,
            ExistingImageUrl = project.ImageUrl,
            SelectedTechnologyIds = project.Technologies.Select(t => t.TechnologyId).ToList(),
            AvailableTechnologies = await LoadTechnologiesAsync(cancellationToken)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();

        var project = await _db.Projects
            .Include(p => p.Technologies)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project is null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.AvailableTechnologies = await LoadTechnologiesAsync(cancellationToken);
            model.ExistingImageUrl ??= project.ImageUrl;
            return View(model);
        }

        project.Title = model.Title.Trim();
        project.Slug = await EnsureUniqueSlugAsync(model.Slug, model.Title, cancellationToken,
            currentId: project.Id);
        project.ShortDescription = model.ShortDescription.Trim();
        project.Description = model.Description.Trim();
        project.LiveUrl = string.IsNullOrWhiteSpace(model.LiveUrl) ? null : model.LiveUrl.Trim();
        project.GithubUrl = string.IsNullOrWhiteSpace(model.GithubUrl) ? null : model.GithubUrl.Trim();
        project.IsFeatured = model.IsFeatured;
        project.IsPublished = model.IsPublished;
        project.UpdatedAt = DateTime.UtcNow;

        // ---- image handling: keep / replace / remove ----
        if (model.RemoveImage)
        {
            _storage.DeleteImage(project.ImageUrl);
            project.ImageUrl = null;
        }

        var newImage = await _storage.SaveImageAsync(model.ImageFile, "projects");
        if (!string.IsNullOrEmpty(newImage))
        {
            _storage.DeleteImage(project.ImageUrl);
            project.ImageUrl = newImage;
        }

        // ---- technologies (many-to-many) ----
        project.Technologies.Clear();
        await AttachTechnologiesAsync(project, model.SelectedTechnologyIds);

        await _db.SaveChangesAsync(cancellationToken);

        Success($"“{project.Title}” updated.");
        return RedirectToAction(nameof(Index));
    }

    // ----------------------------------------------------------------- DELETE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project is null) return NotFound();

        var title = project.Title;
        _storage.DeleteImage(project.ImageUrl);
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);

        Success($"“{title}” deleted.");
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------- QUICK FLAGS
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project is null) return NotFound();

        project.IsPublished = !project.IsPublished;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        TempData[project.IsPublished ? "Success" : "Info"] =
            project.IsPublished ? $"“{project.Title}” is live." : $"“{project.Title}” unpublished.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project is null) return NotFound();

        project.IsFeatured = !project.IsFeatured;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        TempData["Info"] = project.IsFeatured
            ? $"“{project.Title}” added to featured projects."
            : $"“{project.Title}” removed from featured projects.";
        return RedirectToAction(nameof(Index));
    }

    // ----------------------------------------------------------------- HELPERS
    private Task<List<Technology>> LoadTechnologiesAsync(CancellationToken cancellationToken) =>
        _db.Technologies.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);

    private async Task AttachTechnologiesAsync(Project project, IEnumerable<int> technologyIds)
    {
        var ids = technologyIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var technologies = await _db.Technologies
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();

        foreach (var technology in technologies)
            project.Technologies.Add(new ProjectTechnology { Technology = technology });
    }

    private async Task<string> EnsureUniqueSlugAsync(string? slugFromForm, string title,
        CancellationToken cancellationToken, int? currentId = null)
    {
        var slug = string.IsNullOrWhiteSpace(slugFromForm)
            ? SlugHelper.Slugify(title)
            : SlugHelper.Slugify(slugFromForm);

        if (string.IsNullOrEmpty(slug)) slug = "project-" + Guid.NewGuid().ToString("N")[..6];

        var candidate = slug;
        var suffix = 2;
        while (await _db.Projects.AnyAsync(p => p.Slug == candidate && p.Id != currentId,
                   cancellationToken))
        {
            candidate = $"{slug}-{suffix++}";
        }

        return candidate;
    }

    private static string FallbackImage() => "/images/browser-window.png";
}
