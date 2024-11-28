using Content.Shared.Projectiles;

namespace Content.Shared._Goobstation.Projectiles.SpawnEntitiesOnHit;

public sealed class SpawnEntitiesOnHitSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnEntitiesOnHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<SpawnEntitiesOnHitComponent> ent, ref ProjectileHitEvent args)
    {
        var coords = Transform(ent).Coordinates;
        for (var i = 0; i < ent.Comp.Amount; i++)
        {
            Spawn(ent.Comp.Proto, coords);
        }
    }
}
