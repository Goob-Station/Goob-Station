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
    public EntProtoId ActionDestroyEngines = "ActionDestroyEngines";

    [DataField]
    public TimeSpan DelayTime = TimeSpan.FromMinutes(10);

    [DataField]
    public bool HasBeenUsed;
}
