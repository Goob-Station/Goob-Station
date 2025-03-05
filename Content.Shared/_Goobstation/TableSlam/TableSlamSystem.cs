using System.Linq;
using Content.Shared.Climbing.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Goobstation.TableSlam;

/// <summary>
/// This handles...
/// </summary>
public sealed class TableSlamSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PullerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<PullableComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnMeleeHit(Entity<PullerComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.GrabStage < GrabStage.Hard
            || ent.Comp.Pulling == null)
            return;

        if(!TryComp<PullableComponent>(ent.Comp.Pulling, out var pullableComponent))
            return;

        if (args.Direction != null)
            return;
        if (args.HitEntities.Count is > 1 or 0)
            return;

        var target = args.HitEntities.ElementAt(0);
        if (!HasComp<BonkableComponent>(target))
            return;

        pullableComponent.BeingTabled = true;
        TryTableSlam((ent.Comp.Pulling.Value, pullableComponent), target);
        _pullingSystem.TryStopPull(ent.Comp.Pulling.Value, pullableComponent, ent.Comp.Pulling.Value, ignoreGrab: true);
    }

    public void TryTableSlam(Entity<PullableComponent> pullable, EntityUid tableUid)
    {
        var tableCoords = _transformSystem.GetWorldPosition(tableUid);
        var pullableCords = _transformSystem.GetWorldPosition(pullable);
        // Calculate direction
        var direction = (tableCoords - pullableCords).Normalized();
        var distance = (tableCoords - pullableCords).Length();
        var throwVelocity = direction * MathF.Max(distance * 3f, 5f);

        // Apply physics impulse to throw the entity towards the table
        _physicsSystem.SetLinearVelocity(pullable, throwVelocity);
    }

    private void OnStartCollide(Entity<PullableComponent> ent, ref StartCollideEvent args)
    {
        if(!ent.Comp.BeingTabled)
            return;

        if (!HasComp<BonkableComponent>(args.OtherEntity))
            return;

        // Apply damage and stun effect
        _damageableSystem.TryChangeDamage(ent,
            new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", 10 } },
            });

        _staminaSystem.TakeStaminaDamage(ent, 40);

        // Knock them down
        _stunSystem.TryParalyze(ent, TimeSpan.FromSeconds(4), refresh: false);
        ent.Comp.BeingTabled = false;
        //_audioSystem.PlayPvs("/Audio/Effects/thudswoosh.ogg", uid);
    }
}
