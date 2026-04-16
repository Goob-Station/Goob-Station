using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used to inject alcohol upon stepping on this web. Ideally it should give you temporary blurry vision but this is a much simpler way of doing this.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class SlimyWebComponent : Component
{
    [DataField]
    public ProtoId<ReagentPrototype> ReagentId = "Vodka";

    [DataField]
    public FixedPoint2 AlcoholAmount = FixedPoint2.New(20);
}
