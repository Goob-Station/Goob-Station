// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.BlockTeleport;
using Content.Goobstation.Common.Effects;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared._Trauma.Teleportation;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Content.Shared.Teleportation;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._Trauma.Teleportation;

public sealed partial class RandomTeleportSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!; // This should be axed when upstream
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<RandomTeleportOnUseComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<RandomTeleportOnUseComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!RandomTeleport(args.User, ent.Comp, out var wp, user: args.User))
            return;

        _adminLog.Add(LogType.Action, LogImpact.Low, $"{args.User:actor} randomly teleported to {wp} using {ent:used}");

        if (!ent.Comp.ConsumeOnUse)
            return;

        if (TryComp<StackComponent>(ent, out var stack))
        {
            _stack.ReduceCount((ent.Owner, stack), 1);
            return;
        }

        // It's consumed on use and it's not a stack so delete it
        PredictedQueueDel(ent);
    }

    public bool RandomTeleport(EntityUid target, RandomTeleportComponent rtp, bool sound = true, EntityUid? user = null, bool predicted = true)
        => RandomTeleport(target, rtp, out _, sound, user, predicted);

    public bool RandomTeleport(EntityUid target, RandomTeleportComponent rtp, out Vector2 finalWorldPos, bool sound = true, EntityUid? user = null, bool predicted = true)
    {
        finalWorldPos = Vector2.Zero;

        if (!_teleport.CanTeleport(target, predicted))
            return false;

        // play sound before and after teleport if sound is true
        var oldCoords = Transform(target).Coordinates;
        if (sound)
            _audio.PlayPredicted(rtp.DepartureSound, oldCoords, predicted ? user : null);
        _sparks.DoSparks(oldCoords); // also sparks!!

        finalWorldPos = RandomTeleport(target, rtp.Radius, rtp.TeleportAttempts, rtp.ForceSafeTeleport, rtp.TeleportPulled);

        var newCoords = Transform(target).Coordinates;
        if (sound)
            _audio.PlayPredicted(rtp.ArrivalSound, oldCoords, predicted ? user : null);
        _sparks.DoSparks(newCoords);

        return true;
    }

    public Vector2 GetTeleportVector(IRobustRandom rand, float minRadius, float extraRadius)
    {
        // Generate a random number from 0 to 1 and multiply by radius to get distance we should teleport to
        // A square root is taken from the random number so we get an uniform distribution of teleports, else you would get more teleports close to you
        var distance = minRadius + extraRadius * MathF.Sqrt(rand.NextFloat());
        // Generate a random vector with the length we've chosen
        return rand.NextAngle().ToVec() * distance;
    }

    public Vector2 RandomTeleport(EntityUid uid, MinMax radius, int triesBase = 10, bool forceSafe = true, bool pulled = true, EntityUid? user = null, bool predicted = false)
    {
        var seed = SharedRandomExtensions.HashCodeCombine((int) _timing.CurTick.Value, GetNetEntity(uid).Id);
        var rand = new RobustRandom();
        rand.SetSeed(seed);

        var xform = Transform(uid);
        var entityCoords = _xform.ToMapCoordinates(xform.Coordinates);

        var targetCoords = new MapCoordinates();

        // Randomly picks tiles in range until it finds a valid tile
        // If attempts is 1 or less, degenerates to a completely random teleport
        var tries = triesBase;

        // If forcing a safe teleport, try double the attempts but gradually lower radius in the second half of them
        if (forceSafe) tries *= 2;

        // How far outwards from the minimum radius we can teleport
        var extraRadiusBase = radius.Max - radius.Min;
        var foundValid = false;
        for (var i = 0; i < tries; i++)
        {
            var extraRadius = extraRadiusBase;
            // If we're trying to force a safe teleport and haven't found a valid destination in a while, gradually lower the search radius so we're searching in a smaller area
            if (forceSafe && i >= triesBase)
                extraRadius *= (tries - i) / triesBase;

            targetCoords = entityCoords.Offset(GetTeleportVector(rand, radius.Min, extraRadius));

            // Try to not teleport into open space
            if (!_mapManager.TryFindGridAt(targetCoords, out var gridUid, out var grid))
                continue;

            // Check if we picked a position inside a solid object
            var valid = true;
            foreach (var entity in _map.GetAnchoredEntities((gridUid, grid), targetCoords))
            {
                if (!_physicsQuery.TryGetComponent(entity, out var body))
                    continue;

                if (body.BodyType != BodyType.Static || !body.Hard ||
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
        if (!foundValid) targetCoords = entityCoords.Offset(GetTeleportVector(rand, radius.Min, extraRadiusBase));

        var newPos = _xform.ToCoordinates(targetCoords);
        _teleport.Teleport(uid, newPos, user, predicted, pulled);
        return newPos.Position;
    }
}
