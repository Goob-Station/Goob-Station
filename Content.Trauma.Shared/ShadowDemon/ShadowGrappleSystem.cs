// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.ShadowDemon;

public sealed class ShadowGrappleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private const string GrappleJoint = "grappling";

    private static readonly EntProtoId Ash = "Ash";

    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<HandheldLightComponent> _handheldQuery;

    private readonly HashSet<Entity<PoweredLightComponent>> _lights = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileEmbedEvent>(OnEmbed);
        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileHitEvent>(OnHit);

        SubscribeLocalEvent<ShadowGrappleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowGrappleComponent, ComponentShutdown>(OnShutdown);

        _bodyQuery = GetEntityQuery<BodyComponent>();
        _handheldQuery = GetEntityQuery<HandheldLightComponent>();
    }

    private void OnEmbed(Entity<ShadowGrappleProjectileComponent> ent, ref ProjectileEmbedEvent args)
    {
        if (!_timing.IsFirstTimePredicted || args.Shooter is not {} shooter)
                return;

        EnsureComp<JointComponent>(ent.Owner);
        var joint = _joints.CreateDistanceJoint(
            ent.Owner,
            shooter,
            anchorA: Vector2.Zero,
            anchorB: Vector2.Zero,
            id: GrappleJoint);

        joint.MinLength = 0.35f;
        joint.Stiffness = 10f;
        joint.Damping = 5f;
        Dirty(ent);
    }

    private void OnHit(Entity<ShadowGrappleProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (args.Shooter is not { } shooter)
            return;

        var target = args.Target;
        _throwingSystem.TryThrow(shooter, Transform(target).Coordinates, 10f, shooter, doSpin: true);

        // Body, apply damage
        if (_bodyQuery.HasComp(target))
        {
            _damageableSystem.TryChangeDamage(target, ent.Comp.DamageOnHit);
            BreakLightsOnTarget(target);

            _stunSystem.TryAddParalyzeDuration(target, ent.Comp.StunTime);
            return;
        }

        BreakNearbyLights(target, args.Shooter, ent.Comp.BreakLightsRange);
    }

    private void OnMapInit(Entity<ShadowGrappleComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionUid, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowGrappleComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionUid);

    #region Helper
    /// <summary>
    /// Break any lights nearby.
    /// </summary>
    private void BreakNearbyLights(EntityUid target, EntityUid? user, float range = 1f)
    {
        _lights.Clear();
        _lookup.GetEntitiesInRange(Transform(target).Coordinates, range, _lights);
        foreach (var light in _lights)
        {
            _poweredLight.TryDestroyBulb(light.Owner, light.Comp, user);
        }
    }

    /// <summary>
    /// Breaks any lights on someone.
    /// </summary>
    private void BreakLightsOnTarget(EntityUid target)
    {
        foreach (var slotEnt in _inventory.GetHandOrInventoryEntities(target))
        {
            if (!_handheldQuery.HasComp(slotEnt))
                continue;

            PredictedSpawnAtPosition(Ash, Transform(target).Coordinates);
            PredictedQueueDel(slotEnt);
        }
    }
    #endregion
 }
