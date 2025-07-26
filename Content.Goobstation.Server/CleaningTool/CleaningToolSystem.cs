// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Footprints;
using Content.Goobstation.Shared.CleaningTool;
using Content.Server.Decals;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Tiles;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Server.CleaningTool;

public sealed class CleaningToolSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly DecalSystem _decal = default!;
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
            foundDecals =_decal.GetDecalsIntersecting(tileRef.GridUid, _lookup.GetLocalBounds(tileRef, mapgrid.TileSize).Enlarged(0.5f).Translated(new Vector2(-0.5f, -0.5f)));
        }

        foundEntities.RemoveWhere(ent =>
            !_interaction.InRangeUnobstructed(user, ent, cleaningTool.Comp.Radius)
            || !HasComp<FootprintComponent>(ent));

        foundDecals.RemoveWhere(decal =>
            !decal.Decal.Cleanable);

        if (foundEntities.Count == 0
            && foundDecals.Count == 0)
            return;

        args.Handled = TryStartCleaning(cleaningTool, args.User, foundEntities, foundDecals);
    }

    private bool TryStartCleaning(Entity<CleaningToolComponent> cleaningTool,
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

        _popup.PopupEntity(Loc.GetString("cleaning-tool-scrubbing-start", ("user", user)), user);
        return _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<CleaningToolComponent> cleaningTool, ref CleaningToolDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        foreach (var ent in GetEntityList(args.Entities))
        {
            Spawn(cleaningTool.Comp.SparkleProto, Transform(ent).Coordinates);
            Del(ent);
        }

        foreach (var (index, _) in args.Decals)
        {
            var gridNullable = Transform(cleaningTool).GridUid;

            if (gridNullable is {} grid)
                _decal.RemoveDecal(grid, index);
        }

    }
}
