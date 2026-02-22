using System.Numerics;
using Content.Shared.Body;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
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
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private const string GrappleJoint = "grappling";

    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<InventoryComponent> _inventoryQuery;
    private EntityQuery<HandheldLightComponent> _handheldQuery;

    private readonly HashSet<Entity<PoweredLightComponent>> _lights = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowGrappleEvent>(OnGrapple);

        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileEmbedEvent>(OnEmbed);
        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileHitEvent>(OnHit);

        _bodyQuery = GetEntityQuery<BodyComponent>();
        _inventoryQuery = GetEntityQuery<InventoryComponent>();
        _handheldQuery = GetEntityQuery<HandheldLightComponent>();
    }

    private void OnGrapple(ShadowGrappleEvent args)
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

        var target = args.Target;

        _throwingSystem.TryThrow(shooter, Transform(target).Coordinates, 10f, shooter);

        // Body, apply damage
        if (_bodyQuery.HasComp(target))
        {
            _damageableSystem.TryChangeDamage(target, ent.Comp.DamageOnHit);
            BreakLightsOnTarget(target);

            _stunSystem.TryAddParalyzeDuration(target, ent.Comp.StunTime);
            return;
        }

        BreakNearbyLights(target, args.Shooter);
    }

    /// <summary>
    /// Break any lights nearby.
    /// </summary>
    public void BreakNearbyLights(EntityUid target, EntityUid? user)
    {
        _lights.Clear();
        _lookup.GetEntitiesInRange(Transform(target).Coordinates, 1f, _lights);
        foreach (var light in _lights)
        {
            _poweredLight.TryDestroyBulb(light.Owner, light.Comp, user);
        }
    }

    /// <summary>
    /// Breaks any lights on someone.
    /// </summary>
    public void BreakLightsOnTarget(EntityUid target)
    {
        // todo: fix because this doesn't work
        if (_inventoryQuery.TryComp(target, out var inv))
        {
            foreach (var container in inv.Containers)
            {
                foreach (var containerItem in container.ContainedEntities)
                {
                    if (!_handheldQuery.HasComp(containerItem))
                        continue;

                    Spawn("Ash", Transform(target).Coordinates);
                    QueueDel(containerItem);
                }
            }
        }
    }
 }
