using System.Linq;
using Content.Goobstation.Common.Stunnable;
using Content.Shared.Damage.Components;
using Content.Shared.Jittering;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared.Movement.Systems;

/// <summary>
/// This handles the slowed status effect and other movement status effects.
/// <see cref="MovementModStatusEffectComponent"/> holds a modifier for a status effect which is relayed to a mob's
/// All effects of this kinda are multiplicative.
/// Each 'source' of speed modification usually should have separate effect prototype.
/// </summary>
/// <remarks>
/// Movement modifying status effects should by default be separate effect prototypes, and their effects
/// should stack with each other (multiply). In case multiplicative effect is undesirable - such effects
/// could occupy same prototype, but be aware that this will make controlling duration of effect
/// extra 'challenging', as it will be shared too.
/// </remarks>
public sealed class MovementModStatusSystem : EntitySystem
{
    public static readonly EntProtoId VomitingSlowdown = "VomitingSlowdownStatusEffect";
    public static readonly EntProtoId TaserSlowdown = "TaserSlowdownStatusEffect";
    public static readonly EntProtoId FlashSlowdown = "FlashSlowdownStatusEffect";
    public static readonly EntProtoId StatusEffectFriction = "StatusEffectFriction";

    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!; // goob edit
    [Dependency] private readonly SharedJitteringSystem _jitter = default!; // goob edit

