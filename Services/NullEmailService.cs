namespace ZE.Services;

/// <summary>No-op email provider used until a real provider is configured.</summary>
public class NullEmailService : IEmailService
{
    private readonly ILogger<NullEmailService> _logger;

    public NullEmailService(ILogger<NullEmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email provider not configured. Skipped mail to {Recipient}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
