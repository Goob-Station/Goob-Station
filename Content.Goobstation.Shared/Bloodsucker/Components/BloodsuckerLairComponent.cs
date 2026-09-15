using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Marks a coffin as claimed by a specific bloodsucker.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerLairComponent : Component
{
    /// <summary>
    /// The bloodsucker who owns this coffin.
    /// </summary>
    [DataField]
    public EntityUid Owner;
}

/// <summary>
/// Tracks the bloodsucker's claimed coffin.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerClaimedCoffinComponent : Component
{
    [DataField]
    public EntityUid Coffin;

    /// <summary>
    /// Radius around the coffin considered "inside the lair".
    /// </summary>
    [DataField]
    public float LairRadius = 10f;
}

/// <summary>
/// Marks this entity as a bloodsucker vassal rack.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerVassalRackComponent : Component;
