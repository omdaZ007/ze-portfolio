using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class ServicesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public ServicesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var query = _db.Services.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(s => s.Title.Contains(term) || s.Description.Contains(term));
        }

        ViewBag.Query = q;
        return View(await query.OrderBy(s => s.DisplayOrder).ToListAsync(cancellationToken));
    }

    [HttpGet]
    public IActionResult Create() => View(new ServiceFormViewModel { DisplayOrder = _db.Services.Count() + 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        _db.Services.Add(new Service
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Icon = model.Icon.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        });

        await _db.SaveChangesAsync(cancellationToken);
        Success($"Service “{model.Title}” added.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var service = await _db.Services.FindAsync(new object[] { id }, cancellationToken);
        if (service is null) return NotFound();

        return View(new ServiceFormViewModel
        {
            Id = service.Id,
            Title = service.Title,
            Description = service.Description,
            Icon = service.Icon,
            DisplayOrder = service.DisplayOrder,
            IsActive = service.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var service = await _db.Services.FindAsync(new object[] { id }, cancellationToken);
        if (service is null) return NotFound();

        service.Title = model.Title.Trim();
        service.Description = model.Description.Trim();
        service.Icon = model.Icon.Trim();
        service.DisplayOrder = model.DisplayOrder;
        service.IsActive = model.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        Success("Service updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var service = await _db.Services.FindAsync(new object[] { id }, cancellationToken);
        if (service is null) return NotFound();

        var title = service.Title;
        _db.Services.Remove(service);
        await _db.SaveChangesAsync(cancellationToken);
        Success($"Service “{title}” deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken cancellationToken)
    {
        var service = await _db.Services.FindAsync(new object[] { id }, cancellationToken);
        if (service is null) return NotFound();

        service.IsActive = !service.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Info"] = service.IsActive ? $"“{service.Title}” is now visible." : $"“{service.Title}” hidden from the site.";
        return RedirectToAction(nameof(Index));
    }
}
