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
using Content.Shared.Interaction;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Blob;

public abstract class SharedBlobObserverSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<BlobObserverComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<BlobObserverComponent, GetUsedEntityEvent>(OnGetUsedEntityEvent);
    }

    private void OnGetUsedEntityEvent(Entity<BlobObserverComponent> ent, ref GetUsedEntityEvent args)
    {
        if(ent.Comp.VirtualItem.Valid)
            args.Used = ent.Comp.VirtualItem;
    }

    /*private void OnUpdateCanMove(EntityUid uid, BlobObserverComponent component, UpdateCanMoveEvent args)
    {
        if (component.CanMove)
            return;

        args.Cancel();
    }*/

    public (EntityUid? nearestEntityUid, float nearestDistance) CalculateNearestBlobTileDistance(MapCoordinates position)
    {
        var nearestDistance = float.MaxValue;
        EntityUid? nearestEntityUid = null;

        foreach (var lookupUid in _lookup.GetEntitiesInRange<BlobTileComponent>(position, 5f))
        {
            var tileCords = _transform.GetMapCoordinates(lookupUid);
            var distance = Vector2.Distance(position.Position, tileCords.Position);

            if (!(distance < nearestDistance))
                continue;

            nearestDistance = distance;
            nearestEntityUid = lookupUid;
        }

        return (nearestEntityUid, nearestDistance);
    }
}