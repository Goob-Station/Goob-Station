// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Server.DoAfter;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Religion.Nullrod;

public sealed partial class NullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeAttempt);
        SubscribeLocalEvent<NullrodComponent, AttemptShootEvent>(OnShootAttempt);
        SubscribeLocalEvent<NullrodComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<NullrodComponent, NullrodPrayDoAfterEvent>(OnPrayDoAfter);
    }

    #region Attack Attempts
    private void OnMeleeAttempt(Entity<NullrodComponent> ent, ref MeleeHitEvent args)
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

    #endregion
    private void OnGetVerbs(Entity<NullrodComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !HasComp<BibleUserComponent>(args.User))
            return;

        var user = args.User;

        InteractionVerb prayVerb = new()
        {
            Act = () => StartPrayDoAfter(user, ent, ent.Comp),
            Text = Loc.GetString("nullrod-pray-prompt"),
            Icon = new SpriteSpecifier.Rsi(new("_ShitChap/Objects/Weapons/Nullrod/nullrod.rsi"), "icon"),
        };

        args.Verbs.Add(prayVerb);
    }

    #region Helper Methods

    private void StartPrayDoAfter(EntityUid user, EntityUid nullRod, NullrodComponent comp)
    {
        var popup = Loc.GetString("nullrod-pray-start", ("user", Name(user)), ("nullrod", Name(nullRod)));
        _popupSystem.PopupEntity(popup, user);

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            comp.PrayDoAfterDuration,
            new NullrodPrayDoAfterEvent(),
            nullRod,
            user,
            nullRod)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnPrayDoAfter(EntityUid uid, NullrodComponent comp, ref NullrodPrayDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !args.User.IsValid() || args.Used == null)
            return;

        var ev = new NullrodPrayEvent(args.User, comp, args.Used);
        RaiseLocalEvent((EntityUid)args.Used, ref ev);

        args.Repeat = comp.RepeatPrayer;
    }
    private void UntrainedDamageAndPopup(Entity<NullrodComponent> ent, EntityUid user)
    {
        if (_damageableSystem.TryChangeDamage(user, ent.Comp.DamageOnUntrainedUse, origin: ent) is null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(ent.Comp.UntrainedUseString), user, user, PopupType.MediumCaution);
        _audio.PlayPvs(ent.Comp.UntrainedUseSound, user);
    }
    #endregion

}
