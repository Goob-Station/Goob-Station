using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Contract;
using Content.Goobstation.Shared.Exorcism;
using Content.Goobstation.Shared.Religion;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Bible;

public sealed partial class GoobBibleSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public void TryDoSmite(EntityUid uid, BibleComponent component, AfterInteractUsingEvent args, UseDelayComponent useDelay)
    {
        if (args.Target is not { } target || !HasComp<WeakToHolyComponent>(args.Target) || !HasComp<BibleUserComponent>(args.User))
            return;

        var multiplier = 1f;
        var isDevil = false;

        if (TryComp<DevilComponent>(target, out var devil))
        {
            isDevil = true;
            multiplier = devil.BibleUserDamageMultiplier;
        }

        if (!_mobStateSystem.IsIncapacitated(target))
        {
            var popup = Loc.GetString("weaktoholy-component-bible-sizzle", ("target", target), ("item", args.Used));
            _popupSystem.PopupEntity(popup, target, PopupType.LargeCaution);
            _audio.PlayPvs(component.SizzleSoundPath, args.Target.Value);

            _damageableSystem.TryChangeDamage(target, component.SmiteDamage * multiplier, true, origin: uid);
            _stun.TryParalyze(target, component.SmiteStunDuration * multiplier, false);
            _delay.TryResetDelay((args.Used, useDelay));
        }
        else if (isDevil && HasComp<BibleUserComponent>(args.User))
        {
            var doAfterArgs = new DoAfterArgs(
                EntityManager,
                args.User,
                10f,
                new ExorcismDoAfterEvent(),
                eventTarget: target,
                target: target)
            {
                BreakOnMove = true,
                NeedHand = true,
                BlockDuplicate = true,
                BreakOnDropItem = true,
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
            var popup = Loc.GetString("devil-banish-begin", ("target", target), ("user", target));
            _popupSystem.PopupEntity(popup, target, PopupType.LargeCaution);
        }
    }
}
