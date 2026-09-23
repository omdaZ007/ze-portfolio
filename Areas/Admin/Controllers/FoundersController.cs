using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;
using ZE.Services;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class FoundersController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _storage;

    public FoundersController(ApplicationDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _db.Founders.AsNoTracking().OrderBy(f => f.DisplayOrder)
            .ToListAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() =>
        View(new FounderFormViewModel { DisplayOrder = _db.Founders.Count() + 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FounderFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var founder = new Founder
        {
            Name = model.Name.Trim(),
            Role = model.Role.Trim(),
            JobTitle = model.JobTitle.Trim(),
            Bio = model.Bio.Trim(),
            AccentColor = model.AccentColor,
            GithubUrl = string.IsNullOrWhiteSpace(model.GithubUrl) ? null : model.GithubUrl.Trim(),
            LinkedInUrl = string.IsNullOrWhiteSpace(model.LinkedInUrl) ? null : model.LinkedInUrl.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };

        founder.ImageUrl = await _storage.SaveImageAsync(model.ImageFile, "founders");

        _db.Founders.Add(founder);
        await _db.SaveChangesAsync(cancellationToken);
        Success($"Founder “{model.Name}” added.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var founder = await _db.Founders.FindAsync(new object[] { id }, cancellationToken);
        if (founder is null) return NotFound();

        return View(new FounderFormViewModel
        {
            Id = founder.Id,
            Name = founder.Name,
            Role = founder.Role,
            JobTitle = founder.JobTitle,
            Bio = founder.Bio,
            ExistingImageUrl = founder.ImageUrl,
            AccentColor = founder.AccentColor,
            GithubUrl = founder.GithubUrl,
            LinkedInUrl = founder.LinkedInUrl,
            DisplayOrder = founder.DisplayOrder,
            IsActive = founder.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FounderFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var founder = await _db.Founders.FindAsync(new object[] { id }, cancellationToken);
        if (founder is null) return NotFound();

        founder.Name = model.Name.Trim();
        founder.Role = model.Role.Trim();
        founder.JobTitle = model.JobTitle.Trim();
        founder.Bio = model.Bio.Trim();
        founder.AccentColor = model.AccentColor;
        founder.GithubUrl = string.IsNullOrWhiteSpace(model.GithubUrl) ? null : model.GithubUrl.Trim();
        founder.LinkedInUrl = string.IsNullOrWhiteSpace(model.LinkedInUrl) ? null : model.LinkedInUrl.Trim();
        founder.DisplayOrder = model.DisplayOrder;
        founder.IsActive = model.IsActive;

        var newImage = await _storage.SaveImageAsync(model.ImageFile, "founders");
        if (!string.IsNullOrEmpty(newImage))
        {
            _storage.DeleteImage(founder.ImageUrl);
            founder.ImageUrl = newImage;
        }

        await _db.SaveChangesAsync(cancellationToken);
        Success("Founder updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var founder = await _db.Founders.FindAsync(new object[] { id }, cancellationToken);
        if (founder is null) return NotFound();

        var name = founder.Name;
        _storage.DeleteImage(founder.ImageUrl);
        _db.Founders.Remove(founder);
        await _db.SaveChangesAsync(cancellationToken);
        Success($"Founder “{name}” deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken cancellationToken)
    {
        var founder = await _db.Founders.FindAsync(new object[] { id }, cancellationToken);
        if (founder is null) return NotFound();

        founder.IsActive = !founder.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Info"] = founder.IsActive ? "Founder is now visible." : "Founder hidden from the site.";
        return RedirectToAction(nameof(Index));
    }
}
