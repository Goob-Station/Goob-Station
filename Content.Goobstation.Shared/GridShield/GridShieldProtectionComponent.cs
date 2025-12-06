using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.GridShield;

/// <summary>
/// Allows this entity to be shielded by a grid shield.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GridShieldProtectionComponent : Component
{
    [DataField]
    public EntProtoId? HitEffect = "ShieldTriggeredEffect";

    /// <summary>
    /// If specified, will try to search for space tiles in the
    /// specified radius, and if not found, it will not be shielded.
    /// </summary>
    [DataField("searchRadius")]
    public float? SpaceSearchRadius;
}
