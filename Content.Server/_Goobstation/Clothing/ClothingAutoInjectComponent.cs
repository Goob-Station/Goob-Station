using Content.Shared._Goobstation.Clothing;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Clothing;

[RegisterComponent]

public sealed partial class ClothingAutoInjectComponent : Component
{
    [DataField(required: true)]
    public ProtoId<AutoInjectorPrototype> Proto;
}
