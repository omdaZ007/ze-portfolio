using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.Models;
using ZE.ViewModels;

namespace ZE.Services;

/// <summary>Read-side access to project content for both the public site and the admin area.</summary>
public interface IProjectService
{
    Task<List<Project>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default);
    Task<List<Project>> GetLatestPublishedAsync(int count, CancellationToken cancellationToken = default);
    Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ProjectListViewModel> SearchAsync(string? query, string? status, int page, int pageSize,
        bool publishedOnly = false, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _db;

    public ProjectService(ApplicationDbContext db) => _db = db;

    private static IQueryable<Project> WithTechnologies(IQueryable<Project> query) =>
        query.Include(p => p.Technologies).ThenInclude(t => t.Technology);

    public async Task<List<Project>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default) =>
        await WithTechnologies(_db.Projects.AsNoTracking()
                .Where(p => p.IsPublished && p.IsFeatured))
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<List<Project>> GetLatestPublishedAsync(int count, CancellationToken cancellationToken = default) =>
        await WithTechnologies(_db.Projects.AsNoTracking()
                .Where(p => p.IsPublished))
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await WithTechnologies(_db.Projects.AsNoTracking())
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished, cancellationToken);

    public async Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await WithTechnologies(_db.Projects)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<ProjectListViewModel> SearchAsync(string? query, string? status, int page,
        int pageSize, bool publishedOnly = false, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        var q = _db.Projects.AsNoTracking()
            .Include(p => p.Technologies).ThenInclude(t => t.Technology)
            .AsQueryable();

        if (publishedOnly) q = q.Where(p => p.IsPublished);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p => p.Title.Contains(term) ||
                             p.ShortDescription.Contains(term) ||
                             p.Technologies.Any(t => t.Technology.Name.Contains(term)));
        }

        q = status?.ToLowerInvariant() switch
        {
            "published" => q.Where(p => p.IsPublished),
            "draft" => q.Where(p => !p.IsPublished),
            "featured" => q.Where(p => p.IsFeatured),
            _ => q
        };

        var total = await q.CountAsync(cancellationToken);

        var projects = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ProjectListViewModel
        {
            Projects = projects,
            Query = query,
            Status = status,
            Page = page,
            TotalCount = total,
            PageSize = pageSize
        };
    }
}