    public override void Initialize()
    {
        SubscribeLocalEvent<MovementModStatusEffectComponent, StatusEffectRemovedEvent>(OnMovementModRemoved);
        SubscribeLocalEvent<MovementModStatusEffectComponent, StatusEffectRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshRelay);
        SubscribeLocalEvent<FrictionStatusEffectComponent, StatusEffectRemovedEvent>(OnFrictionStatusEffectRemoved);
        SubscribeLocalEvent<FrictionStatusEffectComponent, StatusEffectRelayedEvent<RefreshFrictionModifiersEvent>>(OnRefreshFrictionStatus);
        SubscribeLocalEvent<FrictionStatusEffectComponent, StatusEffectRelayedEvent<TileFrictionEvent>>(OnRefreshTileFrictionStatus);
    }

    private void OnMovementModRemoved(Entity<MovementModStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        TryUpdateMovementStatus(args.Target, (ent, ent), 1f);
    }

    private void OnFrictionStatusEffectRemoved(Entity<FrictionStatusEffectComponent> entity, ref StatusEffectRemovedEvent args)
    {
        TrySetFrictionStatus(entity!, 1f, args.Target);
    }

    private void OnRefreshRelay(
        Entity<MovementModStatusEffectComponent> entity,
        ref StatusEffectRelayedEvent<RefreshMovementSpeedModifiersEvent> args
    )
    {
        args.Args.ModifySpeed(entity.Comp.WalkSpeedModifier, entity.Comp.WalkSpeedModifier);
    }

    private void OnRefreshFrictionStatus(Entity<FrictionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<RefreshFrictionModifiersEvent> args)
    {
        var ev = args.Args;
        ev.ModifyFriction(ent.Comp.FrictionModifier);
        ev.ModifyAcceleration(ent.Comp.AccelerationModifier);
        args.Args = ev;
    }

    private void OnRefreshTileFrictionStatus(Entity<FrictionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<TileFrictionEvent> args)
    {
        var ev = args.Args;
        ev.Modifier *= ent.Comp.FrictionModifier;
        args.Args = ev;
    }

    /// <summary>
    /// Apply mob's walking/running speed modifier with provided duration, or increment duration of existing.
    /// </summary>
    /// <param name="uid">Target entity, for which speed should be modified.</param>
    /// <param name="effectProtoId">Slowdown effect to be used.</param>
    /// <param name="duration">Duration of speed modifying effect.</param>
    /// <param name="speedModifier">Multiplier by which walking/sprinting speed should be modified.</param>
    /// <returns>True if entity have slowdown effect applied now or previously and duration was modified.</returns>
    public bool TryAddMovementSpeedModDuration(
        EntityUid uid,
        EntProtoId effectProtoId,
        TimeSpan duration,
        float speedModifier
    )
    {
        return TryAddMovementSpeedModDuration(uid, effectProtoId, duration, speedModifier, speedModifier);
    }

    /// <summary>
    /// Apply mob's walking/running speed modifier with provided duration, or increment duration of existing.
    /// </summary>
    /// <param name="uid">Target entity, for which speed should be modified.</param>
    /// <param name="effectProtoId">Slowdown effect to be used.</param>
    /// <param name="duration">Duration of speed modifying effect.</param>
    /// <param name="walkSpeedModifier">Multiplier by which walking speed should be modified.</param>
    /// <param name="sprintSpeedModifier">Multiplier by which sprinting speed should be modified.</param>
    /// <returns>True if entity have slowdown effect applied now or previously and duration was modified.</returns>
    public bool TryAddMovementSpeedModDuration(
        EntityUid uid,
        EntProtoId effectProtoId,
        TimeSpan duration,
        float walkSpeedModifier,
        float sprintSpeedModifier
    )
    {
        return _status.TryAddStatusEffectDuration(uid, effectProtoId, out var status, duration)
               && TryUpdateMovementStatus(uid, status!.Value, walkSpeedModifier, sprintSpeedModifier);
    }

    /// <summary>
    /// Apply mob's walking/running speed modifier with provided duration,
    /// or update duration of existing if it is lesser than provided duration.
    /// </summary>
    /// <param name="uid">Target entity, for which speed should be modified.</param>
    /// <param name="effectProtoId">Slowdown effect to be used.</param>
    /// <param name="duration">Duration of speed modifying effect.</param>
    /// <param name="speedModifier">Multiplier by which walking/sprinting speed should be modified.</param>
    /// <returns>True if entity have slowdown effect applied now or previously and duration was modified.</returns>
    public bool TryUpdateMovementSpeedModDuration(
        EntityUid uid,
        EntProtoId effectProtoId,
        TimeSpan duration,
        float speedModifier
    )
    {
        return TryUpdateMovementSpeedModDuration(uid, effectProtoId, duration, speedModifier, speedModifier);
    }

    /// <summary>
    /// Apply mob's walking/running speed modifier with provided duration,
    /// or update duration of existing if it is lesser than provided duration.
    /// </summary>
    /// <param name="uid">Target entity, for which speed should be modified.</param>
    /// <param name="effectProtoId">Slowdown effect to be used.</param>
    /// <param name="duration">Duration of speed modifying effect.</param>
    /// <param name="walkSpeedModifier">Multiplier by which walking speed should be modified.</param>
    /// <param name="sprintSpeedModifier">Multiplier by which sprinting speed should be modified.</param>
    /// <returns>True if entity have slowdown effect applied now or previously and duration was modified.</returns>
    public bool TryUpdateMovementSpeedModDuration(
        EntityUid uid,
        EntProtoId effectProtoId,
        TimeSpan? duration,
        float walkSpeedModifier,
        float sprintSpeedModifier
    )
    {
        return _status.TryUpdateStatusEffectDuration(uid, effectProtoId, out var status, duration)
               && TryUpdateMovementStatus(uid, status!.Value, walkSpeedModifier, sprintSpeedModifier);
    }

    /// <summary>
    /// Updates entity's movement speed using <see cref="MovementModStatusEffectComponent"/> to provided values.
    /// Then refreshes the movement speed of the entity.
    /// </summary>
    /// <param name="uid">Entity whose component we're updating</param>
    /// <param name="status">Status effect entity whose modifiers we are updating</param>
    /// <param name="walkSpeedModifier">New walkSpeedModifer we're applying</param>
    /// <param name="sprintSpeedModifier">New sprintSpeedModifier we're applying</param>
    /// <param name="visual">Should the ent jitter, requires staminacomp, all else runs if it doesn't have one</param>
    public bool TryUpdateMovementStatus(
        EntityUid uid,
        Entity<MovementModStatusEffectComponent?> status,
        float walkSpeedModifier,
        float sprintSpeedModifier,
        bool visual = false // Goobstation edit.
    )
    {
        if (!Resolve(status, ref status.Comp))
            return false;

        status.Comp.SprintSpeedModifier = sprintSpeedModifier;
        status.Comp.WalkSpeedModifier = walkSpeedModifier;

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);

        // Goob edit start

        var ignoreEv = new BeforeTrySlowdownEvent();
        RaiseLocalEvent(uid, ref ignoreEv);

        if (ignoreEv.Cancelled)
            return false;


        // specifically mostly john station moved here from sharedstunsystem
        if (!TryComp<StaminaComponent>(uid, out var staminaComp))
            return true; // look all the other code ran if they have no stamina im gonna say they dont need to jitter and its fine.

        // goob edit - stunmeta
        // no slowdown because funny
        // Choose bigger of speed modifiers (usually sprint) and use it to scale Crowd Control effect time
        var cCFactor = Math.Clamp(1 - Math.Min(walkSpeedModifier, sprintSpeedModifier), 0, 1);
        var cCTime = TimeSpan.FromSeconds(10f);
        if (visual && !TerminatingOrDeleted(uid)
            && 0 <= staminaComp.ActiveDrains.Aggregate((float) 0, (current, modifier) => current + modifier.Value.DrainRate)) // Goob edit // Goob edit 2 - So stamina regenerating effects doesn't cause jittering
        {
            _jitter.DoJitter(uid, cCFactor * cCTime, true);
            _stutter.DoStutter(uid, cCFactor * cCTime, true);
        }
        // Goobstation edit end.

        return true;
    }

    /// <summary>
    /// Updates entity's movement speed using <see cref="MovementModStatusEffectComponent"/> to provided value.
    /// Then refreshes the movement speed of the entity.
    /// </summary>
    /// <param name="uid">Entity whose component we're updating</param>
    /// <param name="status">Status effect entity whose modifiers we are updating</param>
    /// <param name="speedModifier">
    /// Multiplier by which speed should be modified.
    /// Will be applied to both walking and running speed.
    /// </param>
    public bool TryUpdateMovementStatus(
        EntityUid uid,
        Entity<MovementModStatusEffectComponent?> status,
        float speedModifier
    )
    {
        return TryUpdateMovementStatus(uid, status, speedModifier, speedModifier);
    }

    /// <inheritdoc cref="TryAddFrictionModDuration(EntityUid,TimeSpan,float,float)"/>
    public bool TryAddFrictionModDuration(
        EntityUid uid,
        TimeSpan duration,
        float friction
    )
    {
        return TryAddFrictionModDuration(uid, duration, friction, friction);
    }

    /// <summary>
    /// Apply friction modifier with provided duration,
    /// or incrementing duration of existing.
    /// </summary>
    /// <param name="uid">Target entity, for which friction modifier should be applied.</param>
    /// <param name="duration">Duration of speed modifying effect.</param>
    /// <param name="friction">Multiplier by which walking speed should be modified.</param>
    /// <param name="acceleration">Multiplier by which sprinting speed should be modified.</param>
    /// <returns>True if entity have slowdown effect applied now or previously and duration was modified.</returns>
    public bool TryAddFrictionModDuration(
        EntityUid uid,
        TimeSpan duration,
        float friction,
        float acceleration
    )
    {
        return _status.TryAddStatusEffectDuration(uid, StatusEffectFriction, out var status, duration)
               && TrySetFrictionStatus(status.Value, friction, acceleration, uid);
    }

    /// <inheritdoc cref="TryUpdateFrictionModDuration(EntityUid,TimeSpan,float,float)"/>
    public bool TryUpdateFrictionModDuration(
        EntityUid uid,
        TimeSpan duration,
        float friction
    )
    {
        return TryUpdateFrictionModDuration(uid,duration, friction, friction);
    }

    /// <summary>
    /// Apply friction modifier with provided duration,
    /// or update duration of existing if it is lesser than provided duration.
    /// </summary>
    /// <param name="uid">Target entity, for which friction modifier should be applied.</param>
    /// <param name="duration">Duration of speed modifying effect.</param>
    /// <param name="friction">Multiplier by which walking speed should be modified.</param>
    /// <param name="acceleration">Multiplier by which sprinting speed should be modified.</param>
    /// <returns>True if entity have slowdown effect applied now or previously and duration was modified.</returns>
    public bool TryUpdateFrictionModDuration(
        EntityUid uid,
        TimeSpan duration,
        float friction,
        float acceleration
    )
    {
        return _status.TryUpdateStatusEffectDuration(uid, StatusEffectFriction, out var status, duration)
               && TrySetFrictionStatus(status.Value, friction, acceleration, uid);
    }

    /// <summary>
    /// Sets the friction status modifiers for a status effect.
    /// </summary>
    /// <param name="status">The status effect entity we're modifying.</param>
    /// <param name="friction">The friction modifier we're applying.</param>
    /// <param name="entity">The entity the status effect is attached to that we need to refresh.</param>
    private bool TrySetFrictionStatus(Entity<FrictionStatusEffectComponent?> status, float friction, EntityUid entity)
    {
        return TrySetFrictionStatus(status, friction, friction, entity);
    }

    /// <summary>
    /// Sets the friction status modifiers for a status effect.
    /// </summary>
    /// <param name="status">The status effect entity we're modifying.</param>
    /// <param name="friction">The friction modifier we're applying.</param>
    /// <param name="acceleration">The acceleration modifier we're applying</param>
    /// <param name="entity">The entity the status effect is attached to that we need to refresh.</param>
    private bool TrySetFrictionStatus(Entity<FrictionStatusEffectComponent?> status, float friction, float acceleration, EntityUid entity)
    {
        if (!Resolve(status, ref status.Comp, false))
            return false;

        status.Comp.FrictionModifier = friction;
        status.Comp.AccelerationModifier = acceleration;
        Dirty(status);

        _movementSpeedModifier.RefreshFrictionModifiers(entity);
        return true;
    }
}
