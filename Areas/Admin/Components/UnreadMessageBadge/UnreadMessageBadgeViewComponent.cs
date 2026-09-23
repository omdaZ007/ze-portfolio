using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZE.Data;

namespace ZE.Areas.Admin.Components;

public class UnreadMessageBadgeViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;
    public UnreadMessageBadgeViewComponent(ApplicationDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        int unread = await _db.ContactMessages.CountAsync(m => !m.IsRead, cancellationToken);
        if (unread == 0) return Content(string.Empty);
        return View(unread);
    }
}
