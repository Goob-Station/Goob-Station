using System.Numerics;
using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Trauma.Shared.ShadowDemon;

/// <summary>
/// TODO: Move to another folder
/// </summary>
public sealed class ShootGrappleSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShootGrappleEvent>(OnGrapple);
    }

    private void OnGrapple(ShootGrappleEvent args)
    {
        var user = args.Performer;

        var proj = PredictedSpawnAtPosition(args.ProjectileProto, Transform(user).Coordinates);
        var projPos = _transform.GetWorldPosition(proj);
        var targetPos = _transform.GetWorldPosition(args.Target);

        var dir = (targetPos - projPos).Normalized();

        var visuals = EnsureComp<JointVisualsComponent>(proj);

        if (args.JointSprite is {} jointSprite)
            visuals.Sprite = jointSprite;

        visuals.OffsetA = new Vector2(0f, 0.5f);
        visuals.Target = user;
        Dirty(proj, visuals);

        _gun.ShootProjectile(proj,
            dir,
            Vector2.Zero,
            null,
            user);

        args.Handled = true;
    }
}
