using Content.Shared._Trauma.Projectiles;
using Content.Shared.Projectiles;
using Robust.Client.Physics;
using Robust.Client.Player;
using Robust.Shared.Physics.Systems;

namespace Content.Client._Trauma.Projectiles;

/// <summary>
/// Predicts physics for projectiles the player shot.
/// </summary>
public sealed class PredictedProjectileSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<PredictedShooterComponent> _shooterQuery;

    public override void Initialize()
    {
        base.Initialize();

        _shooterQuery = GetEntityQuery<PredictedShooterComponent>();

        SubscribeLocalEvent<ProjectileComponent, UpdateIsPredictedEvent>(OnUpdateIsPredicted);
        SubscribeLocalEvent<PredictedShooterComponent, ComponentStartup>(OnShooterStartup);
    }

    private void OnUpdateIsPredicted(Entity<ProjectileComponent> ent, ref UpdateIsPredictedEvent args)
    {
        // Goob - only predict our own shots, either the clientside one or the server's copy of it
        if (IsClientSide(ent) ||
            _shooterQuery.TryComp(ent, out var shooter) && shooter.Shooter == _player.LocalEntity)
        {
            args.IsPredicted = true;
        }
    }

    private void OnShooterStartup(Entity<PredictedShooterComponent> ent, ref ComponentStartup args)
    {
        // Goob - this runs while applying the state the server's projectile arrived in, before that frame's prediction.
        // So it gets simulated up to the predicted tick straight away instead of showing a frame of it back at the gun.
        _physics.UpdateIsPredicted(ent.Owner);
    }
}
