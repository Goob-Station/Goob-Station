// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.BlockTeleport;
using Content.Goobstation.Common.Effects;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Teleportation;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Teleportation.Systems;

[Virtual]
public partial class SharedRandomTeleportSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    public bool RandomTeleport(EntityUid target, RandomTeleportComponent rtp, bool sound = true, bool @event = true)
        => RandomTeleport(target, rtp, out _, sound, @event);

    public bool RandomTeleport(EntityUid target, RandomTeleportComponent rtp, out Vector2? finalWorldPos, bool sound = true, bool @event = true)
    {
        finalWorldPos = null;

        if (@event && !CanTeleport(target))
            return false;

        // play sound before and after teleport if playSound is true
        if (sound) _audio.PlayPvs(rtp.DepartureSound, Transform(target).Coordinates, AudioParams.Default);
        _sparks.DoSparks(Transform(target).Coordinates); // also sparks!!

        finalWorldPos = RandomTeleport(target, rtp.Radius, rtp.TeleportAttempts, rtp.ForceSafeTeleport);

        if (sound) _audio.PlayPvs(rtp.ArrivalSound, Transform(target).Coordinates, AudioParams.Default);
        _sparks.DoSparks(Transform(target).Coordinates);

        return true;
    }

    public Vector2 GetTeleportVector(float minRadius, float extraRadius)
    {
        // Generate a random number from 0 to 1 and multiply by radius to get distance we should teleport to
        // A square root is taken from the random number so we get an uniform distribution of teleports, else you would get more teleports close to you
        var distance = minRadius + extraRadius * MathF.Sqrt(_random.NextFloat());
        // Generate a random vector with the length we've chosen
        return _random.NextAngle().ToVec() * distance;
    }

    public Vector2? RandomTeleport(EntityUid uid, MinMax radius, int triesBase = 10, bool forceSafe = true)
    {
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

            targetCoords = entityCoords.Offset(GetTeleportVector(radius.Min, extraRadius));

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
        if (!foundValid) targetCoords = entityCoords.Offset(GetTeleportVector(radius.Min, extraRadiusBase));

        // if we teleport the pulled entity goes with us
        EntityUid? pullableEntity = null;
        var stage = GrabStage.No;
        if (TryComp<PullerComponent>(uid, out var puller))
        {
            stage = puller.GrabStage;
            pullableEntity = puller.Pulling;
        }

        _pullingSystem.StopAllPulls(uid);

        var newPos = targetCoords.Position;
        _xform.SetWorldPosition(uid, newPos);

        // pulled entity goes with us
        // btw STOP REVERSING CHECKS
        if (pullableEntity != null)
        {
            _xform.SetWorldPosition(pullableEntity.Value, newPos);
            _pullingSystem.TryStartPull(uid, pullableEntity.Value, grabStageOverride: stage, force: true);
        }

        return newPos;
    }

    private bool CanTeleport(EntityUid uid)
    {
        var ev = new TeleportAttemptEvent(false);
        RaiseLocalEvent(uid, ref ev);
        return !ev.Cancelled;
    }
}
