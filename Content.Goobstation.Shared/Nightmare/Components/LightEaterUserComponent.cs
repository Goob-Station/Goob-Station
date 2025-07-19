using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Nightmare.Components;

/// <summary>
/// This is used for indicating that the user owns this action
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightEaterUserComponent : Component
{
    [DataField]
    public EntProtoId LightEaterProto = "LightEaterArmBlade";

    [DataField]
    public bool Activated;

    [DataField]
    public EntityUid? LightEaterEntity;
}
