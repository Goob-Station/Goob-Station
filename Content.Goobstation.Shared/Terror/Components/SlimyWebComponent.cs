using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SlimyWebComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> ReagentId = "Vodka";

    [DataField]
    public FixedPoint2 AlcoholAmount = FixedPoint2.New(20);
}
