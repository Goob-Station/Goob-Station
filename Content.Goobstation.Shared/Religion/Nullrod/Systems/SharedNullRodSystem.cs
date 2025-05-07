// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
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
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Religion.Nullrod.Systems;

public abstract partial class SharedNullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullrodComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<NullrodComponent, ShotAttemptedEvent>(OnShootAttempt);

        SubscribeLocalEvent<NullrodComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<NullrodComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
    }

    #region Attack Attempts
    private void OnAttackAttempt(Entity<NullrodComponent> ent, ref AttackAttemptEvent args)
    {
        if (!ent.Comp.UntrainedUseRestriction || HasComp<BibleUserComponent>(args.Uid))
            return;

        args.Cancel();
        UntrainedDamageAndPopup(ent, args.Uid);
    }

    private void OnShootAttempt(Entity<NullrodComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!ent.Comp.UntrainedUseRestriction || HasComp<BibleUserComponent>(args.User))
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
    #endregion

    #region Add Component Methods

    private void OnGotEquippedHand(EntityUid uid, NullrodComponent comp, GotEquippedHandEvent args)
    {
        if (comp.Components.Count == 0) return;

        if (comp.Components.Count > 1)
        {
            Logger.Error("Although a component registry supports multiple components, we cannot bookkeep more than 1 component for ClothingGrantComponent at this time.");
            return;
        }

        foreach (var (name, data) in comp.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            if (HasComp(args.User, newComp.GetType()))
                continue;

            newComp.Owner = args.User;

            var temp = (object) newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(args.User, (Component)temp!);
        }
    }

    private void OnGotUnequippedHand(EntityUid uid, NullrodComponent comp, GotUnequippedHandEvent args)
    {
        if (comp.Components.Count == 0) return;

        foreach (var (name, data) in comp.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            RemComp(args.User, newComp.GetType());
        }
    }

    #endregion

}
