using System.ComponentModel.DataAnnotations;

namespace ZE.Models;

public class Service
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Font Awesome class or an image path.</summary>
    [Required, StringLength(300)]
    public string Icon { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
