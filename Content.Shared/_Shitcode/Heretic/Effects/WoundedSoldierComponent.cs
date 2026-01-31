using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.Heretic.Effects;

[RegisterComponent, NetworkedComponent]
public sealed partial class WoundedSoldierComponent : Component
{
    [DataField]
    public float LifeStealMultiplier = 0.3f;

    [DataField]
    public float StaminaHealMultiplier = 0.3f;

    [DataField]
    public DamageSpecifier DamageOverTime = new()
    {
        DamageDict =
        {
            { "Heat", 3 },
        },
    };
}
