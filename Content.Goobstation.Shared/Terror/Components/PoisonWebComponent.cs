using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.ComponentModel.DataAnnotations;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used to inject poison on someone upon stepping on this web.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class PoisonWebComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> ReagentId;

    [DataField, Required]
    public FixedPoint2 ReagentAmount = 1;
}
