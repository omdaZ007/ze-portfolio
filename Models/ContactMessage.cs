using System.ComponentModel.DataAnnotations;

namespace ZE.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ProjectType { get; set; }

    [Required, StringLength(4000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }
}
