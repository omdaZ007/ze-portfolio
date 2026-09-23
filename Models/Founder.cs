using System.ComponentModel.DataAnnotations;

namespace ZE.Models;

public class Founder
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string JobTitle { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [StringLength(400)]
    public string? ImageUrl { get; set; }

    /// <summary>CSS color used for this founder's visual accent (gold / electric blue).</summary>
    [StringLength(40)]
    public string AccentColor { get; set; } = "#F5B942";

    [StringLength(400), Url]
    public string? GithubUrl { get; set; }

    [StringLength(400), Url]
    public string? LinkedInUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
