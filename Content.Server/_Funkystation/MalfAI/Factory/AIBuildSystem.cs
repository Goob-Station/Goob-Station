// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Factory;
using Content.Shared.Silicons.StationAi;
using Content.Shared.SubFloor;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Event to request building a prototype at a specific location.
/// </summary>
public sealed class AIBuildRequestEvent : EntityEventArgs
{
    public EntityUid Requester { get; }
    public EntityCoordinates Target { get; }
    public string Prototype { get; }

    public AIBuildRequestEvent(EntityUid requester, EntityCoordinates target, string prototype)
    {
        Requester = requester;
        Target = target;
        Prototype = prototype;
    }
}

/// <summary>
/// Handles Malf AI building requests by spawning prototypes at specified locations after a DoAfter.
/// </summary>
public sealed class AIBuildSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly TimeSpan BuildDelay = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AIBuildRequestEvent>(OnBuildRequest);
        SubscribeLocalEvent<MalfAiMarkerComponent, AIBuildDoAfterEvent>(OnBuildDoAfter);
    }

    private void OnBuildRequest(AIBuildRequestEvent args)
    {
        if (!args.Target.IsValid(EntityManager) || !IsTileFree(args.Target))
            return;

        // Show the DoAfter on the AI's remote eye when possible (the brain is hidden in the core).
        var doAfterUser = args.Requester;
        var core = Transform(args.Requester).ParentUid;
        if (TryComp<StationAiCoreComponent>(core, out var coreComp) && coreComp.RemoteEntity is { } eye)
            doAfterUser = eye;

        var doAfterEvent = new AIBuildDoAfterEvent(GetNetCoordinates(args.Target), args.Prototype);
        var doAfterArgs = new DoAfterArgs(EntityManager, doAfterUser, BuildDelay, doAfterEvent, eventTarget: args.Requester)
        {
            BreakOnMove = true, // Cancel if the AI eye moves during the build
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnBuildDoAfter(Entity<MalfAiMarkerComponent> ent, ref AIBuildDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var location = GetCoordinates(args.Location);
        if (!IsTileFree(location))
            return;

        var spawned = Spawn(args.Prototype, location);

        // If this is a robotics factory, remember who built it so created borgs go to the right AI.
        if (HasComp<RoboticsFactoryGridComponent>(spawned))
        {
            var owner = EnsureComp<MalfFactoryOwnerComponent>(spawned);
            owner.Controller = ent.Owner;

            // The factory is single use: remove the build action.
            RemoveRoboticsFactoryAction(ent.Owner);
        }

        var xform = Transform(spawned);
        if (!xform.Anchored)
            _transform.AnchorEntity(spawned, xform);

        args.Handled = true;
    }

    private void RemoveRoboticsFactoryAction(EntityUid performer)
    {
        var toRemove = new List<EntityUid>();
        foreach (var action in _actions.GetActions(performer))
        {
            if (MetaData(action.Owner).EntityPrototype?.ID == "ActionMalfAiRoboticsFactory")
                toRemove.Add(action.Owner);
        }

        foreach (var id in toRemove)
            _actions.RemoveAction(performer, id);
    }

    /// <summary>
    /// Checks if a tile is free for building: floor present, no blocking anchored entities.
    /// </summary>
    private bool IsTileFree(EntityCoordinates coordinates)
    {
        if (!coordinates.IsValid(EntityManager))
            return false;

        if (!TryComp<MapGridComponent>(coordinates.EntityId, out var grid))
            return false;

        var tile = grid.TileIndicesFor(coordinates);
        if (grid.GetTileRef(tile).Tile.IsEmpty)
            return false;

        foreach (var entity in grid.GetAnchoredEntities(tile))
        {
            // Cables, pipes and wall-mounted devices don't block construction.
            if (HasComp<SubFloorHideComponent>(entity))
                continue;

            if (_tag.HasTag(entity, "WallMount"))
                continue;

            return false;
        }

        return true;
    }
}
