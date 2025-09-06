using Robust.Shared.Map;

namespace Content.Server._White.StationEvents.Components;

[RegisterComponent]
public sealed partial class VentSpawnRuleComponent : Component
{
    /// <summary>
    /// Locations that was picked.
    /// </summary>
    [ViewVariables]
    public List<MapCoordinates>? Coords;
}
