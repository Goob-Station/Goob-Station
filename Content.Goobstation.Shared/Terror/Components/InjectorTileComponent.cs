using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Injects a configurable reagent into anyone who steps on this tile.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InjectorTileComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> ReagentId;

    [DataField]
    public FixedPoint2 Amount = FixedPoint2.New(1);
}
