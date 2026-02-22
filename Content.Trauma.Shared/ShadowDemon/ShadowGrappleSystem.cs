using System.Numerics;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.ShadowDemon;

public sealed class ShadowGrappleSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;

    private const string GrappleJoint = "grappling";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowGrappleEvent>(OnGrapple);

        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileEmbedEvent>(OnEmbed);
        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnGrapple(ref ShadowGrappleEvent args)
    {
        // TODO: test in-game
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

    private void OnEmbed(Entity<ShadowGrappleProjectileComponent> ent, ref ProjectileEmbedEvent args)
    {
        if (!_timing.IsFirstTimePredicted || args.Shooter is not {} shooter)
                return;

        EnsureComp<JointComponent>(ent.Owner);
        var joint = _joints.CreateDistanceJoint(ent.Owner, shooter, anchorA: new Vector2(0f, 0.5f), id: GrappleJoint);
        joint.MaxLength = joint.Length + 0.2f;
        joint.Stiffness = 1f;
        joint.MinLength = 0.35f;
        Dirty(ent);
    }

    private void OnHit(Entity<ShadowGrappleProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (args.Shooter is not { } shooter)
            return;

        _throwingSystem.TryThrow(shooter, Transform(args.Target).Coordinates, 30f, shooter);

        // TODO:
        // BreakLights(args.Target);
        // Damage entity if body, else don't damage structures
    }
 }
