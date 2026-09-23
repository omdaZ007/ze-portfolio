using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ZE.Models;

namespace ZE.ViewModels;

public class FounderFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(160)]
    [Display(Name = "Job title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Bio { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }
    public string? ExistingImageUrl { get; set; }

    [RegularExpression("^#([0-9A-Fa-f]{6})$", ErrorMessage = "Use a hex color such as #F5B942.")]
    [StringLength(40)]
    [Display(Name = "Accent color")]
    public string AccentColor { get; set; } = "#F5B942";

    [Url, StringLength(400)]
    [Display(Name = "GitHub URL")]
    public string? GithubUrl { get; set; }

    [Url, StringLength(400)]
    [Display(Name = "LinkedIn URL")]
    public string? LinkedInUrl { get; set; }

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public class SkillFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    [Display(Name = "Icon (FA class or image path)")]
    public string Icon { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Category { get; set; }

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public class ServiceFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(300)]
    [Display(Name = "Icon (FA class or image path)")]
    public string Icon { get; set; } = string.Empty;

    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public class TechnologyFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    [Display(Name = "Icon class")]
    public string? IconClass { get; set; }

    [StringLength(400)]
    [Display(Name = "Icon URL")]
    public string? IconUrl { get; set; }
}

public class MessageListViewModel
{
    public IReadOnlyList<ContactMessage> Messages { get; init; } = Array.Empty<ContactMessage>();
    public string? Filter { get; init; }   // all | unread | read
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int TotalCount { get; init; }
    public int PageSize { get; init; } = 10;
    public int UnreadCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
