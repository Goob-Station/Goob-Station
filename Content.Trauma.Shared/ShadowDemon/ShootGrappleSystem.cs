using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Trauma.Shared.ShadowDemon;

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

/// <summary>
/// Action event that shoots a grapple at the direction of clicking.
/// </summary>
public sealed partial class ShootGrappleEvent : EntityTargetActionEvent
{
    /// <summary>
    /// The projectile to shoot
    /// </summary>
    [DataField]
    public EntProtoId ProjectileProto;

    /// <summary>
    /// The joint sprite of the projectile (the huge rope that will be attached to the projectile)
    /// </summary>
    [DataField]
    public SpriteSpecifier? JointSprite;
};
