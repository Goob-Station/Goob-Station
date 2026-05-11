using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Drains stamina of target, and spawns an entity under them.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EtherDrainComponent : Component
{
    /// <summary>
    /// How much stamina the action should drain.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int StaminaDrain = 25;

    /// <summary>
    /// Lookup range.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 30f;

    /// <summary>
    /// Prototype spawned in under target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Prototype = "ORTBeamWarning";
}
