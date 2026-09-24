namespace ZE.ViewModels;

/// <summary>
/// Read model for the "Digital Heartbeat" pulse visual.
/// The heartbeat is a visual representation only — no real metrics are claimed.
/// </summary>
public class DigitalHeartbeatViewModel
{
    /// <summary>Deterministic seed that shapes the waveform (rhythm/frequency).</summary>
    public int Seed { get; init; } = 11;

    /// <summary>When set, the section is tuned to a single project's detail page.</summary>
    public string? ProjectTitle { get; init; }

    public string? ProjectSlug { get; init; }

    /// <summary>Words flashed on each pulse.</summary>
    public IReadOnlyList<string> PulseWords { get; init; } =
        new[] { "Performance", "Responsive", "Fast", "Stable", "Alive" };
}
