// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.DoAfter;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Religion.AlternatePrayable;

public sealed partial class AlternatePrayableSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlternatePrayableComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<AlternatePrayableComponent, AlternatePrayDoAfterEvent>(OnPrayDoAfter);
    }
    private void OnGetVerbs(Entity<AlternatePrayableComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || args.Using is not { } item
            || !TryComp<ItemComponent>(item, out var itemComp))
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
            Text = Loc.GetString("alternate-pray-prompt", ("item",item)),
            Icon = new SpriteSpecifier.Rsi(new ResPath("Objects/Specific/Chapel/bible.rsi"), "icon"),
            Priority = 30,
        };

        args.Verbs.Add(prayVerb);
    }

    #region Doafter
    private void StartPrayDoAfter(EntityUid user, EntityUid nullRod, AlternatePrayableComponent comp)
    {
        if (_timing.CurTime > comp.NextPopup)
        {
            var popup = Loc.GetString("alternate-pray-start", ("user", Name(user)), ("item", Name(nullRod)));
            _popupSystem.PopupPredicted(popup, user, user);

            comp.NextPopup = _timing.CurTime + comp.PopupDelay;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            comp.PrayDoAfterDuration,
            new AlternatePrayDoAfterEvent(),
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

    private void OnPrayDoAfter(EntityUid uid, AlternatePrayableComponent comp, ref AlternatePrayDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || TerminatingOrDeleted(args.User))
            return;

        var ev = new AlternatePrayEvent(args.User);
        RaiseLocalEvent(uid, ref ev);

        args.Repeat = comp.RepeatPrayer;
    }

    #endregion

}
