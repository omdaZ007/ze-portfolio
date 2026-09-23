namespace ZE.Services;

/// <summary>
/// Outgoing mail abstraction. The contact form persists messages to the database only;
/// plug in a real provider (SendGrid, SMTP, Azure Communication Services) later by
/// replacing <see cref="NullEmailService"/> in DI — no other code changes required.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
