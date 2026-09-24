using ZE.Models;
using ZE.ViewModels;

namespace ZE.Services;

/// <summary>
/// Builds the "Website Brain" read model: six abstract systems
/// (UI, Frontend, Backend, Database, Security, Performance) whose technology
/// chips are aggregated from the real projects + skills stored in the database.
/// Falls back to sensible defaults when a system has no matching data yet.
/// </summary>
public static class WebsiteBrainBuilder
{
    private sealed record SystemDef(
        string Key, string Label, string Icon, string Description,
        double X, double Y, string[] Keywords, string[] Fallback);

    private static readonly SystemDef[] Systems =
    {
        new("ui", "UI", "fa-solid fa-pen-ruler",
            "Hierarchy, layout and interaction — the surface people touch.",
            0.18, 0.45,
            new[] { "figma", "sketch", "xd", "photoshop", "canva", "ui/ux", "ui design", "ux", "design system" },
            new[] { "Design System", "Component Library" }),

        new("frontend", "FRONTEND", "fa-solid fa-code",
            "Components, state and responsive interfaces running in the browser.",
            0.32, 0.78,
            new[] { "react", "vue", "angular", "html", "css", "sass", "scss", "tailwind", "bootstrap",
                    "javascript", "typescript", "jquery", "next", "nuxt", "svelte", "blazor" },
            new[] { "Responsive UI", "Modern JavaScript" }),

        new("backend", "BACKEND", "fa-solid fa-server",
            "APIs, business logic and authentication behind the experience.",
            0.55, 0.18,
            new[] { "asp.net", ".net", "c#", "node", "express", "python", "php", "laravel",
                    "java", "rest", "api", "graphql" },
            new[] { "REST API", "Authentication" }),

        new("database", "DATABASE", "fa-solid fa-database",
            "Structured storage, relations and reliable data access.",
            0.84, 0.40,
            new[] { "sql", "postgres", "mongo", "mysql", "redis", "sqlite",
                    "entity framework", "ef core", "firebase", "supabase" },
            new[] { "SQL Server", "Entity Framework" }),

        new("security", "SECURITY", "fa-solid fa-shield-halved",
            "Safe sessions, protected routes and trusted connections.",
            0.68, 0.74,
            new[] { "identity", "jwt", "oauth", "auth", "https", "ssl", "tls", "roles", "validation", "cors" },
            new[] { "Secure Auth", "HTTPS" }),

        new("performance", "PERFORMANCE", "fa-solid fa-bolt",
            "Fast loads, smooth interaction and stable rendering.",
            0.46, 0.50,
            new[] { "cache", "caching", "cdn", "lighthouse", "optimize", "optimization",
                    "lazy", "vite", "webpack", "core web vitals" },
            new[] { "Caching", "Optimization" }),
    };

    public static WebsiteBrainViewModel Build(IEnumerable<Project> projects, IEnumerable<Skill>? skills = null)
    {
        var corpus = new List<string>();

        foreach (var project in projects)
        {
            foreach (var link in project.Technologies)
            {
                var name = link.Technology?.Name?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    corpus.Add(name);
            }
        }

        if (skills != null)
        {
            foreach (var skill in skills)
            {
                if (!string.IsNullOrWhiteSpace(skill.Name))
                    corpus.Add(skill.Name.Trim());
            }
        }

        var nodes = Systems.Select(sys =>
        {
            var matched = new List<string>();
            foreach (var entry in corpus.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (sys.Keywords.Any(k => entry.Contains(k, StringComparison.OrdinalIgnoreCase)) &&
                    !matched.Contains(entry, StringComparer.OrdinalIgnoreCase))
                {
                    matched.Add(entry);
                }
                if (matched.Count >= 4) break;
            }

            if (matched.Count == 0)
                matched.AddRange(sys.Fallback);

            return new BrainNodeViewModel
            {
                Key = sys.Key,
                Label = sys.Label,
                Icon = sys.Icon,
                Description = sys.Description,
                X = sys.X,
                Y = sys.Y,
                Technologies = matched.Take(4).ToList()
            };
        }).ToList();

        return new WebsiteBrainViewModel { Nodes = nodes };
    }
}
