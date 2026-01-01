using Content.Goobstation.Shared.ExplodeOnPickup;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Item;

namespace Content.Goobstation.Server.ExplodeOnPickup;

public sealed class ExplodeOnPickupSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ExplodeOnPickupComponent, GettingPickedUpAttemptEvent>(OnPickup);
    }

    private void OnPickup(EntityUid uid, ExplodeOnPickupComponent comp, GettingPickedUpAttemptEvent args)
    {
        _explosionSystem.QueueExplosion(uid, comp.ExplosionType, comp.ExplosionIntensity, comp.Slope, comp.TileIntensity, canCreateVacuum: comp.CreateVacuum);
        RemComp<ExplodeOnPickupComponent>(uid);
    }
}
