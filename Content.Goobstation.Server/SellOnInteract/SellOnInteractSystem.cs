// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SellOnInteract;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Construction.Conditions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.DoAfter;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.SellOnInteract;

public sealed partial class SellOnInteractSystem : EntitySystem
{
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;

    private static readonly SoundPathSpecifier ApproveSound = new("/Audio/Effects/Cargo/ping.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SellOnInteractComponent, SellOnInteractDoAfter>(OnDoAfter);
        SubscribeLocalEvent<SellOnInteractComponent, GetVerbsEvent<UtilityVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, SellOnInteractComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Target == args.Using)
            return;

        if (!HasComp<ItemComponent>(args.Target) || HasComp<CargoSellBlacklistComponent>(args.Target) && !component.CanSellAnything)
            return;

        var performer = args.User;
        var target = args.Target;

        UtilityVerb verb = new()
        {
            Act = () => { StartSellDoAfter(target, component, performer); },
            Text = Loc.GetString("sell-object-verb"),
            Icon = new SpriteSpecifier.Rsi(new("Objects/Economy/cash.rsi"), "cash"),
            Priority = 2
        };

        args.Verbs.Add(verb);
    }

    private void StartSellDoAfter(EntityUid target, SellOnInteractComponent comp, EntityUid performer)
    {
        if (TerminatingOrDeleted(target))
            return;

        var popup = Loc.GetString("sell-object-popup", ("user", performer), ("target", target));
        _popup.PopupEntity(popup, performer, PopupType.Medium);

        var doAfterArgs = new DoAfterArgs(EntityManager,
            performer,
            comp.DoAfterDuration,
            new SellOnInteractDoAfter(),
            comp.Owner,
            target)
        {
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(EntityUid uid, SellOnInteractComponent comp, ref SellOnInteractDoAfter args)
    {
        if (args.Handled || args.Target == null || args.Cancelled)
            return;

        var target = (EntityUid)args.Target;

        var xform = Transform(target);

        var price = _pricing.GetPrice(target);

        var ev = new EntitySoldEvent([target]);
        RaiseLocalEvent(ref ev);

        var stackPrototype = _prototypeManager.Index<StackPrototype>(comp.CashType);
        _stack.Spawn((int) price, stackPrototype, xform.Coordinates);
        _audio.PlayPvs(ApproveSound, args.User);

        QueueDel(target);
    }
}
