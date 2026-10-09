using Content.Goobstation.Common.Weapons;
using Content.Shared._ES.Camera;
using Content.Shared.Damage;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Melee;

public abstract partial class SharedMeleeWeaponSystem
{
    [Dependency] private ESScreenshakeSystem _shake = default!;

    private static readonly ESScreenshakeParameters MeleeUserShake = new(0.08f, 1.0f, 0.009f);
    private static readonly ESScreenshakeParameters MeleeTargetShake = new(0.45f, 1.1f, 0.04f);

    private void DoMeleeScreenshake(EntityUid user, List<EntityUid> targets)
    {
        _shake.Screenshake(user, null, MeleeUserShake);
        foreach (var target in targets)
        {
            _shake.Screenshake(target, MeleeTargetShake, null);
        }
    }

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
