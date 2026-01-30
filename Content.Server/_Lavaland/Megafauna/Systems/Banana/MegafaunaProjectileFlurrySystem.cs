using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Lavaland.Megafauna.Components.Banana;
using Content.Shared._Lavaland.Megafauna.Events.Banana;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server._Lavaland.Megafauna.Systems.Banana;

public sealed class MegafaunaProjectileFlurrySystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ITimerManager _timer = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MegafaunaProjectileFlurryComponent, MegafaunaProjectileFlurryEvent>(OnFlurry);
    }

    private void OnFlurry(EntityUid uid, MegafaunaProjectileFlurryComponent comp, MegafaunaProjectileFlurryEvent args)
    {
        for (var i = 0; i < comp.ProjectileNumber; i++)
        {
            var delay = TimeSpan.FromSeconds(comp.SpawnDelay * i);

            Timer.Spawn(delay, () =>
            {
                if (Deleted(uid))
                    return;

                FireRandomProjectile(uid, comp);
            });
        }
    }

    private void FireRandomProjectile(EntityUid owner, MegafaunaProjectileFlurryComponent comp)
    {
        var xform = Transform(owner);
        var fromCoords = xform.Coordinates;
        var fromMap = _transform.ToMapCoordinates(fromCoords);
        var shooterVelocity = _physics.GetMapLinearVelocity(fromCoords);

        var angle = _random.NextAngle();
        var direction = angle.ToVec();

        var projectile = Spawn(comp.Prototype, fromMap);

        _gun.ShootProjectile(
            projectile,
            direction,
            shooterVelocity,
            owner,
            owner,
            comp.Speed
        );
    }
}
