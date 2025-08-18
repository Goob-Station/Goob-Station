// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Footprints;
using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Shared.CleaningTool;

public sealed class CleaningToolSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedDecalSystem _decal = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CleaningToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CleaningToolComponent, CleaningToolDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<CleaningToolComponent> cleaningTool, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Handled
            || args.Target != null)
            return;

        var user = args.User;
        var foundEntities = new HashSet<EntityUid>();
        var foundDecals = new HashSet<(uint Index, Decal Decal)>();
        var gridUid = _transform.GetGrid(args.ClickLocation);

        _lookup.GetEntitiesInRange(args.ClickLocation,
            cleaningTool.Comp.Radius,
            foundEntities);

        if (TryComp<MapGridComponent>(gridUid, out var mapgrid))
        {
            var tileRef =  _map.GetTileRef(gridUid.Value, mapgrid, args.ClickLocation);
            foundDecals = _decal.GetDecalsInRange(tileRef.GridUid, tileRef.GridIndices, cleaningTool.Comp.Radius);
        }

        foundEntities.RemoveWhere(ent =>
            !_interaction.InRangeUnobstructed(user, ent, cleaningTool.Comp.Radius)
            || !HasComp<FootprintComponent>(ent)); // Otherwise you can mop people :P

        foundDecals.RemoveWhere(decal =>
            !decal.Decal.Cleanable);

        if (foundEntities.Count == 0
            && foundDecals.Count == 0)
            return;

        args.Handled = TryStartCleaning(cleaningTool, args.User, foundEntities, foundDecals);
    }

    private bool TryStartCleaning(
        Entity<CleaningToolComponent> cleaningTool,
        EntityUid user,
        HashSet<EntityUid> targets,
        HashSet<(uint Index, Decal Decal)> decals)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            cleaningTool.Comp.CleanDelay,
            new CleaningToolDoAfterEvent(GetNetEntityList(targets), decals),
            cleaningTool,
            used: cleaningTool)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
        };

        _popup.PopupClient(Loc.GetString("cleaning-tool-scrubbing-start", ("user", user)), user);
        return _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<CleaningToolComponent> cleaningTool, ref CleaningToolDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        foreach (var ent in GetEntityList(args.Entities))
            TryQueueDel(ent);

        foreach (var (index, _) in args.Decals)
        {
            var gridNullable = Transform(cleaningTool).GridUid;

            if (gridNullable is {} grid)
                _decal.RemoveDecal(grid, index);
        }

    }
}
