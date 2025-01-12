using Robust.Shared.Containers;

namespace Content.Shared.VentCrawler.Tube.Components;

/// <summary>
/// A component representing a vent that you can crawl through
/// </summary>
[RegisterComponent]
public sealed partial class VentCrawlerTubeComponent : Component
{
    [DataField("containerId")]
    public string ContainerId { get; set; } = "VentCrawlerTube";

    [DataField("connected")]
    public bool Connected;

    [ViewVariables]
    public Container Contents { get; set; } = null!;
}

[ByRefEvent]
public record struct GetVentCrawlingsConnectableDirectionsEvent
{
    public Direction[] Connectable;
}
