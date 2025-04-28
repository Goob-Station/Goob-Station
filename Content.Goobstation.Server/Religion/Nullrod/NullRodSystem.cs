// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Goobstation.Shared.Religion.Nullrod.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Religion.Nullrod;

public sealed partial class NullRodSystem : SharedNullRodSystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullrodComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<NullrodComponent, NullrodPrayDoAfterEvent>(OnPrayDoAfter);
    }
    private void OnGetVerbs(Entity<NullrodComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !HasComp<BibleUserComponent>(args.User))
            return;

        var user = args.User;

        InteractionVerb prayVerb = new()
        {
            Act = () =>
            {
                StartPrayDoAfter(user, ent, ent.Comp);
            },
            Text = Loc.GetString("nullrod-pray-prompt"),
            Icon = new SpriteSpecifier.Rsi(new("_ShitChap/Objects/Weapons/Nullrod/nullrod.rsi"), "icon"),
        };

        args.Verbs.Add(prayVerb);
    }

    #region Doafter
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
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            NeedHand = true,
            BlockDuplicate = true,
            MultiplyDelay = false,
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnPrayDoAfter(EntityUid uid, NullrodComponent comp, ref NullrodPrayDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !args.User.IsValid())
            return;

        var ev = new NullrodPrayEvent(args.User, comp);
        RaiseLocalEvent(uid, ref ev);

        args.Repeat = comp.RepeatPrayer;
    }

    #endregion

}
