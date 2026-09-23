using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;

namespace ZE.Services;

/// <summary>Read-side access to the dynamic page sections (skills, services, founders).</summary>
public interface IContentService
{
    Task<List<Skill>> GetActiveSkillsAsync(CancellationToken cancellationToken = default);
    Task<List<Service>> GetActiveServicesAsync(CancellationToken cancellationToken = default);
    Task<List<Founder>> GetActiveFoundersAsync(CancellationToken cancellationToken = default);
    Task<int> CountPublishedProjectsAsync(CancellationToken cancellationToken = default);
}

public class ContentService : IContentService
{
    private readonly ApplicationDbContext _db;

    public ContentService(ApplicationDbContext db) => _db = db;

    public Task<List<Skill>> GetActiveSkillsAsync(CancellationToken cancellationToken = default) =>
        _db.Skills.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public Task<List<Service>> GetActiveServicesAsync(CancellationToken cancellationToken = default) =>
        _db.Services.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<List<Founder>> GetActiveFoundersAsync(CancellationToken cancellationToken = default) =>
        _db.Founders.AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<int> CountPublishedProjectsAsync(CancellationToken cancellationToken = default) =>
        _db.Projects.AsNoTracking().CountAsync(p => p.IsPublished, cancellationToken);
}
