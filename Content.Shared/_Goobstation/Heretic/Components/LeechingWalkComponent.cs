using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent]
public sealed partial class LeechingWalkComponent : Component
{
    [DataField]
    public float AscensuionMultiplier = 3f;

    [DataField]
    public DamageSpecifier ToHeal = new()
    {
        DamageDict =
        {
            {"Blunt", -2},
            {"Slash", -2},
            {"Piercing", -2},
            {"Heat", -2},
            {"Cold", -2},
            {"Shock", -2},
            {"Asphyxiation", -2},
            {"Bloodloss", -2},
            {"Caustic", -2},
            {"Poison", -2},
            {"Radiation", -2},
            {"Cellular", -2},
        },
    };

    [DataField]
    public float StaminaHeal = 10f;

    [DataField]
    public FixedPoint2 BloodHeal = 5f;

    [DataField]
    public TimeSpan StunReduction = TimeSpan.FromSeconds(1f);

    [DataField]
    public float TargetTemperature = 310f;
}
