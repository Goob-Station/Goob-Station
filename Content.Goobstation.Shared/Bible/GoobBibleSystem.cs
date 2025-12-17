// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Exorcism;
using Content.Goobstation.Shared.Religion;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

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
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    public bool TryDoSmite(EntityUid bible, EntityUid performer, EntityUid target, UseDelayComponent? useDelay = null, BibleComponent? bibleComp = null)
    {
        if (!Resolve(bible, ref useDelay, ref bibleComp))
            return false;

        if (!TryComp<WeakToHolyComponent>(target, out var weakToHoly)
            || weakToHoly is {AlwaysTakeHoly: false}
            || !HasComp<BibleUserComponent>(performer)
            || !_timing.IsFirstTimePredicted
            || _delay.IsDelayed(bible)
            || !_netManager.IsServer)
            return false;

        var multiplier = 1f;
        var isDevil = false;

        if (TryComp<DevilComponent>(target, out var devil))
        {
            isDevil = true;
            multiplier = devil.BibleUserDamageMultiplier;
        }

        if (!_mobStateSystem.IsIncapacitated(target))
        {
            var popup = Loc.GetString("weaktoholy-component-bible-sizzle", ("target", target), ("item", bible));
            _popupSystem.PopupPredicted(popup, target, performer, PopupType.LargeCaution);
            _audio.PlayPvs(bibleComp.SizzleSoundPath, target);
            _damageableSystem.TryChangeDamage(target, bibleComp.SmiteDamage * multiplier, true, origin: bible, targetPart: TargetBodyPart.All, ignoreBlockers: true);
            _stun.TryParalyze(target, bibleComp.SmiteStunDuration * multiplier, false);
            _delay.TryResetDelay((bible, useDelay));
        }
        else if (isDevil && HasComp<BibleUserComponent>(performer))
        {
            var doAfterArgs = new DoAfterArgs(
                EntityManager,
                performer,
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
            var popup = Loc.GetString("devil-banish-begin", ("target", target), ("user", performer));
            _popupSystem.PopupEntity(popup, target, PopupType.LargeCaution);
        }

        return true;
    }
}

/// <summary>
/// Raised on the target once bible smite gets used
/// </summary>
[ByRefEvent]
public record struct BibleSmiteUsed;
