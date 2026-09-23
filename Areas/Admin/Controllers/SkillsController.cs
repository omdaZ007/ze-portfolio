using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class SkillsController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public SkillsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var query = _db.Skills.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(s => s.Name.Contains(term) || (s.Category != null && s.Category.Contains(term)));
        }

        ViewBag.Query = q;
        return View(await query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            .ToListAsync(cancellationToken));
    }

    [HttpGet]
    public IActionResult Create() => View(new SkillFormViewModel { DisplayOrder = _db.Skills.Count() + 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SkillFormViewModel model, CancellationToken cancellationToken)
    {
        if (await _db.Skills.AnyAsync(s => s.Name == model.Name.Trim(), cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "A skill with this name already exists.");

        if (!ModelState.IsValid) return View(model);

        _db.Skills.Add(new Skill
        {
            Name = model.Name.Trim(),
            Icon = model.Icon.Trim(),
            Category = string.IsNullOrWhiteSpace(model.Category) ? null : model.Category.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        });

        await _db.SaveChangesAsync(cancellationToken);
        Success($"Skill “{model.Name}” added.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var skill = await _db.Skills.FindAsync(new object[] { id }, cancellationToken);
        if (skill is null) return NotFound();

        return View(new SkillFormViewModel
        {
            Id = skill.Id,
            Name = skill.Name,
            Icon = skill.Icon,
            Category = skill.Category,
            DisplayOrder = skill.DisplayOrder,
            IsActive = skill.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SkillFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();

        if (await _db.Skills.AnyAsync(s => s.Name == model.Name.Trim() && s.Id != id, cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "A skill with this name already exists.");

        if (!ModelState.IsValid) return View(model);

        var skill = await _db.Skills.FindAsync(new object[] { id }, cancellationToken);
        if (skill is null) return NotFound();

        skill.Name = model.Name.Trim();
        skill.Icon = model.Icon.Trim();
        skill.Category = string.IsNullOrWhiteSpace(model.Category) ? null : model.Category.Trim();
        skill.DisplayOrder = model.DisplayOrder;
        skill.IsActive = model.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        Success("Skill updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var skill = await _db.Skills.FindAsync(new object[] { id }, cancellationToken);
        if (skill is null) return NotFound();

        var name = skill.Name;
        _db.Skills.Remove(skill);
        await _db.SaveChangesAsync(cancellationToken);
        Success($"Skill “{name}” deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken cancellationToken)
    {
        var skill = await _db.Skills.FindAsync(new object[] { id }, cancellationToken);
        if (skill is null) return NotFound();

        skill.IsActive = !skill.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Info"] = skill.IsActive ? $"“{skill.Name}” is now visible." : $"“{skill.Name}” hidden from the site.";
        return RedirectToAction(nameof(Index));
    }
}
