using System.ComponentModel.DataAnnotations;

namespace ZE.Models;

public class Project
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(400)]
    public string? ImageUrl { get; set; }

    [StringLength(400), Url]
    public string? LiveUrl { get; set; }

    [StringLength(400), Url]
    public string? GithubUrl { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public List<ProjectTechnology> Technologies { get; set; } = new();
}
