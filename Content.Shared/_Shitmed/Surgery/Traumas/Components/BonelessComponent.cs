using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BonelessComponent : Component
{
    /// <summary>
    /// The higher it is, the lower the chance to delimb.
    /// A perfectly healed bone is equivalent to 1.
    /// A destroyed bone is 0.
    /// See TraumaSystem.Process.cs RandomDismembermentTraumaChance method
    /// </summary>
    [ViewVariables, DataField]
    public FixedPoint2 BonePenalty = 1;
}
