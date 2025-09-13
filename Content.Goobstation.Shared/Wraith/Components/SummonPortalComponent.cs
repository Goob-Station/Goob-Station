using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonPortalComponent : Component
{
    /// <summary>
    /// How many portals are currently active.
    /// </summary>
    [DataField]
    public int CurrentActivePortals;

    /// <summary>
    /// How many portals the wraith is allowed to have.
    /// </summary>
    [DataField]
    public int PortalLimit = 1;

    /// <summary>
    /// The range of the WP regeneration boost for the wraith, so long as it stays near the portal.
    /// </summary>
    [DataField]
    public float PortalRange = 10f;
}
