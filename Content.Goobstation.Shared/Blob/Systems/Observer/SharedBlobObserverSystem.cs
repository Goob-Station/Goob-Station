// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Systems.Core;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Systems;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Blob.Systems.Observer;

public abstract class SharedBlobObserverSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedBlobCoreSystem _blobCore = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    private const double MoverJobTime = 0.005;
    private readonly JobQueue _moveJobQueue = new(MoverJobTime);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlobProjectionComponent, MoveEvent>(OnMoveEvent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _moveJobQueue.Process();
    }

    /// <summary>
    /// Attaches player from a blob core to an observer.
    /// </summary>
    /// <param name="ent"></param>
    public void AttachToObserver(Entity<BlobCoreComponent> ent)
    {
        var (uid, comp) = ent;

        if (comp.Projection == null
            || TerminatingOrDeleted(comp.Projection))
        {
            if (_net.IsClient)
                return;

            CreateProjection(ent);
        }

        var projection = comp.Projection!.Value; // Can't be null here
        var position = Transform(uid).Coordinates;

        _transform.SetCoordinates(projection, position);

        if (TryComp(uid, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(uid, false, eyeComp);
            _eye.SetTarget(uid, projection, eyeComp);
        }

        _mover.SetRelay(uid, projection);
    }

    /// <summary>
    /// Creates blob projection in nullspace.
    /// </summary>
    public void CreateProjection(Entity<BlobCoreComponent> ent)
    {
        var (uid, comp) = ent;

        var newProjection = Spawn(comp.ProjectionProtoId, MapCoordinates.Nullspace);
        var projectionComp = EnsureComp<BlobProjectionComponent>(newProjection);

        projectionComp.Core = uid;
        comp.Projection = newProjection;

        Dirty(newProjection, projectionComp);
    }

    public (EntityUid? nearestEntityUid, float nearestDistance) CalculateNearestBlobTileDistance(MapCoordinates position)
    {
        var nearestDistance = float.MaxValue;
        EntityUid? nearestEntityUid = null;

        foreach (var lookupUid in _lookup.GetEntitiesInRange<BlobTileComponent>(position, 5f))
        {
            if (lookupUid.Comp.Core == null)
                continue;

            var tileCords = _transform.GetMapCoordinates(lookupUid);
            var distance = Vector2.Distance(position.Position, tileCords.Position);

            if (!(distance < nearestDistance))
                continue;

            nearestDistance = distance;
            nearestEntityUid = lookupUid;
        }

        return (nearestEntityUid, nearestDistance);
    }

    private void OnMoveEvent(EntityUid uid, BlobProjectionComponent projectionComponent, ref MoveEvent args)
    {
        if (projectionComponent.IsProcessingMoveEvent)
            return;

        projectionComponent.IsProcessingMoveEvent = true;

        var job = new BlobObserverMover(EntityManager, _transform,this, MoverJobTime)
        {
            Observer = (uid,projectionComponent),
            NewPosition = args.NewPosition
        };

        _moveJobQueue.EnqueueJob(job);
    }
}
