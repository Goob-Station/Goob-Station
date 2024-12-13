using System.Linq;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.WhiteDream.BloodCult.Constructs.PhaseShift;

public abstract class SharedPhaseShiftSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PhaseShiftedComponent, ComponentStartup>(OnComponentStartup);

        SubscribeLocalEvent<PhaseShiftedComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
        SubscribeLocalEvent<PhaseShiftedComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, ThrowAttemptEvent>(OnThrowAttempt);

        SubscribeLocalEvent<PhaseShiftedComponent, ComponentShutdown>(OnComponentShutdown);
    }

    protected virtual void OnComponentStartup(Entity<PhaseShiftedComponent> ent, ref ComponentStartup args)
    {
        var pos = _transform.GetMapCoordinates(ent);
        Spawn(ent.Comp.PhaseInEffect, pos);
        _audio.PlayPvs(ent.Comp.PhaseInSound, Transform(ent).Coordinates);

        if (TryComp<FixturesComponent>(ent, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();
            ent.Comp.StoredMask = fixture.Value.CollisionMask;
            ent.Comp.StoredLayer = fixture.Value.CollisionLayer;
            _physics.SetCollisionMask(ent, fixture.Key, fixture.Value, ent.Comp.CollisionMask, fixtures);
            _physics.SetCollisionLayer(ent, fixture.Key, fixture.Value, ent.Comp.CollisionLayer, fixtures);
        }

        var stealth = EnsureComp<StealthComponent>(ent);
        _stealth.SetVisibility(ent, -1, stealth);

        if (TryComp(ent, out PullableComponent? pullable))
            _pulling.TryStopPull(ent, pullable);

        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefresh(Entity<PhaseShiftedComponent> ent, ref RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(ent.Comp.MovementSpeedBuff, ent.Comp.MovementSpeedBuff);

    private void OnAttackAttempt(Entity<PhaseShiftedComponent> ent, ref AttackAttemptEvent args)
    {
        if (_statusEffects.HasStatusEffect(ent, ent.Comp.StatusEffectId))
            _statusEffects.TryRemoveStatusEffect(ent, ent.Comp.StatusEffectId);
    }

    private void OnThrowAttempt(Entity<PhaseShiftedComponent> ent, ref ThrowAttemptEvent args)
    {
        if (_statusEffects.HasStatusEffect(ent, ent.Comp.StatusEffectId))
            _statusEffects.TryRemoveStatusEffect(ent, ent.Comp.StatusEffectId);
    }

    protected virtual void OnComponentShutdown(Entity<PhaseShiftedComponent> ent, ref ComponentShutdown args)
    {
        Spawn(ent.Comp.PhaseOutEffect, _transform.GetMapCoordinates(ent));
        _audio.PlayPvs(ent.Comp.PhaseOutSound, ent);

        if (TryComp<FixturesComponent>(ent, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();

            _physics.SetCollisionMask(ent, fixture.Key, fixture.Value, ent.Comp.StoredMask, fixtures);
            _physics.SetCollisionLayer(ent, fixture.Key, fixture.Value, ent.Comp.StoredLayer, fixtures);
        }

        _stealth.SetVisibility(ent, 1);
        RemComp<StealthComponent>(ent);

        ent.Comp.MovementSpeedBuff = 1;
        _movement.RefreshMovementSpeedModifiers(ent);
    }
}
