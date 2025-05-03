// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Clothing;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Server.Popups;
using Content.Shared._Goobstation.Clothing;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Robust.Server.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Clothing.Systems;

/// <summary>
/// This can be used for modsuit modules in the future.
/// Currently, it allows you to have an entity inject regeants into itself, defined by a prototype.
/// </summary>
public sealed partial class ClothingAutoinjectorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingAutoInjectComponent, ActionActivateAutoInjectorEvent>(OnInjectorActivated);
        SubscribeLocalEvent<ClothingAutoInjectComponent, GetItemActionsEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingAutoInjectComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ClothingAutoInjectComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<ClothingAutoInjectComponent, InventoryRelayedEvent<ClothingAutoInjectRelayedEvent>>(OnInject);
    }

    private void OnInjectorActivated(EntityUid uid, ClothingAutoInjectComponent component, ref ActionActivateAutoInjectorEvent args)
    {
        if (args.Handled)
            return;

        if (!TryInjectReagents(args.Performer, component.Reagents))
            return;

        _audio.PlayPvs(component.InjectSound, args.Performer);
        _popup.PopupEntity(Loc.GetString("autoinjector-injection-hardsuit"), args.Performer, args.Performer);
        args.Handled = true;
    }

    private bool TryInjectReagents(EntityUid uid, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);

        if (!_solution.TryGetInjectableSolution(uid, out var targetSolution, out _))
            return false;

        return _solution.TryAddSolution(targetSolution.Value, solution);
    }

    private void OnMobStateChange(MobStateChangedEvent args)
    {
        RaiseLocalEvent(args.Target, new ClothingAutoInjectRelayedEvent(args.Target, args.NewMobState));
    }

    private void OnInject(EntityUid uid, ClothingAutoInjectComponent comp, InventoryRelayedEvent<ClothingAutoInjectRelayedEvent> args)
    {
        if (args.Args.NewState != MobState.Critical
        || comp.NextAutoInjectTime > _timing.CurTime)
        return;

        TryInjectReagents(args.Args.Target, comp.Reagents);
        _audio.PlayPvs(comp.InjectSound, args.Args.Target);
        _popup.PopupEntity(Loc.GetString(comp.Popup), args.Args.Target, args.Args.Target);

        comp.NextAutoInjectTime = _timing.CurTime + comp.AutoInjectInterval;
    }

    private void OnEquipped(EntityUid uid, ClothingAutoInjectComponent component, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;

        if (component.AutoInjectOnAbility)
            args.AddAction(ref component.ActionEntity, component.Action);
    }

    private void OnUnequipped(EntityUid uid, ClothingAutoInjectComponent component, ref GotUnequippedEvent args)
    {
        if (component.AutoInjectOnAbility)
            _actions.RemoveProvidedActions(args.Equipee, uid);
    }

    private void OnExamined(EntityUid uid, ClothingAutoInjectComponent component, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var onMsg = component.NextAutoInjectTime < _timing.CurTime
            ? Loc.GetString("comp-autoinjector-examined-on")
            : Loc.GetString("comp-autoinjector-examined-off", ("time", Math.Floor(component.NextAutoInjectTime.TotalSeconds - _timing.CurTime.TotalSeconds)));
        args.PushMarkup(onMsg);
    }
}
