using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Used to chase the target. Temporary replacement for lack of movement AI in LHTN.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaChaseTargetComponent : Component
{
    /// <summary>
    /// Who the target is.
    /// </summary>
    [DataField]
    public EntityUid? Target;

    /// <summary>
    /// Which components to chase after.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Which components to ignore.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// The speed at which the mob will move towards the target.
    /// </summary>
    [DataField]
    public float MovementSpeed;

    /// <summary>
    /// When true, the mob will chase, when false, the mob will stop.
    /// </summary>
    [DataField]
    public bool ChaseNow;

    /// <summary>
    /// Distance at which the mob should stop chasing the target.
    /// </summary>
    [DataField]
    public float StopDistance;

    /// <summary>
    /// If the mob should stop chasing at all when near.
    /// </summary>
    [DataField]
    public bool StopWhenNear;

}
