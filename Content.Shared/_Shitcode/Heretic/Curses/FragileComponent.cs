using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Curses;

[RegisterComponent, NetworkedComponent]
public sealed partial class FragileComponent : Component
{
    [DataField]
    public DamageModifierSet ModifierSet = new()
    {
        Coefficients =
        {
            {"Blunt", 2},
            {"Slash", 2},
            {"Piercing", 2},
            {"Heat", 2},
            {"Cold", 2},
            {"Shock", 2},
            {"Asphyxiation", 2},
            {"Bloodloss", 2},
            {"Caustic", 2},
            {"Poison", 2},
            {"Radiation", 2},
            {"Cellular", 2},
        },
        IgnoreArmorPierceFlags = (int) PartialArmorPierceFlags.All,
    };
}
