using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new DashboardViewModel
        {
            TotalProjects = await _db.Projects.CountAsync(cancellationToken),
            PublishedProjects = await _db.Projects.CountAsync(p => p.IsPublished, cancellationToken),
            FeaturedProjects = await _db.Projects.CountAsync(p => p.IsFeatured && p.IsPublished, cancellationToken),
            DraftProjects = await _db.Projects.CountAsync(p => !p.IsPublished, cancellationToken),
            TotalSkills = await _db.Skills.CountAsync(cancellationToken),
            TotalServices = await _db.Services.CountAsync(cancellationToken),
            TotalTechnologies = await _db.Technologies.CountAsync(cancellationToken),
            TotalMessages = await _db.ContactMessages.CountAsync(cancellationToken),
            UnreadMessages = await _db.ContactMessages.CountAsync(m => !m.IsRead, cancellationToken),
            RecentProjects = await _db.Projects.AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken),
            RecentMessages = await _db.ContactMessages.AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken)
        };

        return View(model);
    }
}
