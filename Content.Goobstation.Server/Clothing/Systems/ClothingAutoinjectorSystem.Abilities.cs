// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Shared._Goobstation.Clothing;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Clothing.Systems;

/// <summary>
/// This can be used for modsuit modules in the future.
/// Currently, it allows you to have an entity inject regeants into itself, defined by a prototype.
/// </summary>
public sealed partial class ClothingAutoinjectorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingAutoInjectComponent, ActionActivateAutoInjectorEvent>(OnInjectorActivated);
        SubscribeLocalEvent<ClothingAutoInjectComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<ClothingAutoInjectComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInjectorActivated(EntityUid uid, ClothingAutoInjectComponent component, ref ActionActivateAutoInjectorEvent args)
    {
        if (args.Handled)
            return;

        if (!_proto.TryIndex(component.Proto, out var proto))
            return;

        if (_timing.CurTime < component.NextAvailableTime)
        {
            var time = component.NextAvailableTime - _timing.CurTime;
            _popup.PopupEntity(Loc.GetString(component.FailPopup, ("time", time.TotalSeconds)), args.Performer, args.Performer);
            args.Handled = true;
            return;
        }

        if (!TryInjectReagents(args.Performer, proto.Reagents))
            return;

        component.NextAvailableTime = _timing.CurTime + proto.Cooldown;

        _audio.PlayPvs(component.InjectSound, uid);
        _popup.PopupEntity(Loc.GetString("autoinjector-injection-hardsuit"), uid, uid);
        args.Handled = true;
    }

    private bool TryInjectReagents(EntityUid uid, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);

        if (!_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            return false;

        if (!_solution.TryAddSolution(targetSolution.Value, solution))
            return false;

        return true;
    }

    private void OnCompInit(Entity<ClothingAutoInjectComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionEntity = _actions.AddAction(ent.Owner, ent.Comp.Action);
    }

    private void OnShutdown(Entity<ClothingAutoInjectComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }
}
