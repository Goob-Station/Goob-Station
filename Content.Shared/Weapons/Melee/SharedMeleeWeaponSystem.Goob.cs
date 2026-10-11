using Content.Goobstation.Common.Weapons;
using Content.Shared.Damage;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Melee;

public abstract partial class SharedMeleeWeaponSystem
{
    private void AddArmorPenetrationExamine(FormattedMessage message, DamageSpecifier damage)
    {
        var ap = (int) Math.Round(damage.ArmorPenetration * 100);
        if (ap == 0)
            return;

        var abs = Math.Abs(ap);
        message.AddMarkupPermissive("\n" + Loc.GetString("armor-penetration", ("arg", ap / abs), ("abs", abs)));
    }

    private bool IsHeavyAttackTargetBlocked(EntityUid target, EntityUid user, EntityUid weapon)
    {
        var ev = new HeavyAttackTargetAttemptEvent(user, weapon);
        RaiseLocalEvent(target, ref ev);
        return ev.Cancelled;
    }
}
