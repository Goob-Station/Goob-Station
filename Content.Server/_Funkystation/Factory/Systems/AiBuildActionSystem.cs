// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Content.Shared._Funkystation.Factory.Events;
using Content.Shared.Silicons.StationAi;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.Factory;
using Content.Shared.DoAfter;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Funkystation.Actions.Events;

namespace Content.Server._Funkystation.Factory.Systems;


// External cancel event (another system or input can raise this to cancel an active build)
public sealed class AiBuildCancelEvent : EntityEventArgs
{
    public EntityUid Performer { get; }
    public AiBuildCancelEvent(EntityUid performer) => Performer = performer;
}

// Notifications for subscribers
public sealed class AiBuildStartedEvent : EntityEventArgs
{
    public EntityUid Performer { get; }
    public MapId MapId { get; }
    public Vector2 Coordinates { get; }
    public string Prototype { get; }
    public float Duration { get; }
    public float Price { get; }

    public AiBuildStartedEvent(EntityUid performer, MapId mapId, Vector2 coordinates, string prototype, float duration, float price)
    {
        Performer = performer;
        MapId = mapId;
        Coordinates = coordinates;
        Prototype = prototype;
        Duration = duration;
        Price = price;
    }
}

public sealed class AiBuildCancelledEvent : EntityEventArgs
{
    public EntityUid Performer { get; }
    public MapId MapId { get; }
    public Vector2 Coordinates { get; }
    public string Prototype { get; }

    public AiBuildCancelledEvent(EntityUid performer, MapId mapId, Vector2 coordinates, string prototype)
    {
        Performer = performer;
        MapId = mapId;
        Coordinates = coordinates;
        Prototype = prototype;
    }
}

public sealed class AiBuildCompletedEvent : EntityEventArgs
{
    public EntityUid Performer { get; }
    public EntityUid Spawned { get; }
    public MapId MapId { get; }
    public Vector2 Coordinates { get; }
    public string Prototype { get; }

    public AiBuildCompletedEvent(EntityUid performer, EntityUid spawned, MapId mapId, Vector2 coordinates, string prototype)
    {
        Performer = performer;
        Spawned = spawned;
        MapId = mapId;
        Coordinates = coordinates;
        Prototype = prototype;
    }
}

// DoAfter payload is defined in Content.Shared._Funkystation.Factory.Events.AiBuildDoAfterEvent

