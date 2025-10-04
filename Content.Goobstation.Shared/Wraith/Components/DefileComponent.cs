using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DefileComponent : Component
{
    /// <summary>
    /// Dictionary of reagents and their quantities to be injected.
    /// Key: Reagent ID, Value: Quantity to inject.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();
}
