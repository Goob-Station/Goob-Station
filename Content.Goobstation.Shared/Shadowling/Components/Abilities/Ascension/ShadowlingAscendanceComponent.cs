using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for Ascendance debug
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscendanceComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(20);

    [DataField]
    public EntProtoId EggProto = "SlingEggAscension";
}
