using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PoisonWebComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> ReagentId;

    [DataField(required: true)]
    public FixedPoint2 ReagentAmount = FixedPoint2.New(1);
}
