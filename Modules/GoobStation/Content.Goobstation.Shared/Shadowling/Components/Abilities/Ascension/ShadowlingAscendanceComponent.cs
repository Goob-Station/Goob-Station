using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for Ascendance ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscendanceComponent : Component
{
    /// <summary>
    /// Indicates how long the ability takes to complete.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(20);

    /// <summary>
    /// The prototype of the ascension egg
    /// </summary>
    [DataField]
    public EntProtoId EggProto = "SlingEggAscension";

    [DataField]
    public EntProtoId ActionId = "ActionAscendance";

    [DataField]
    public EntityUid? ActionEnt;
}
