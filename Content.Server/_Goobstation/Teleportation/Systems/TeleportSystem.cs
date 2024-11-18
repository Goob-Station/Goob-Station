using System.Numerics;
using Content.Server.Administration.Logs;
using Content.Server.Stack;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics;
using Robust.Shared.Random;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Maps;
using Content.Shared.Tiles;
using Robust.Shared.Map.Components;
using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Server.Teleportation;

public sealed class TeleportSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly IAdminLogManager _alog = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomTeleportOnUseComponent, UseInHandEvent>(OnUseInHand);

        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    private void OnUseInHand(EntityUid uid, RandomTeleportOnUseComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        RandomTeleport(args.User, component);

        if (component.ConsumeOnUse)
        {
            if (TryComp<StackComponent>(uid, out var stack))
            {
                _stack.SetCount(uid, stack.Count - 1, stack);
                return;
            }

            // It's consumed on use and it's not a stack so delete it
            QueueDel(uid);
        }

        _alog.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):actor} randomly teleported with {ToPrettyString(uid)}");
    }

    public void RandomTeleport(EntityUid uid, RandomTeleportComponent component, bool playSound = true)
    {
        // play sound before and after teleport if playSound is true
        if (playSound)
            _audio.PlayPvs(component.DepartureSound, Transform(uid).Coordinates, AudioParams.Default);

        RandomTeleport(uid, component.Radius, component.TeleportAttempts, component.ForceSafeTeleport);

        if (playSound)
            _audio.PlayPvs(component.ArrivalSound, Transform(uid).Coordinates, AudioParams.Default);

    }

    public Vector2 GetTeleportVector(float minRadius, float extraRadius)
    {
        // Generate a random number from 0 to 1 and multiply by radius to get distance we should teleport to
        // A square root is taken from the random number so we get an uniform distribution of teleports, else you would get more teleports close to you
        var distance = minRadius + extraRadius * MathF.Sqrt(_random.NextFloat());
        // Generate a random vector with the length we've chosen
        return _random.NextAngle().ToVec() * distance;
    }

    public void RandomTeleport(EntityUid uid, MinMax radius, int triesBase = 10, bool forceSafe = true)
    {
        var xform = Transform(uid);
        // break any active pulls e.g. secoff pulling you with cuffs
        if (TryComp<PullableComponent>(uid, out var pullable) && _pullingSystem.IsPulled(uid, pullable))
            _pullingSystem.TryStopPull(uid, pullable);

        // if we teleport the pulled entity goes with us
        EntityUid? pullableEntity = null;
        if (TryComp<PullerComponent>(uid, out var puller))
            pullableEntity = puller.Pulling;

        var entityCoords = xform.Coordinates.ToMap(EntityManager, _xform);

        var targetCoords = new MapCoordinates();
        // Randomly picks tiles in range until it finds a valid tile
        // If attempts is 1 or less, degenerates to a completely random teleport
        var tries = triesBase;
        // If forcing a safe teleport, try double the attempts but gradually lower radius in the second half of them
        if (forceSafe)
            tries *= 2;
        // How far outwards from the minimum radius we can teleport
        var extraRadiusBase = radius.Max - radius.Min;
        var foundValid = false;
        for (var i = 0; i < tries; i++)
        {
            var extraRadius = extraRadiusBase;
            // If we're trying to force a safe teleport and haven't found a valid destination in a while, gradually lower the search radius so we're searching in a smaller area
            if (forceSafe && i >= triesBase)
                extraRadius *= (tries - i) / triesBase;

            targetCoords = entityCoords.Offset(GetTeleportVector(radius.Min, extraRadius));

            // Try to not teleport into open space
            if (!_mapManager.TryFindGridAt(targetCoords, out var gridUid, out var grid))
                continue;
            // Check if we picked a position inside a solid object
            var valid = true;
            foreach (var entity in grid.GetAnchoredEntities(targetCoords))
            {
                if (!_physicsQuery.TryGetComponent(entity, out var body))
                    continue;

                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            // Current target coordinates are not inside a solid body, can go ahead and teleport
            if (valid)
            {
                foundValid = true;
                break;
            }
        }
        // We haven't found a valid teleport, so just teleport to any spot in range
        if (!foundValid)
            targetCoords = entityCoords.Offset(GetTeleportVector(radius.Min, extraRadiusBase));

        _xform.SetWorldPosition(uid, targetCoords.Position);
        // pulled entity goes with us
        if (pullableEntity != null)
            _xform.SetWorldPosition((EntityUid) pullableEntity, _xform.GetWorldPosition(uid));
    }
}
