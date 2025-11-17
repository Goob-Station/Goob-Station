using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShapeshiftActionComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<PolymorphPrototype>> Polymorphs = new();

    [DataField]
    public LocId Speech = "heretic-speech-shapeshft";
}
