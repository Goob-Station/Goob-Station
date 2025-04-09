// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Religion.Nullrod;

public sealed partial class NullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<NullrodComponent, AttemptShootEvent>(OnShootAttempt);
    }

    private void OnMeleeHitEvent(EntityUid uid, NullrodComponent comp, MeleeHitEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.DamageOnUntrainedUse, false, origin: uid) == null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(comp.UntrainedUseString), args.User, args.User, PopupType.MediumCaution);

        _audio.PlayPvs(comp.UntrainedUseSound, args.User);
        args.Handled = true;
    }

    private void OnShootAttempt(EntityUid uid, NullrodComponent comp, ref AttemptShootEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.DamageOnUntrainedUse, false, origin: uid) == null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(comp.UntrainedUseString), args.User, args.User, PopupType.MediumCaution);
        _audio.PlayPvs(comp.UntrainedUseSound, args.User);

        args.Cancelled = true;
    }
}
