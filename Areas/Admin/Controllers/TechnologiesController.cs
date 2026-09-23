using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class TechnologiesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public TechnologiesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var technologies = await _db.Technologies.AsNoTracking()
            .Select(t => new TechnologyUsageViewModel
            {
                Technology = t,
                ProjectCount = t.Projects.Count
            })
            .OrderBy(x => x.Technology.Name)
            .ToListAsync(cancellationToken);

        return View(technologies);
    }

    [HttpGet]
    public IActionResult Create() => View(new TechnologyFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TechnologyFormViewModel model, CancellationToken cancellationToken)
    {
        if (await _db.Technologies.AnyAsync(t => t.Name == model.Name.Trim(), cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "This technology already exists.");

        if (!ModelState.IsValid) return View(model);

        _db.Technologies.Add(new Technology
        {
            Name = model.Name.Trim(),
            IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? null : model.IconClass.Trim(),
            IconUrl = string.IsNullOrWhiteSpace(model.IconUrl) ? null : model.IconUrl.Trim()
        });

        await _db.SaveChangesAsync(cancellationToken);
        Success($"Technology “{model.Name}” added.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var technology = await _db.Technologies.FindAsync(new object[] { id }, cancellationToken);
        if (technology is null) return NotFound();

        return View(new TechnologyFormViewModel
        {
            Id = technology.Id,
            Name = technology.Name,
            IconClass = technology.IconClass,
            IconUrl = technology.IconUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TechnologyFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();

        if (await _db.Technologies.AnyAsync(t => t.Name == model.Name.Trim() && t.Id != id, cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "This technology already exists.");

        if (!ModelState.IsValid) return View(model);

        var technology = await _db.Technologies.FindAsync(new object[] { id }, cancellationToken);
        if (technology is null) return NotFound();

        technology.Name = model.Name.Trim();
        technology.IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? null : model.IconClass.Trim();
        technology.IconUrl = string.IsNullOrWhiteSpace(model.IconUrl) ? null : model.IconUrl.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        Success("Technology updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var technology = await _db.Technologies
            .Include(t => t.Projects)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (technology is null) return NotFound();

        var name = technology.Name;
        var usedBy = technology.Projects.Count;

        _db.Technologies.Remove(technology);
        await _db.SaveChangesAsync(cancellationToken);

        Success(usedBy > 0
            ? $"Technology “{name}” deleted and removed from {usedBy} project(s)."
            : $"Technology “{name}” deleted.");
        return RedirectToAction(nameof(Index));
    }
}

public class TechnologyUsageViewModel
{
    public Technology Technology { get; init; } = null!;
    public int ProjectCount { get; init; }
}
