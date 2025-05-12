// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Religion.AlternatePrayable;

public sealed partial class NullRodSystem : SharedNullRodSystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlternatePrayableComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<AlternatePrayableComponent, NullrodPrayDoAfterEvent>(OnPrayDoAfter);
    }
    private void OnGetVerbs(Entity<AlternatePrayableComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (ent.Comp.RequireBibleUser && !HasComp<BibleUserComponent>(args.User))
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
    private void StartPrayDoAfter(EntityUid user, EntityUid nullRod, AlternatePrayableComponent comp)
    {
        if (_timing.CurTime > comp.NextPopup)
        {
            var popup = Loc.GetString("nullrod-pray-start", ("user", Name(user)), ("nullrod", Name(nullRod)));
            _popupSystem.PopupEntity(popup, user);

            comp.NextPopup = _timing.CurTime + comp.PopupDelay;
        }

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

    private void OnPrayDoAfter(EntityUid uid, AlternatePrayableComponent comp, ref NullrodPrayDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || TerminatingOrDeleted(args.User))
            return;

        var ev = new NullrodPrayEvent(args.User);
        RaiseLocalEvent(uid, ref ev);

        args.Repeat = comp.RepeatPrayer;
    }

    #endregion

}
