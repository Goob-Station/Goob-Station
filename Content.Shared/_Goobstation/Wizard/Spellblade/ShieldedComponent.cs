using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShieldedComponent : Component
{
    [DataField]
    public float Lifetime = 5f;

    [DataField]
    public bool AntiStun = true;

    [DataField]
    public DamageModifierSet Resistances = new()
        { Coefficients = new() { ["Blunt"] = 0.5f, ["Slash"] = 0.5f, ["Piercing"] = 0.5f, ["Heat"] = 0.5f } };

    [DataField]
    public SpriteSpecifier Sprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Wizard/Effects/effects.rsi"), "shield-old");
}

public enum ShieldedKey : byte
{
    Key,
}
