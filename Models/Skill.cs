using System.ComponentModel.DataAnnotations;

namespace ZE.Models;

public class Skill
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Font Awesome class (e.g. "fa-brands fa-html5") or an image path (/images/icon-html5.png).</summary>
    [Required, StringLength(300)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Category { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
