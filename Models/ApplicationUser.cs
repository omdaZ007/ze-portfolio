using Microsoft.AspNetCore.Identity;

namespace ZE.Models;

/// <summary>Identity user for the ZE admin area.</summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
