using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HealingAuraComponent : Component
{
    [DataField]
    public DamageSpecifier ToHeal = new()
    {
        DamageDict =
        {
            {"Blunt", -1.5f},
            {"Slash", -1.5f},
            {"Piercing", -1.5f},
            {"Heat", -1.5f},
            {"Cold", -1.5f},
            {"Shock", -1.5f},
            {"Asphyxiation", -1.5f},
            {"Bloodloss", -1.5f},
            {"Caustic", -1.5f},
            {"Poison", -1.5f},
            {"Radiation", -1.5f},
            {"Cellular", -1.5f},
            {"Holy", -1.5f},
        },
    };

    [DataField]
    public float Range = 4f;

    [DataField]
    public float HealDelay = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator;

    [DataField]
    public FixedPoint2 PainHeal = -3f;

    [DataField]
    public FixedPoint2 BoneHeal = -3f;

    [DataField]
    public FixedPoint2 WoundHeal = -3f;

    /// <summary>
    /// Set this to 0 to disable self-heal
    /// </summary>
    [DataField]
    public float SelfHealMultiplier = 1f;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public Dictionary<string, float>? ComponentHealMultipliers;
}
