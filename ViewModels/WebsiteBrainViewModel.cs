namespace ZE.ViewModels;

/// <summary>Read model for the "How It Thinks" (Website Brain) section.</summary>
public class WebsiteBrainViewModel
{
    public IReadOnlyList<BrainNodeViewModel> Nodes { get; init; } = Array.Empty<BrainNodeViewModel>();
}

/// <summary>One system inside the abstract brain network.</summary>
public class BrainNodeViewModel
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    /// <summary>Normalized position on the brain canvas (0..1 → x).</summary>
    public double X { get; init; }

    /// <summary>Normalized position on the brain canvas (0..1 → y).</summary>
    public double Y { get; init; }

    public IReadOnlyList<string> Technologies { get; init; } = Array.Empty<string>();
}
