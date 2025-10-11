using Content.Shared._White.RadialSelector;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EvolveComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> AvailableEvolutions = new();

    [DataField]
    public int CorpsesRequired = 3;

    [ViewVariables]
    public EntityUid? ActionEnt;

    [ViewVariables]
    public EntProtoId ActionId = "ActionEvolve";
}
