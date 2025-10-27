using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._Lavaland.Damage;

/// <summary>
/// This is used for prevents damage if that damage is above death threshold.
/// </summary>
[RegisterComponent]
public sealed partial class PreventDamageComponent : Component
{
    /// <summary>
    ///  the minimum point diferance from crit to dead. for this to work
    /// </summary>
    [DataField]
    public  FixedPoint2 DifferensMinimum  = FixedPoint2.New(1);
}
