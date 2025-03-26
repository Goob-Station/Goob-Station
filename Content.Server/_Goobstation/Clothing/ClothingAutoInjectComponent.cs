using Content.Shared._Goobstation.Clothing;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Clothing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

public sealed partial class ClothingAutoInjectComponent : Component
{
    [DataField(required: true)]
    public ProtoId<AutoInjectorPrototype> Proto;

    [DataField]
    public EntProtoId Action = "ActionActivateAutoinjector";
}
