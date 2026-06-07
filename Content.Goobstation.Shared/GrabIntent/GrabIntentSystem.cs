using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Grab;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._White.Grab;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.GrabIntent;

public sealed partial class GrabIntentSystem : EntitySystem
{
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualSystem = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifierSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly HeldSpeedModifierSystem _clothingMoveSpeed = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;

    private readonly SoundPathSpecifier _thudswoosh = new("/Audio/Effects/thudswoosh.ogg");

    #region Initialization

    public override void Initialize()
    {
        InitializeCoreEvents();
        InitializeGrabStageEvents();
        InitializeReleaseAndThrowEvents();
    }

    private void InitializeCoreEvents()
    {
        SubscribeLocalEvent<GrabbableComponent, MoveInputEvent>(OnPullableMoveInput);
        SubscribeLocalEvent<GrabbableComponent, CheckGrabbedEvent>(OnCheckGrabbed);
        SubscribeLocalEvent<GrabbableComponent, GrabAttemptEvent>(OnGrabAttempt);
        SubscribeLocalEvent<GrabbableComponent, PullStoppedMessage>(OnPullStoppedGrabbable);
        SubscribeLocalEvent<GrabIntentComponent, PullStoppedMessage>(OnPullStoppedGrabIntent);
    }

    #endregion

    #region Core Events

    private void OnPullStoppedGrabbable(EntityUid uid, GrabbableComponent component, ref PullStoppedMessage args)
    {
        if (args.PulledUid != uid)
            return;

        component.GrabStage = GrabStage.No;
        component.GrabEscapeChance = 1f;
        component.EscapeAttemptModifier = 1f;
        _blocker.UpdateCanMove(uid);
        _alertsSystem.ClearAlert(uid, component.PulledAlert);
        Dirty(uid, component);
    }

    private void OnPullStoppedGrabIntent(EntityUid uid, GrabIntentComponent component, ref PullStoppedMessage args)
    {
        if (args.PullerUid != uid)
            return;

        component.GrabStage = GrabStage.No;

        foreach (var item in GetGrabVirtualItems(uid, args.PulledUid).ToList())
        {
            if (TryComp<VirtualItemComponent>(item, out var vi))
                _virtualSystem.DeleteVirtualItem((item, vi), uid);
            else
                QueueDel(item);
        }

        Dirty(uid, component);
    }

    private void OnCheckGrabbed(EntityUid uid, GrabbableComponent component, ref CheckGrabbedEvent args)
    {
        args.IsGrabbed = component.GrabStage != GrabStage.No;
    }

    private void OnGrabAttempt(Entity<GrabbableComponent> ent, ref GrabAttemptEvent args)
    {
        if (!TryComp<PullableComponent>(ent, out var pullable))
            return;

        args.Grabbed = TryGrab((ent.Owner, pullable, ent.Comp),
            args.Puller,
            args.IgnoreCombatMode,
            args.GrabStageOverride,
            args.EscapeAttemptModifier);
    }

    private void OnPullableMoveInput(EntityUid uid, GrabbableComponent component, ref MoveInputEvent args)
    {
        if (!TryComp<PullableComponent>(uid, out var pullable) || !pullable.BeingPulled)
            return;

        if (component.GrabStage == GrabStage.Soft)
            _pulling.TryStopPull(uid, pullable, uid);

        if (!_blocker.CanMove(args.Entity))
            return;

        _pulling.TryStopPull(uid, pullable, user: uid);
    }

    #endregion

    #region Public API

    public bool CanGrab(EntityUid puller, EntityUid pullable)
    {
        return !HasComp<PacifiedComponent>(puller) && HasComp<MobStateComponent>(pullable);
    }

    public void ThrowGrabbedEntity(Entity<PullerComponent?, GrabIntentComponent?, PhysicsComponent?> ent, Vector2 dir)
    {
        if (!Resolve(ent.Owner, ref ent.Comp1, ref ent.Comp2, ref ent.Comp3, false)
            || ent.Comp1.Pulling is not { } pulling
            || !TryComp(pulling, out PullableComponent? pullingPullableComp))
            return;

        if (!_combatMode.IsInCombatMode(ent.Owner)
            || HasComp<GrabThrownComponent>(pulling)
            || ent.Comp2.GrabStage <= GrabStage.Soft)
            return;

        var distanceToCursor = dir.Length();
        var direction = dir.Normalized() * MathF.Min(distanceToCursor, ent.Comp2.ThrowingDistance);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add(ent.Comp2.GrabThrowDamageType, ent.Comp2.GrabThrowDamage);

        _pulling.TryStopPull(pulling, pullingPullableComp, ent.Owner, true);
        _grabThrown.Throw(pulling,
            ent.Owner,
            direction,
            ent.Comp2.GrabThrownSpeed,
            damage * ent.Comp2.GrabThrowDamageModifier);
        _throwing.TryThrow(ent.Owner, -direction * ent.Comp3.InvMass);
        _audio.PlayPredicted(ent.Comp2.GrabSoundEffect, pulling, ent.Owner);
        ent.Comp2.NextStageChange = _timing.CurTime.Add(TimeSpan.FromSeconds(3f));
        Dirty(ent.Owner, ent.Comp2);
    }

    #endregion
}