public sealed partial class AiBuildActionSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    private static readonly ISawmill Sawmill = Logger.GetSawmill("ai.build");

    // Tracks active builds keyed by performer
    private readonly Dictionary<EntityUid, ActiveBuild> _active = new();

    private record struct ActiveBuild(
        EntityCoordinates Target,
        string Prototype
    );

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfunctioningAiComponent, AiBuildActionEvent>(OnBuildStart);
        SubscribeLocalEvent<MalfunctioningAiComponent, AiBuildCancelEvent>(OnBuildCancel);
        SubscribeLocalEvent<MalfunctioningAiComponent, AiBuildDoAfterEvent>(OnBuildDoAfter);
    }

    // Start: resolve tile, mark active, play visual, start DoAfter
    private void OnBuildStart(EntityUid uid, MalfunctioningAiComponent comp, ref AiBuildActionEvent args)
    {
        var performer = args.Performer;

        if (args.Handled)
            return;

        var coords = args.Target;

        // Basic validation
        if (performer == EntityUid.Invalid || !coords.IsValid(EntityManager))
        {
            Sawmill.Warning("AiBuild start: invalid performer or coordinates.");
            return;
        }


        // Ensure tile is free (no anchored entities such as walls or machinery)
        if (!IsTileFree(coords))
        {
            Sawmill.Debug($"AiBuild start: tile is occupied at {coords.ToString()}. Build rejected.");
            return;
        }

        // Price default to free
        var price = args.Price ?? 0f;

        // If an existing build is active for this performer, cancel it first
        if (_active.TryGetValue(performer, out var existing))
        {
            _active.Remove(performer);
            RaiseLocalEvent(performer, new AiBuildCancelledEvent(performer, _transform.GetMapId(existing.Target), existing.Target.Position, existing.Prototype));
        }

        // Record active build
        _active[performer] = new ActiveBuild(coords, args.Prototype);

        // Raise start event for subscribers
        RaiseLocalEvent(performer, new AiBuildStartedEvent(performer, _transform.GetMapId(coords), coords.Position, args.Prototype, args.Duration, price));

        // Start DoAfter
        var doAfterEvent = new AiBuildDoAfterEvent(GetNetCoordinates(coords, null), args.Prototype, null);

        var delay = TimeSpan.FromSeconds(Math.Max(0f, args.Duration));

        // Bind the DoAfter to the AI eye (RemoteEntity) so movement of the eye cancels the build
        EntityUid doAfterUser = performer;
        var aiCore = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _transform, performer);
        if (aiCore != EntityUid.Invalid &&
            TryComp<StationAiCoreComponent>(aiCore, out var coreComp) &&
            coreComp.RemoteEntity.HasValue)
        {
            doAfterUser = coreComp.RemoteEntity.Value;
            Sawmill.Info($"AiBuild start: Using RemoteEntity for DoAfter display and movement break: {ToPrettyString(doAfterUser)}");
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, doAfterUser, delay, doAfterEvent, eventTarget: performer)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            CancelDuplicate = false,
            BlockDuplicate = false,
            NeedHand = false,
            Hidden = false
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfterArgs);
        if (!args.Handled)
        {
            _active.Remove(performer);
            Sawmill.Warning("AiBuild start: Failed to start DoAfter.");
        }
    }

    // External cancel: stop countdown, clean visuals, notify
    private void OnBuildCancel(EntityUid uid, MalfunctioningAiComponent comp, ref AiBuildCancelEvent ev)
    {
        var performer = ev.Performer;
        if (!_active.TryGetValue(performer, out var build))
            return;

        _active.Remove(performer);

        RaiseLocalEvent(performer, new AiBuildCancelledEvent(performer, _transform.GetMapId(build.Target), build.Target.Position, build.Prototype));

        Sawmill.Debug($"AiBuild cancel: performer={performer} target='{build.Prototype}' at {build.Target}.");
    }

    // Completion or cancellation from DoAfter
    private void OnBuildDoAfter(EntityUid uid, MalfunctioningAiComponent comp, ref AiBuildDoAfterEvent args)
    {
        var performer = uid;


        if (!_active.TryGetValue(performer, out var build))
            return;

        _active.Remove(performer);

        if (args.Cancelled)
        {
            RaiseLocalEvent(performer, new AiBuildCancelledEvent(performer, _transform.GetMapId(build.Target), build.Target.Position, build.Prototype));
            Sawmill.Debug("AiBuild doafter: cancelled by movement/damage/etc.");
            return;
        }

        // Otherwise, spawn the entity at the target tile
        var location = GetCoordinates(args.Location);
        if (string.IsNullOrWhiteSpace(args.Prototype) || !_prototypes.HasIndex<EntityPrototype>(args.Prototype))
        {
            Sawmill.Error($"AiBuild doafter: invalid or missing prototype at completion '{args.Prototype ?? "<null>"}'.");
            RaiseLocalEvent(performer, new AiBuildCancelledEvent(performer, _transform.GetMapId(build.Target), build.Target.Position, build.Prototype));
            return;
        }

        // Final occupancy check in case the tile became blocked during the countdown
        if (!IsTileFree(location))
        {
            Sawmill.Debug($"AiBuild doafter: tile is now occupied at {location}. Cancelling spawn.");
            RaiseLocalEvent(performer, new AiBuildCancelledEvent(performer, _transform.GetMapId(build.Target), build.Target.Position, build.Prototype));
            return;
        }
    }

    private void RemoveRoboticsFactoryAction(EntityUid performer)
    {
        if (!TryComp<ActionsComponent>(performer, out var actionsComp))
            return;

        var toRemove = new HashSet<Entity<ActionComponent>>();
        foreach (var action in _actions.GetActions(performer, actionsComp))
        {
            var baseEvent = _actions.GetEvent(action.Owner);
            if (baseEvent is MalfAiRoboticsFactoryActionEvent)
                toRemove.Add(action);
        }

        foreach (var action in toRemove)
            _actions.RemoveAction(action.AsNullable());
    }

    // Ensure tile is unoccupied by anchored entities (e.g., walls, machinery)
    private bool IsTileFree(EntityCoordinates coordinates)
    {
        if (!coordinates.IsValid(EntityManager))
            return false;

        var gridUid = coordinates.EntityId;
        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            return false;
        // Use MapSystem.LocalToTile (non-obsolete)
        var tile = _mapSystem.LocalToTile(gridUid, gridComp, coordinates);
        foreach (var _ in _mapSystem.GetAnchoredEntities(gridUid, gridComp, tile))
        {
            return false;
        }
        return true;
    }
}
