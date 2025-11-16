using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for Destroy Engines ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingDestroyEnginesComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionDestroyEngines";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// Indicates how long the shuttle wil be delayed for.
    /// </summary>
    [DataField]
    public TimeSpan DelayTime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Indicates whether the ability has already been used before.
    /// </summary>
    [DataField]
    public bool HasBeenUsed;
}
