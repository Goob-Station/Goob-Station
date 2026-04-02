using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Enviroment;

[RegisterComponent]
public sealed partial class ToxicGasDamageComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public float Interval = 1.0f;

    [DataField]
    public float Accumulator;
}
