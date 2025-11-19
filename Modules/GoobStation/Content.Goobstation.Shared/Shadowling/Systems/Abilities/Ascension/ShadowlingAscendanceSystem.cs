// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This is the ascendance ability.
/// The ascendance ability only forms the Ascension Egg.
/// Other info about the Ascension Egg exists in its own system.
/// </summary>
public sealed class ShadowlingAscendanceSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscendanceComponent, AscendanceEvent>(OnAscendance);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, AscendanceDoAfterEvent>(OnAscendanceDoAfter);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingAscendanceComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingAscendanceComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnAscendance(EntityUid uid, ShadowlingAscendanceComponent component, AscendanceEvent args)
    {
        if (args.Handled)
            return;

        if (!TileFree(uid))
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-ascendance-fail"), uid, uid, PopupType.MediumCaution);
            return;
        }

        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.Duration,
            new AscendanceDoAfterEvent(),
            uid,
            used: args.Action)
        {
            BreakOnDamage = true,
            CancelDuplicate = true,
            BreakOnMove = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnAscendanceDoAfter(
        EntityUid uid,
        ShadowlingAscendanceComponent component,
        AscendanceDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        var cocoon = PredictedSpawnAtPosition(component.EggProto, Transform(uid).Coordinates);
        var ascEgg = Comp<ShadowlingAscensionEggComponent>(cocoon);
        ascEgg.Creator = uid;

        args.Handled = true;
        _actions.RemoveAction(uid, args.Args.Used);
    }

    private bool TileFree(EntityUid uid)
    {
        // Check if tile is occupied
        var mapCoords = _transformSystem.GetMapCoordinates(uid);
        if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid))
            return false;

        if (_mapSystem.GetAnchoredEntities(gridUid, grid, mapCoords).Any())
            return false;

        return true;
    }
}
