using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.Projectiles.SpawnEntitiesOnHit;

public sealed class SpawnEntitiesOnHitSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnEntitiesOnHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<SpawnEntitiesOnHitComponent> ent, ref ProjectileHitEvent args)
    {
        var (uid, comp) = ent;

        if (comp.SpawnOnlyIfHitMob && !HasComp<MobStateComponent>(args.Target))
            return;

        var coords = Transform(uid).Coordinates;
        for (var i = 0; i < ent.Comp.Amount; i++)
        {
            Spawn(comp.Proto, coords);
        }

        if (comp.DeleteOnSpawn && _net.IsServer)
            QueueDel(uid);
    }
}
