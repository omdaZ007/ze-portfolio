using System.ComponentModel.DataAnnotations;

namespace ZE.ViewModels;

public class ContactFormViewModel
{
    [Required(ErrorMessage = "Please tell us your name.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "That email doesn't look right.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ProjectType { get; set; }

    [Required(ErrorMessage = "Please write a message.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Please write at least 10 characters.")]
    public string Message { get; set; } = string.Empty;
}

public static class ProjectTypes
{
    public static readonly string[] All =
    {
        "Business Website",
        "Web Application",
        "E-Commerce",
        "UI/UX Design",
        "Full-Stack Product",
        "Other"
    };
}
