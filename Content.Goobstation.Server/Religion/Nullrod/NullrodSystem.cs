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

    private void OnMeleeHitEvent(Entity<NullrodComponent> ent, ref MeleeHitEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        UntrainedDamageAndPopup(ent, args.User);

        args.Handled = true;
    }

    private void OnShootAttempt(Entity<NullrodComponent> ent, ref AttemptShootEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        UntrainedDamageAndPopup(ent, args.User);
        args.Cancelled = true;
    }

    private void UntrainedDamageAndPopup(Entity<NullrodComponent> ent, EntityUid user)
    {
        if (_damageableSystem.TryChangeDamage(user, ent.Comp.DamageOnUntrainedUse, origin: ent) is null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(ent.Comp.UntrainedUseString), user, user, PopupType.MediumCaution);
        _audio.PlayPvs(ent.Comp.UntrainedUseSound, user);
    }

}
