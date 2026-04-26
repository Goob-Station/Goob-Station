using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Atmos;

namespace Content.Server._Lavaland.Damage.Components;

/// <summary>
/// This is used for prevents damage if that damage is above death threshold.
/// </summary>
[RegisterComponent]
public sealed partial class PreventDamageComponent : Component
{
    /// <summary>
    ///  the minimum point difference from crit to dead. for this to work
    /// </summary>
    [DataField]
    public  FixedPoint2 DifferensMinimum  = FixedPoint2.New(1);


    [DataField]
    public bool LockedToLavaland = true;

    [DataField]
    public float LowerBound = Atmospherics.OneAtmosphere * 0.2f;

    [DataField]
    public float UpperBound = Atmospherics.OneAtmosphere * 0.5f;

}
