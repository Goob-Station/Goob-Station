// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion.Events;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Religion.Nullrod.Systems;

public sealed partial class SharedNullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullrodComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<NullrodComponent, ShotAttemptedEvent>(OnShootAttempt);

        SubscribeLocalEvent<DivineInterventionComponent, BeforeCastTouchSpellEvent>(OnTouchSpellAttempt);
    }

    #region Spell denial

    private void OnTouchSpellAttempt(Entity<DivineInterventionComponent> ent, ref BeforeCastTouchSpellEvent args)
    {
        if (HasComp<NullrodComponent>(args.Target))
        {
            args.Cancelled = true;
            SpellDenialEffectsPopup(args.Target.Value, args.Performer);
        }
    }

    #endregion

    #region Attack Attempts
    private void OnAttackAttempt(Entity<NullrodComponent> ent, ref AttackAttemptEvent args)
    {
        if (HasComp<BibleUserComponent>(args.Uid))
            return;

        args.Cancel();
        UntrainedDamageAndPopup(ent, args.Uid);
    }

    private void OnShootAttempt(Entity<NullrodComponent> ent, ref ShotAttemptedEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        args.Cancel();
        UntrainedDamageAndPopup(ent, args.User);
    }
    #endregion

    #region Helper Methods
    private void UntrainedDamageAndPopup(Entity<NullrodComponent> ent, EntityUid user)
    {
        // WHY IS EVERY ATTACK ATTEMPT EVENT SO FUCKING SCUFFED AAARGGGHHHH
        if (_timing.CurTime < ent.Comp.NextPopupTime)
            return;

        if (_damageableSystem.TryChangeDamage(user, ent.Comp.DamageOnUntrainedUse, origin: ent) is null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(ent.Comp.UntrainedUseString), user, user, PopupType.MediumCaution);
        _audio.PlayPvs(ent.Comp.UntrainedUseSound, user);

        ent.Comp.NextPopupTime = _timing.CurTime + ent.Comp.PopupCooldown;
    }

    private void SpellDenialEffectsPopup(EntityUid target, EntityUid performer)
    {
        _popupSystem.PopupEntity(Loc.GetString("nullrod-spelldenial-popup"), performer, performer, PopupType.MediumCaution);
    }
    #endregion

}
