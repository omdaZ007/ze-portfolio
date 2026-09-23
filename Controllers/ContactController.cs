using Microsoft.AspNetCore.Mvc;
using ZE.Data;
using ZE.Models;
using ZE.Services;
using ZE.ViewModels;

namespace ZE.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _email;

    public ContactController(ApplicationDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    [HttpGet("/contact")]
    public IActionResult Index() => View(new ContactFormViewModel());

    [HttpPost("/contact/send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(ContactFormViewModel form, string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return BadRequest(new { errors = GetErrors() });

            return View(nameof(Index), form);
        }

        var message = new ContactMessage
        {
            Name = form.Name.Trim(),
            Email = form.Email.Trim(),
            ProjectType = form.ProjectType,
            Message = form.Message.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        // Email delivery is intentionally optional; swap NullEmailService in DI for a real provider.
        await _email.SendAsync("hello@ze.dev", $"New inquiry from {message.Name}", message.Message,
            cancellationToken);

        TempData["Success"] = "Thanks! Your message landed in our inbox — we'll get back to you soon.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    private Dictionary<string, string[]> GetErrors() =>
        ModelState.ToDictionary(
            kv => kv.Key,
            kv => kv.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());
}
