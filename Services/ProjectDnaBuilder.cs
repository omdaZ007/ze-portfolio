using ZE.Models;
using ZE.ViewModels;

namespace ZE.Services;

/// <summary>
/// Builds the "Project DNA" read models from <see cref="Project"/> entities.
/// Technologies are classified into strands (DESIGN, FRONTEND, BACKEND, …);
/// meta strands (RESPONSIVE / PERFORMANCE) also mirror relevant stack entries
/// so the helix stays rich without inventing fake data.
/// </summary>
public static class ProjectDnaBuilder
{
    private sealed record Strand(string Key, string Label, string Icon, string[] Keywords);

    private static readonly Strand[] StrandDefinitions =
    {
        new("design", "DESIGN", "fa-solid fa-pen-nib",
            new[] { "figma", "sketch", "adobe", "xd", "photoshop", "illustrator", "canva", "ui/ux", "ui design", "ux" }),
        new("frontend", "FRONTEND", "fa-solid fa-code",
            new[] { "react", "vue", "angular", "html", "css", "sass", "scss", "tailwind", "bootstrap",
                    "javascript", "typescript", "jquery", "next", "nuxt", "svelte", "webpack", "blazor" }),
        new("backend", "BACKEND", "fa-solid fa-server",
            new[] { "asp.net", ".net", "c#", "node", "express", "nest", "python", "django", "flask",
                    "php", "laravel", "java", "spring", "golang", "graphql", "rest", "api", "identity", "jwt" }),
        new("database", "DATABASE", "fa-solid fa-database",
            new[] { "sql", "postgres", "mongo", "mysql", "redis", "sqlite", "entity framework",
                    "ef core", "firebase", "supabase", "cosmos", "oracle" }),
        new("performance", "PERFORMANCE", "fa-solid fa-bolt",
            new[] { "lighthouse", "cache", "caching", "optimization", "optimize", "cdn", "core web vitals", "lazy" }),
        new("responsive", "RESPONSIVE", "fa-solid fa-mobile-screen-button",
            new[] { "responsive", "mobile", "pwa", "progressive web app" }),
        new("deployment", "DEPLOYMENT", "fa-solid fa-rocket",
            new[] { "docker", "azure", "vercel", "netlify", "github actions", "ci/cd", "iis",
                    "aws", "kubernetes", "nginx", "deploy" }),
    };

    /// <summary>Stack entries that also tell the RESPONSIVE story (mirrored, not moved).</summary>
    private static readonly string[] ResponsiveSeeds =
        { "css", "tailwind", "bootstrap", "sass", "scss", "responsive", "mobile", "pwa" };

    /// <summary>Stack entries that also tell the PERFORMANCE story (mirrored, not moved).</summary>
    private static readonly string[] PerformanceSeeds =
        { "redis", "cache", "caching", "cdn", "lighthouse", "optimize", "optimization", "vite" };

    public static ProjectDnaViewModel Build(Project project)
    {
        var names = project.Technologies
            .Select(t => t.Technology?.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var buckets = StrandDefinitions.ToDictionary(s => s.Key, _ => new List<string>(), StringComparer.Ordinal);

        foreach (var name in names)
        {
            var strand = StrandDefinitions.FirstOrDefault(s =>
                s.Keywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)));

            strand ??= LooksLikeFrontend(name) ? StrandDefinitions[1] : StrandDefinitions[2];

            var bucket = buckets[strand.Key];
            if (!bucket.Contains(name, StringComparer.OrdinalIgnoreCase))
                bucket.Add(name);
        }

        Mirror(buckets, names, ResponsiveSeeds, "responsive");
        Mirror(buckets, names, PerformanceSeeds, "performance");

        var nodes = new List<ProjectDnaNode>();
        var onLeft = false;

        foreach (var strand in StrandDefinitions)
        {
            var techs = buckets[strand.Key];
            if (techs.Count == 0) continue;

            nodes.Add(new ProjectDnaNode
            {
                Key = strand.Key,
                Label = strand.Label,
                Icon = strand.Icon,
                Side = onLeft ? "left" : "right",
                Technologies = techs
            });
            onLeft = !onLeft;
        }

        return new ProjectDnaViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Slug = project.Slug,
            ShortDescription = project.ShortDescription,
            ImageUrl = project.ImageUrl,
            LiveUrl = project.LiveUrl,
            GithubUrl = project.GithubUrl,
            Nodes = nodes
        };
    }

    private static void Mirror(Dictionary<string, List<string>> buckets, IEnumerable<string> names,
        string[] seeds, string targetKey)
    {
        foreach (var name in names)
        {
            if (!seeds.Any(s => name.Contains(s, StringComparison.OrdinalIgnoreCase))) continue;
            var bucket = buckets[targetKey];
            if (!bucket.Contains(name, StringComparer.OrdinalIgnoreCase))
                bucket.Add(name);
        }
    }

    private static bool LooksLikeFrontend(string name)
    {
        string[] hints = { "js", "ts", "react", "vue", "angular", "html", "css", "style", "web", "front", "ui" };
        return hints.Any(h => name.Contains(h, StringComparison.OrdinalIgnoreCase));
    }
}
