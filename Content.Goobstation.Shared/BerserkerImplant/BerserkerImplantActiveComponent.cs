using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.BerserkerImplant;

[RegisterComponent, NetworkedComponent]
public sealed partial class BerserkerImplantActiveComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float Duration = 8;

    [ViewVariables(VVAccess.ReadWrite)]
    public DamageModifierSet DamageModifier = new()
    {
        Coefficients = new()
        {
            { "Slash", 0.4f },
            { "Piercing", 0.4f },
            { "Blunt", 0.4f },
            { "Heat", 0.4f },
            { "Shock", 0.4f },
        }
    };

    [ViewVariables(VVAccess.ReadWrite)]
    public float StunModifier = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float SelfDamageModifier = 1.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier DelayedDamage = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan EndTime = TimeSpan.Zero;
}
