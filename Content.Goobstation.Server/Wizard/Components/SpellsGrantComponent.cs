using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class SpellsGrantComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Granted;

    [DataField]
    public HashSet<EntProtoId> GuaranteedActions = new();

    [DataField]
    public ProtoId<WeightedRandomEntityPrototype>? RandomActions;

    [DataField]
    public float TotalWeight;

    [DataField]
    public EntProtoId? AntagProfile;
}
