using Content.Shared.Chemistry.Reagent;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Mutations.MassMushroomOrganism;

[RegisterComponent]
public sealed partial class SporeDivisionComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionSporeDivision";

    [DataField]
    public ProtoId<ReagentPrototype> Reagent = "Nutriment";

    [DataField]
    public float ReagentAmount = 5f;

    [DataField]
    public float DoAfterLength = 5f;

    [DataField]
    public float HungerCost = 5f;

    [DataField]
    public HungerThreshold MinHungerThreshold = HungerThreshold.Okay;
}
