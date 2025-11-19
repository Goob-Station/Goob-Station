// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Projectiles.SpawnEntitiesOnHit;

public sealed class SpawnEntitiesOnHitSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnEntitiesOnHitComponent, ProjectileHitEvent>(OnHit);
        SubscribeLocalEvent<SpawnEntitiesOnHitComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<SpawnEntitiesOnHitComponent> ent, ref PreventCollideEvent args)
    {
        var id = MetaData(args.OtherEntity).EntityPrototype?.ID;
        if (id == null)
            return;

        if (ent.Comp.CollideIgnoreEntities.Contains(id))
            args.Cancelled = true;
    }

    private void OnHit(Entity<SpawnEntitiesOnHitComponent> ent, ref ProjectileHitEvent args)
    {
        var (uid, comp) = ent;

        if (comp.SpawnOnlyIfHitMob && !HasComp<MobStateComponent>(args.Target))
            return;

        var coords = _transform.GetMapCoordinates(uid);
        for (var i = 0; i < ent.Comp.Amount; i++)
        {
            Spawn(comp.Proto, coords);
        }

        if (comp.DeleteOnSpawn && _net.IsServer)
            QueueDel(uid);
    }
}