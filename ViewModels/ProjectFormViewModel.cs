using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ZE.Models;

namespace ZE.ViewModels;

/// <summary>Admin form for creating/editing a project (never exposes the EF entity directly).</summary>
public class ProjectFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [StringLength(180)]
    public string? Slug { get; set; }

    [Required, StringLength(300)]
    [Display(Name = "Short description")]
    public string ShortDescription { get; set; } = string.Empty;

    [Required, Display(Name = "Full description")]
    public string Description { get; set; } = string.Empty;

    [Url(ErrorMessage = "Enter a full URL, e.g. https://example.com")]
    [StringLength(400)]
    [Display(Name = "Live demo URL")]
    public string? LiveUrl { get; set; }

    [Url(ErrorMessage = "Enter a full URL, e.g. https://github.com/...")]
    [StringLength(400)]
    [Display(Name = "GitHub URL")]
    public string? GithubUrl { get; set; }

    [Display(Name = "Featured")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = true;

    /// <summary>Newly uploaded cover image (optional).</summary>
    public IFormFile? ImageFile { get; set; }

    /// <summary>Current image path kept when no new file is uploaded.</summary>
    public string? ExistingImageUrl { get; set; }

    /// <summary>When checked, the current image is removed.</summary>
    public bool RemoveImage { get; set; }

    [Display(Name = "Technologies")]
    public List<int> SelectedTechnologyIds { get; set; } = new();

    /// <summary>Technologies available for multi-select in the form.</summary>
    public IReadOnlyList<ZE.Models.Technology> AvailableTechnologies { get; set; } =
        Array.Empty<ZE.Models.Technology>();
}

public class ProjectListViewModel
{
    public IReadOnlyList<Project> Projects { get; init; } = Array.Empty<Project>();
    public string? Query { get; init; }
    public string? Status { get; init; }        // all | published | draft | featured
    public int Page { get; init; } = 1;
    public int TotalCount { get; init; }
    public int PageSize { get; init; } = 8;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public class ProjectDetailsViewModel
{
    public Project Project { get; init; } = null!;
    public IReadOnlyList<Project> RelatedProjects { get; init; } = Array.Empty<Project>();
}
