using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Weapons.Melee;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class NinjutsuSystem
{
    private void ResetDebuff(EntityUid uid)
    {
        _status.TryAddStatusEffect(uid, NinjutsuLossOfSurprise, out var effect, TimeSpan.FromSeconds(5));
        _goobStealth.TryRevealNinja(uid);
    }

    private bool IsWeaponValid(EntityUid user, EntityUid weapon, [NotNullWhen(true)] out MeleeWeaponComponent? melee)
    {
        if (!TryComp(weapon, out melee))
            return false;

        return user == weapon || melee.Damage.DamageDict.ContainsKey("Slash");
    }

    private float GetDamageMultiplier(EntityUid uid)
    {
        if (HasLossOfSurprise(uid) ||
            !TryComp(uid, out NinjutsuSneakAttackComponent? sneakAttack))
            return 1f;

        return sneakAttack.Multiplier;
    }
}
