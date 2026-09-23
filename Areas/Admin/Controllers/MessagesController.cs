using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;
using ZE.ViewModels;

namespace ZE.Areas.Admin.Controllers;

public class MessagesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;

    public MessagesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? filter, string? q, int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        var query = _db.ContactMessages.AsNoTracking().AsQueryable();

        query = filter?.ToLowerInvariant() switch
        {
            "unread" => query.Where(m => !m.IsRead),
            "read" => query.Where(m => m.IsRead),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(m => m.Name.Contains(term) || m.Email.Contains(term) ||
                                     m.Message.Contains(term) ||
                                     (m.ProjectType != null && m.ProjectType.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);

        var model = new MessageListViewModel
        {
            Messages = await query.OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToListAsync(cancellationToken),
            Filter = filter,
            Query = q,
            Page = page,
            TotalCount = total,
            UnreadCount = await _db.ContactMessages.CountAsync(m => !m.IsRead, cancellationToken)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var message = await _db.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message is null) return NotFound();

        if (!message.IsRead)
        {
            message.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return View(message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, bool isRead = true, CancellationToken cancellationToken = default)
    {
        var message = await _db.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message is null) return NotFound();

        message.IsRead = isRead;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Info"] = isRead ? "Marked as read." : "Marked as unread.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var message = await _db.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message is null) return NotFound();

        _db.ContactMessages.Remove(message);
        await _db.SaveChangesAsync(cancellationToken);
        Success("Message deleted.");
        return RedirectToAction(nameof(Index));
    }
}
