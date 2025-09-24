using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Server.Damage;

/// <summary>
/// This is used for prevents damage above a set amount.
/// </summary>
[RegisterComponent]
public sealed partial class PreventDamageComponent : Component
{
    [DataField]
    public  FixedPoint2 MaxDamage = FixedPoint2.New(190);
}
