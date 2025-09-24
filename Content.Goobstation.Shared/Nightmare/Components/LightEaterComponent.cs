using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Nightmare.Components;

/// <summary>
/// This is used for the nightmare armblade
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightEaterComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionLightEater";

    [DataField]
    public EntityUid? ActionEnt;
}
