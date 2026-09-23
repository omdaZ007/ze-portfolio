using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ZE.Areas.Admin.Controllers;

/// <summary>Base controller for the entire Admin area: area route + Admin role required.</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/[controller]/{action=Index}")]
public abstract class AdminControllerBase : Controller
{
    /// <summary>Creates success/error flash messages rendered by the admin layout.</summary>
    protected void Success(string message) => TempData["Success"] = message;
    protected void Error(string message) => TempData["Error"] = message;
}
