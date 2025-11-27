// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server._Funkystation.Factory.Components;
using Content.Shared.DoAfter;
using Content.Shared._Funkystation.Factory;
using Content.Shared._Funkystation.Factory.Components;
using Content.Shared._Funkystation.MalfAI;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Actions;
using Content.Shared.Silicons.StationAi;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared.SubFloor;
using Content.Shared.Tag;
using Content.Shared._Funkystation.Actions.Events;

namespace Content.Server._Funkystation.Factory.Systems;

/// <summary>
/// Event to request building a prototype at a specific location
/// </summary>
public sealed partial class AIBuildRequestEvent : EntityEventArgs
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
/// System that handles AI building requests by spawning prototypes at specified locations
/// </summary>
public sealed partial class AIBuildSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ISawmill Sawmill = Logger.GetSawmill("ai.build.system");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AIBuildRequestEvent>(OnBuildRequest);
        SubscribeLocalEvent<MalfunctioningAiComponent, AIBuildDoAfterEvent>(OnBuildDoAfter);
    }

    /// <summary>
    /// Handles build requests from AI entities
    /// </summary>
    private void OnBuildRequest(AIBuildRequestEvent args)
    {
        var requester = args.Requester;
        var target = args.Target;
        var prototype = args.Prototype;


        // Validate coordinates
        if (!target.IsValid(EntityManager))
        {
            Sawmill.Warning($"AIBuild: Invalid coordinates {target} for prototype '{prototype}'");
            return;
        }

        // Validate tile is free
        if (!IsTileFree(target))
        {
            Sawmill.Warning($"AIBuild: Tile at {target} is occupied, cannot build '{prototype}'");
            return;
        }

        // Start building process with DoAfter
        var doAfterEvent = new AIBuildDoAfterEvent(GetNetCoordinates(target), prototype);
        var delay = TimeSpan.FromSeconds(3.0f); // 3 second build time

        // Try to get the AI's visible eye entity (RemoteEntity) for DoAfter display
        EntityUid doAfterUser = requester;
        var aiCore = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _xform, requester);
        if (aiCore != EntityUid.Invalid &&
            TryComp<StationAiCoreComponent>(aiCore, out var coreComp) &&
            coreComp.RemoteEntity.HasValue)
        {
            doAfterUser = coreComp.RemoteEntity.Value;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, doAfterUser, delay, doAfterEvent, eventTarget: requester)
        {
            BreakOnMove = true, // Cancel if the AI eye moves during the build
            BreakOnDamage = true,
            BreakOnHandChange = false,
            CancelDuplicate = false,
            BlockDuplicate = false,
            NeedHand = false,
            Hidden = false
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
        {
            Sawmill.Warning($"AIBuild: Failed to start DoAfter for '{prototype}' build request");
        }
    }

    /// <summary>
    /// Handles completion of the build process
    /// </summary>
    private void OnBuildDoAfter(EntityUid uid, MalfunctioningAiComponent component, AIBuildDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var location = GetCoordinates(args.Location);


        if (!IsTileFree(location))
        {
            Sawmill.Warning($"AIBuild: Tile at {location} became occupied during build");
            return;
        }

        try
        {
            // Spawn the entity
            var spawned = Spawn(args.Prototype, location);

            // If this is a robotics factory grid, remember who built it so we can assign borgs later.
            var isFactory = false;
            if (HasComp<RoboticsFactoryGridComponent>(spawned))
            {
                isFactory = true;
                var owner = EnsureComp<MalfFactoryOwnerComponent>(spawned);
                owner.Controller = uid; // uid is the AI entity that received the DoAfter completion
            }

            _xform.AnchorEntity(spawned);

            // On success, remove the Robotics Factory action from the Malf AI that built it.
            if (isFactory)
                RemoveRoboticsFactoryAction(uid);
        }
        catch (Exception ex)
        {
            Sawmill.Error($"AIBuild: Failed to spawn '{args.Prototype}' at {location}: {ex}");
        }
    }

    private void RemoveRoboticsFactoryAction(EntityUid performer)
    {
        // Remove the Robotics Factory action (ActionMalfAiRoboticsFactory) from the performer.
        // We search via ActionsComponent -> BaseActionComponent.BaseEvent type.
        if (!TryComp<ActionsComponent>(performer, out var actionsComp))
            return;

        var toRemove = new HashSet<Entity<ActionComponent>>();
        foreach (var action in _actions.GetActions(performer, actionsComp))
        {
            var baseEvent = _actions.GetEvent(action.Owner);

            if (baseEvent is not null
                && baseEvent is MalfAiRoboticsFactoryActionEvent)
                toRemove.Add(action);
        }

        foreach (var action in toRemove)
            _actions.RemoveAction(action.AsNullable());
    }

    /// <summary>
    /// Checks if a tile is free for building
    /// </summary>
    private bool IsTileFree(EntityCoordinates coordinates)
    {
        if (!coordinates.IsValid(EntityManager))
            return false;

        if (!TryComp<MapGridComponent>(coordinates.EntityId, out var gridComp))
            return false;

        var grid = new Entity<MapGridComponent>(coordinates.EntityId, gridComp);

        var tile = _map.TileIndicesFor(grid, coordinates);
        var tileRef = _map.GetTileRef(grid, tile);

        // Check if the tile exists and is not empty space
        if (tileRef.Tile.IsEmpty)
            return false;

        // Check for anchored entities, but allow building on subfloor and wall-mounted entities
        foreach (var entity in _map.GetAnchoredEntities(grid, tile))
        {
            // Allow building over entities with SubFloorHideComponent (cables, pipes, disposal pipes)
            if (HasComp<SubFloorHideComponent>(entity))
                continue;

            // Allow building over entities with WallMount tag (cameras, lights, wall-mounted devices)
            // I may have forgot a few here, add this tag if noticed missing
            if (_tag.HasTag(entity, "WallMount"))
                continue;

            // Block building on other anchored entities (walls, doors, machines, etc.)
            return false;
        }

        return true;
    }
}
