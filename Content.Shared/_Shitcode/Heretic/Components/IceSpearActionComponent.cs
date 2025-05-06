using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class IceSpearActionComponent : Component
{
    [DataField]
    public EntityUid? CreatedSpear;

    [DataField]
    public EntProtoId SpearProto = "SpearIceHeretic";
}
