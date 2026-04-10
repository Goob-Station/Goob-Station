using System.Linq;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Electrocution;
using Content.Shared.Emoting;
using Content.Shared.Flash;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Pointing;
using Content.Shared.ProximityDetection;
using Content.Shared.Standing;
using Content.Shared.StepTrigger.Systems;
using Content.Goobstation.Common.Footprints;

namespace Content.Goobstation.Shared.PhaseShift;

public abstract class SharedPhaseShiftSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PhaseShiftedComponent, ComponentStartup>(OnComponentStartup);

        SubscribeLocalEvent<PhaseShiftedComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
        SubscribeLocalEvent<PhaseShiftedComponent, AttackAttemptEvent>(OnAttackAttempt);

        SubscribeLocalEvent<PhaseShiftedComponent, InteractionVerbAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, UseAttemptEvent>(OnUseAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, GettingInteractedWithAttemptEvent>(OnGettingInteractedWithAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, GettingAttackedAttemptEvent>(OnGettingAttackedAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, BeforeDamageChangedEvent>(OnBeforeDamage);
        SubscribeLocalEvent<PhaseShiftedComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<PhaseShiftedComponent, ElectrocutionAttemptEvent>(OnElectrocutionAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, DownAttemptEvent>(OnDownAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, FlashAttemptEvent>(OnFlashAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, BeforeEmoteEvent>(OnBeforeEmote);
        SubscribeLocalEvent<PhaseShiftedComponent, EmoteAttemptEvent>(OnEmoteAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, PointAttemptEvent>(OnPointAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, FootprintLeaveAttemptEvent>(OnFootprintLeaveAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<PhaseShiftedComponent, ProximityDetectionAttemptEvent>(OnProximityDetectionAttempt);

        SubscribeLocalEvent<PhaseShiftedComponent, ComponentShutdown>(OnComponentShutdown);
    }

    protected virtual void OnComponentStartup(Entity<PhaseShiftedComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.SpawnEffects)
        {
            var pos = _transform.GetMapCoordinates(ent);
            Spawn(ent.Comp.PhaseInEffect, pos);
            _audio.PlayPvs(ent.Comp.PhaseInSound, Transform(ent).Coordinates);
        }

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
        RemComp<PhaseShiftedComponent>(ent);
    }

    private void OnAttempt(EntityUid uid, PhaseShiftedComponent comp, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnUseAttempt(EntityUid uid, PhaseShiftedComponent comp, UseAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnGettingAttackedAttempt(EntityUid uid, PhaseShiftedComponent comp, ref GettingAttackedAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnBeforeDamage(EntityUid uid, PhaseShiftedComponent comp, ref BeforeDamageChangedEvent args)
    {
        args.Cancelled = true;
    }

    private void OnBeforeStaminaDamage(EntityUid uid, PhaseShiftedComponent comp, ref BeforeStaminaDamageEvent args)
    {
        args.Cancelled = true;
    }

    private void OnElectrocutionAttempt(EntityUid uid, PhaseShiftedComponent comp, ElectrocutionAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnDownAttempt(EntityUid uid, PhaseShiftedComponent comp, DownAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnFlashAttempt(EntityUid uid, PhaseShiftedComponent comp, ref FlashAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnBeforeEmote(EntityUid uid, PhaseShiftedComponent comp, ref BeforeEmoteEvent args)
    {
        args.Cancel();
    }

    private void OnEmoteAttempt(EntityUid uid, PhaseShiftedComponent comp, ref EmoteAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnPointAttempt(EntityUid uid, PhaseShiftedComponent comp, PointAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnFootprintLeaveAttempt(EntityUid uid, PhaseShiftedComponent comp, ref FootprintLeaveAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnStepTriggerAttempt(EntityUid uid, PhaseShiftedComponent comp, ref StepTriggerAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnProximityDetectionAttempt(EntityUid uid, PhaseShiftedComponent comp, ref ProximityDetectionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnGettingInteractedWithAttempt(EntityUid uid, PhaseShiftedComponent comp, ref GettingInteractedWithAttemptEvent args)
    {
        args.Cancelled = true;
    }

    protected virtual void OnComponentShutdown(Entity<PhaseShiftedComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.SpawnEffects)
        {
            Spawn(ent.Comp.PhaseOutEffect, _transform.GetMapCoordinates(ent));
            _audio.PlayPvs(ent.Comp.PhaseOutSound, ent);
        }

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
