using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZE.Models;

namespace ZE.Data;

/// <summary>
/// Applies pending migrations and seeds the initial content required by the ZE website.
/// Secrets (admin credentials) are read from configuration/environment variables — never hardcoded.
/// </summary>
public static class DbInitializer
{
    public const string AdminRole = "Admin";

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed. Check the connection string.");
            throw;
        }

        await EnsureRolesAsync(roleManager, logger);
        await EnsureAdminAsync(userManager, config, logger);
        await EnsureContentAsync(db, logger);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
            logger.LogInformation("Created role {Role}", AdminRole);
        }
    }

    private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager,
        IConfiguration config, ILogger logger)
    {
        var email = config["Seed:AdminEmail"]
                    ?? Environment.GetEnvironmentVariable("ZE_ADMIN_EMAIL")
                    ?? "admin@ze.local";

        var password = config["Seed:AdminPassword"]
                       ?? Environment.GetEnvironmentVariable("ZE_ADMIN_PASSWORD");

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return;

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            logger.LogWarning(
                "No admin password configured. Set Seed:AdminPassword in user-secrets or the ZE_ADMIN_PASSWORD environment variable to create the admin account ({Email}).", email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "ZE Admin"
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, AdminRole);
            logger.LogInformation("Seeded admin account {Email}", email);
        }
        else
        {
            logger.LogWarning("Could not create admin account: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task EnsureContentAsync(ApplicationDbContext db, ILogger logger)
    {
        var hasFounders = await db.Founders.AnyAsync();
        var hasSkills = await db.Skills.AnyAsync();
        var hasServices = await db.Services.AnyAsync();
        var hasProjects = await db.Projects.AnyAsync();
        var hasTechnologies = await db.Technologies.AnyAsync();
        if (hasFounders && hasSkills && hasServices && hasProjects && hasTechnologies) return;

        if (!hasFounders)
        {
            // ---- Founders (real supplied photos) ----
            db.Founders.AddRange(
            new Founder
            {
                Name = "Ziad Fayed",
                Role = "Founder",
                JobTitle = "Full-Stack Developer",
                Bio = "Passionate about building modern web applications and turning ideas into real products. I love clean code, creative design and continuous learning.",
                ImageUrl = "/uploads/founders/ziad.png",
                AccentColor = "#F5B942",
                GithubUrl = "https://github.com/",
                LinkedInUrl = "https://www.linkedin.com/",
                DisplayOrder = 1,
                IsActive = true
            },
            new Founder
            {
                Name = "Mohamed Emad",
                Role = "Founder",
                JobTitle = "Business Management & Full-Stack Developer",
                Bio = "Focused on turning ideas into real opportunities. I combine technical skills with business strategy to build sustainable and impactful products.",
                ImageUrl = "/uploads/founders/mohamed.png",
                AccentColor = "#168BFF",
                GithubUrl = "https://github.com/",
                LinkedInUrl = "https://www.linkedin.com/",
                DisplayOrder = 2,
                IsActive = true
            });
        }

        if (!hasTechnologies)
        {
            // ---- Technologies ----
            var technologies = new[]
            {
                Tech("HTML5", "fa-brands fa-html5"),
                Tech("CSS3", "fa-brands fa-css3-alt"),
                Tech("JavaScript", "fa-brands fa-js"),
                Tech("Bootstrap", "fa-brands fa-bootstrap"),
                Tech("React", "fa-brands fa-react"),
                Tech("Tailwind CSS", "fa-solid fa-wind"),
                Tech("Git", "fa-brands fa-git-alt"),
                Tech("GitHub", "fa-brands fa-github"),
                Tech("C#", "fa-solid fa-code"),
                Tech(".NET", "fa-brands fa-microsoft"),
                Tech("ASP.NET Core", "fa-solid fa-server"),
                Tech("SQL Server", "fa-solid fa-database"),
                Tech("Entity Framework Core", "fa-solid fa-layer-group"),
                Tech("Figma", "fa-brands fa-figma"),
                Tech("Node.js", "fa-brands fa-node-js")
            };
            db.Technologies.AddRange(technologies);
        }

        if (!hasSkills)
        {
            // ---- Skills ----
            var skills = new[]
            {
                SkillData("HTML5", "/images/icon-html5.png", "Frontend", 1),
                SkillData("CSS3", "/images/icon-css3.png", "Frontend", 2),
                SkillData("JavaScript", "/images/icon-js.png", "Frontend", 3),
                SkillData("Bootstrap", "/images/icon-bootstrap.png", "Frontend", 4),
                SkillData("React", "/images/icon-react.png", "Frontend", 5),
                SkillData("Tailwind CSS", "/images/icon-tailwind.png", "Frontend", 6),
                SkillData("Git", "/images/icon-git.png", "Tools", 7),
                SkillData("GitHub", "/images/icon-github.png", "Tools", 8),
                SkillData("C#", "fa-solid fa-code", "Backend", 9),
                SkillData(".NET", "fa-brands fa-microsoft", "Backend", 10),
                SkillData("ASP.NET Core", "fa-solid fa-server", "Backend", 11),
                SkillData("SQL Server", "fa-solid fa-database", "Backend", 12),
                SkillData("Entity Framework Core", "fa-solid fa-layer-group", "Backend", 13),
                SkillData("Responsive Design", "fa-solid fa-display", "Design", 14)
            };
            db.Skills.AddRange(skills);
        }

        if (!hasServices)
        {
            // ---- Services ----
            db.Services.AddRange(
                ServiceData("Web Development", "Fast, accessible and SEO-friendly websites built with modern ASP.NET Core technology.", "fa-solid fa-code", 1),
                ServiceData("Full-Stack Development", "End-to-end product development — from database design to polished interfaces.", "fa-solid fa-layer-group", 2),
                ServiceData("UI/UX Design", "Clean, thoughtful interfaces focused on hierarchy, clarity and conversion.", "fa-solid fa-pen-ruler", 3),
                ServiceData("Responsive Websites", "Pixel-perfect layouts that feel right on 320px phones up to 1920px+ desktops.", "fa-solid fa-mobile-screen-button", 4),
                ServiceData("Business Websites", "Credible online presence for companies, brands and startups that need to grow.", "fa-solid fa-briefcase", 5),
                ServiceData("Custom Web Solutions", "Tailored dashboards, portals and internal tools designed around your workflow.", "fa-solid fa-screwdriver-wrench", 6)
            );
        }

        await db.SaveChangesAsync();

        if (!hasProjects)
        {
            // ---- Sample projects (fully dynamic — the admin can add more at runtime) ----
            var byName = await db.Technologies.ToDictionaryAsync(t => t.Name);

            var projects = new[]
            {
                MakeProject(db, byName, "Pharmacy Management System", "pharmacy-management-system",
                    "Inventory, prescriptions and sales for modern pharmacies with role-based dashboards.",
                    "A complete pharmacy management platform that tracks stock, handles prescriptions, manages suppliers and produces sales reports in real time. Built with ASP.NET Core MVC, Entity Framework Core and SQL Server, with a responsive interface and a full admin area.",
                    "/uploads/projects/proj-portal.png", "https://example.com/pharmacy",
                    "https://github.com/example/pharmacy-management-system", featured: true,
                    "ASP.NET Core", "SQL Server", "Entity Framework Core", "JavaScript"),

                MakeProject(db, byName, "Modern E-Commerce", "modern-ecommerce",
                    "A modern e-commerce platform with product management, cart and secure checkout.",
                    "A full storefront experience with product search, category filtering, cart, checkout flow and an admin panel for products and orders. Designed mobile-first with a dark premium identity and animated micro-interactions.",
                    "/uploads/projects/proj-dashboard.png", "https://example.com/shop",
                    "https://github.com/example/modern-ecommerce", featured: true,
                    "ASP.NET Core", "SQL Server", "JavaScript", "Bootstrap"),

                MakeProject(db, byName, "Agency Portfolio", "agency-portfolio",
                    "Portfolio and lead-generation site for a digital agency with animated hero sections.",
                    "A fast marketing site with animated heroes, project showcase, blog-ready architecture and a contact pipeline that stores leads in SQL Server. Ships with an admin dashboard for content editing.",
                    "/uploads/projects/proj-ze-platform.png", "https://example.com/agency",
                    "https://github.com/example/agency-portfolio", featured: true,
                    "ASP.NET Core", "React", "Tailwind CSS"),

                MakeProject(db, byName, "Real Estate Portal", "real-estate-portal",
                    "Property listings with advanced search, maps and agent dashboards.",
                    "A real estate portal with saved searches, property comparisons, image galleries and agent management. Backed by SQL Server with optimized queries and server-side paging.",
                    "/uploads/projects/proj-code-editor.png", "https://example.com/estate",
                    "https://github.com/example/real-estate-portal", featured: true,
                    "ASP.NET Core", "SQL Server", "Entity Framework Core"),

                MakeProject(db, byName, "Team Collaboration Suite", "team-collaboration-suite",
                    "Tasks, threads and file sharing for distributed teams in one workspace.",
                    "A collaboration workspace combining task boards, threaded discussions and shared files. Real-time updates via SignalR, with granular permissions and audit logging.",
                    "/uploads/projects/proj-api-console.png", "", "https://github.com/example/team-suite",
                    featured: false, "ASP.NET Core", "React", "SQL Server"),

                MakeProject(db, byName, "Analytics Dashboard", "analytics-dashboard",
                    "KPI dashboards with charts, filters and exportable reports for operators.",
                    "An analytics dashboard that turns raw data into decisions: configurable KPI cards, interactive charts, date-range filters and CSV/PDF exports, tuned for performance on large datasets.",
                    "/uploads/projects/proj-ui-kit.png", "https://example.com/analytics",
                    "https://github.com/example/analytics-dashboard", featured: false,
                    ".NET", "SQL Server", "JavaScript", "Bootstrap")
            };

            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Seeded {Founders} founders, {Skills} skills, {Services} services, {Projects} projects.",
            await db.Founders.CountAsync(), await db.Skills.CountAsync(),
            await db.Services.CountAsync(), await db.Projects.CountAsync());
    }

    private static Technology Tech(string name, string icon) => new() { Name = name, IconClass = icon };

    private static Skill SkillData(string name, string icon, string category, int order) =>
        new() { Name = name, Icon = icon, Category = category, DisplayOrder = order, IsActive = true };

    private static Service ServiceData(string title, string description, string icon, int order) =>
        new() { Title = title, Description = description, Icon = icon, DisplayOrder = order, IsActive = true };

    private static Project MakeProject(
        ApplicationDbContext db,
        Dictionary<string, Technology> byName,
        string title, string slug, string shortDescription, string description,
        string imageUrl, string liveUrl, string githubUrl, bool featured,
        params string[] technologies)
    {
        var project = new Project
        {
            Title = title,
            Slug = slug,
            ShortDescription = shortDescription,
            Description = description,
            ImageUrl = imageUrl,
            LiveUrl = string.IsNullOrWhiteSpace(liveUrl) ? null : liveUrl,
            GithubUrl = string.IsNullOrWhiteSpace(githubUrl) ? null : githubUrl,
            IsFeatured = featured,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(5, 90))
        };

        foreach (var name in technologies)
        {
            if (byName.TryGetValue(name, out var tech))
                project.Technologies.Add(new ProjectTechnology { Technology = tech });
        }

        return project;
    }
}
