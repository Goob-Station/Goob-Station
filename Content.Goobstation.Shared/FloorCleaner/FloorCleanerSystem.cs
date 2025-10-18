// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Footprints;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Content.Shared.Fluids;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Shared.FloorCleaner;

public sealed class FloorCleanerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedDecalSystem _decal = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAbsorbentSystem _absorbent = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FloorCleanerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<FloorCleanerComponent, FloorCleanerDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<FloorCleanerComponent> floorCleaner, ref AfterInteractEvent args)
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
            floorCleaner.Comp.Radius,
            foundEntities);

        if (TryComp<MapGridComponent>(gridUid, out var mapgrid))
        {
            var tileRef =  _map.GetTileRef(gridUid.Value, mapgrid, args.ClickLocation);
            foundDecals = _decal.GetDecalsInRange(tileRef.GridUid, tileRef.GridIndices, floorCleaner.Comp.Radius);
        }

        foundEntities.RemoveWhere(ent =>
            !_interaction.InRangeUnobstructed(user, ent, floorCleaner.Comp.Radius)
            || !HasComp<FootprintComponent>(ent)); // Otherwise you can mop people :P

        foundDecals.RemoveWhere(decal =>
            !decal.Decal.Cleanable);

        if (foundEntities.Count == 0
            && foundDecals.Count == 0)
            return;

        args.Handled = TryStartCleaning(floorCleaner, args.User, foundEntities, foundDecals);
    }

    private bool TryStartCleaning(
        Entity<FloorCleanerComponent> floorCleaner,
        EntityUid user,
        HashSet<EntityUid> targets,
        HashSet<(uint Index, Decal Decal)> decals)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            floorCleaner.Comp.CleanDelay,
            new FloorCleanerDoAfterEvent(GetNetEntityList(targets), decals),
            floorCleaner,
            used: floorCleaner)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
        };

        _popup.PopupClient(Loc.GetString("cleaning-tool-scrubbing-start", ("user", user)), user);
        return _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<FloorCleanerComponent> floorCleaner, ref FloorCleanerDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        foreach (var ent in GetEntityList(args.Entities))
        {
            if (!StartCleaning(floorCleaner.Owner, ent))
                continue;
            PredictedQueueDel(ent);
        }

        foreach (var (index, _) in args.Decals)
        {
            var gridNullable = Transform(floorCleaner).GridUid;

            if (gridNullable is {} grid)
                _decal.RemoveDecal(grid, index);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="uid"> cleaning item</param>
    /// <param name="target"> target puddle</param>
    /// <returns> returns false if failed to run</returns>
    private bool StartCleaning(EntityUid uid, EntityUid target)
    {
        if(!TryComp<AbsorbentComponent>(uid, out var absorb))
            return false;
        if (!TryComp<UseDelayComponent>(uid, out var useDelay))
            return false;
        if (!_solutionContainer.TryGetSolution(uid, absorb.SolutionName, out var absorberSoln))
            return false;

        if (FixedPoint2.Zero ==
            absorberSoln.Value.Comp.Solution.GetTotalPrototypeQuantity(
                _puddle.GetAbsorbentReagents(absorberSoln.Value.Comp.Solution)))// no cleaning reagent in scrubber
            return true;

        _absorbent.Mop((uid,absorb), uid, target );
        _useDelay.CancelDelay((uid, useDelay)); // prevents cleaning loop from being aborted

        return true;
    }
}
