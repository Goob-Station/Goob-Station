using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Lavaland.Megafauna.Banana.Components;
using Content.Shared._Lavaland.Megafauna.Banana.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MegafaunaProjectileFlurrySystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly List<FlurryState> _flurries = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaProjectileFlurryComponent, MegafaunaProjectileFlurryEvent>(OnFlurry);
    }

    private void OnFlurry(Entity<MegafaunaProjectileFlurryComponent> ent, ref MegafaunaProjectileFlurryEvent args)
    {
        _flurries.Add(new FlurryState
        {
            Owner = ent.Owner,
            Component = ent.Comp,
            Remaining = ent.Comp.ProjectileNumber,
            NextSpawn = _timing.CurTime,
        });

        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        for (var i = _flurries.Count - 1; i >= 0; i--)
        {
            var flurry = _flurries[i];

            if (Deleted(flurry.Owner))
            {
                _flurries.RemoveAt(i);
                continue;
            }

            if (time < flurry.NextSpawn)
            {
                continue;
            }

            FireRandomProjectile(flurry.Owner, flurry.Component);

            flurry.Remaining--;
            flurry.NextSpawn = time + TimeSpan.FromSeconds(flurry.Component.SpawnDelay);

            if (flurry.Remaining <= 0)
            {
                _flurries.RemoveAt(i);
            }
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

    private sealed class FlurryState
    {
        public EntityUid Owner;
        public MegafaunaProjectileFlurryComponent Component = default!;
        public int Remaining;
        public TimeSpan NextSpawn;
    }
}
