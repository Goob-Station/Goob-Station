using Content.Shared._Goobstation.Grab; // Goobstation - Martial Arts
using Content.Shared._Goobstation.MartialArts.Events; // Goobstation - Martial Arts
using Content.Shared._EinsteinEngines.Contests; // Goobstation - Grab Intent
using Content.Shared._Goobstation.Grab; // Goobstation - Grab Intent
using Content.Shared._White.Grab; // Goobstation
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode; // Goobstation
using Content.Shared.Cuffs.Components; // Goobstation
using Content.Shared.Damage; // Goobstation
using Content.Shared.Damage.Systems; // Goobstation
using Content.Shared.Database;
using Content.Shared.Effects; // Goobstation
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem; // Goobstation
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components; // Goobstation
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech; // Goobstation
using Content.Shared.Standing;
using Content.Shared.Throwing; // Goobstation
using Content.Shared.Verbs;
using Robust.Shared.Audio; // Goobstation
using Robust.Shared.Audio.Systems; // Goobstation
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network; // Goobstation
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random; // Goobstation
using Robust.Shared.Timing;

namespace Content.Shared.Movement.Pulling.Systems;

/// <summary>
/// Allows one entity to pull another behind them via a physics distance joint.
/// </summary>
public sealed class PullingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifierSystem = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly HeldSpeedModifierSystem _clothingMoveSpeed = default!;
    [Dependency] private readonly GrabbingItemSystem _grabbingItem = default!; // Goobstation
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualSystem = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly ContestsSystem _contests = default!; // Goobstation - Grab Intent

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SharedPhysicsSystem));
        UpdatesOutsidePrediction = true;

        SubscribeLocalEvent<PullableComponent, MoveInputEvent>(OnPullableMoveInput);
        SubscribeLocalEvent<PullableComponent, CollisionChangeEvent>(OnPullableCollisionChange);
        SubscribeLocalEvent<PullableComponent, JointRemovedEvent>(OnJointRemoved);
        SubscribeLocalEvent<PullableComponent, GetVerbsEvent<Verb>>(AddPullVerbs);
        SubscribeLocalEvent<PullableComponent, EntGotInsertedIntoContainerMessage>(OnPullableContainerInsert);
        SubscribeLocalEvent<PullableComponent, ModifyUncuffDurationEvent>(OnModifyUncuffDuration);
        SubscribeLocalEvent<PullableComponent, StopBeingPulledAlertEvent>(OnStopBeingPulledAlert);
        SubscribeLocalEvent<PullableComponent, UpdateCanMoveEvent>(OnGrabbedMoveAttempt); // Goobstation
        SubscribeLocalEvent<PullableComponent, SpeakAttemptEvent>(OnGrabbedSpeakAttempt); // Goobstation

        SubscribeLocalEvent<PullerComponent, UpdateMobStateEvent>(OnStateChanged);
        SubscribeLocalEvent<PullerComponent, AfterAutoHandleStateEvent>(OnAfterState);
        SubscribeLocalEvent<PullerComponent, EntGotInsertedIntoContainerMessage>(OnPullerContainerInsert);
        SubscribeLocalEvent<PullerComponent, EntityUnpausedEvent>(OnPullerUnpaused);
        SubscribeLocalEvent<PullerComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<PullerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<PullerComponent, DropHandItemsEvent>(OnDropHandItems);
        SubscribeLocalEvent<PullerComponent, StopPullingAlertEvent>(OnStopPullingAlert);
        SubscribeLocalEvent<PullerComponent, VirtualItemThrownEvent>(OnVirtualItemThrown); // Goobstation - Grab Intent
        SubscribeLocalEvent<PullerComponent, AddCuffDoAfterEvent>(OnAddCuffDoAfterEvent); // Goobstation - Grab Intent

        SubscribeLocalEvent<PullableComponent, StrappedEvent>(OnBuckled);
        SubscribeLocalEvent<PullableComponent, BuckledEvent>(OnGotBuckled);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ReleasePulledObject, InputCmdHandler.FromDelegate(OnReleasePulledObject, handle: false))
            .Register<PullingSystem>();
    }
    // Goobstation - Grab Intent
    private void OnAddCuffDoAfterEvent(Entity<PullerComponent> ent, ref AddCuffDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (!args.Cancelled
            && TryComp<PullableComponent>(ent.Comp.Pulling, out var comp)
            && ent.Comp.Pulling != null)
        {
            if(_netManager.IsServer)
                StopPulling(ent.Comp.Pulling.Value, comp);
        }
    }
    // Goobstation

    private void OnStateChanged(EntityUid uid, PullerComponent component, ref UpdateMobStateEvent args)
    {
        if (component.Pulling == null)
            return;

        if (TryComp<PullableComponent>(component.Pulling, out var comp) && (args.State == MobState.Critical || args.State == MobState.Dead))
        {
            TryStopPull(component.Pulling.Value, comp);
        }
    }

    private void OnBuckled(Entity<PullableComponent> ent, ref StrappedEvent args)
    {
        // Prevent people from pulling the entity they are buckled to
        if (ent.Comp.Puller == args.Buckle.Owner && !args.Buckle.Comp.PullStrap)
            StopPulling(ent, ent);
    }

    private void OnGotBuckled(Entity<PullableComponent> ent, ref BuckledEvent args)
    {
        StopPulling(ent, ent);
    }

    private void OnAfterState(Entity<PullerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.Pulling == null)
            RemComp<ActivePullerComponent>(ent.Owner);
        else
            EnsureComp<ActivePullerComponent>(ent.Owner);
    }

    private void OnDropHandItems(EntityUid uid, PullerComponent pullerComp, DropHandItemsEvent args)
    {
        if (pullerComp.Pulling == null || pullerComp.NeedsHands)
            return;

        if (!TryComp(pullerComp.Pulling, out PullableComponent? pullableComp))
            return;

        TryStopPull(pullerComp.Pulling.Value, pullableComp, uid);
    }

    private void OnStopPullingAlert(Entity<PullerComponent> ent, ref StopPullingAlertEvent args)
    {
        if (args.Handled)
            return;
        if (!TryComp<PullableComponent>(ent.Comp.Pulling, out var pullable))
            return;
        args.Handled = TryStopPull(ent.Comp.Pulling.Value, pullable, ent);
    }

    private void OnPullerContainerInsert(Entity<PullerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (ent.Comp.Pulling == null)
            return;

        if (!TryComp(ent.Comp.Pulling.Value, out PullableComponent? pulling))
            return;

        // Goobstation - Grab Intent
        foreach (var item in ent.Comp.GrabVirtualItems)
            QueueDel(item);

        TryStopPull(ent.Comp.Pulling.Value, pulling, ent.Owner, true);
        // Goobstation
    }

    private void OnPullableContainerInsert(Entity<PullableComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        TryStopPull(ent.Owner, ent.Comp, ignoreGrab: true); // Goobstation
    }

    private void OnModifyUncuffDuration(Entity<PullableComponent> ent, ref ModifyUncuffDurationEvent args)
    {
        if (!ent.Comp.BeingPulled)
            return;

        // We don't care if the person is being uncuffed by someone else
        if (args.User != args.Target)
            return;

        args.Duration *= 2;
    }

    private void OnStopBeingPulledAlert(Entity<PullableComponent> ent, ref StopBeingPulledAlertEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryStopPull(ent, ent, ent);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<PullingSystem>();
    }

    private void OnPullerUnpaused(EntityUid uid, PullerComponent component, ref EntityUnpausedEvent args)
    {
        component.NextThrow += args.PausedTime;
    }

    // Goobstation - Grab Intent Refactor
    private void OnVirtualItemDeleted(Entity<PullerComponent> ent, ref VirtualItemDeletedEvent args)
    {
        // If client deletes the virtual hand then stop the pull.
        if (ent.Comp.Pulling == null)
            return;

        if (ent.Comp.Pulling != args.BlockingEntity)
            return;

        if (TryComp(args.BlockingEntity, out PullableComponent? comp))
        {
            TryStopPull(ent.Comp.Pulling.Value, comp, ent);
        }

        foreach (var item in ent.Comp.GrabVirtualItems)
        {
            if(TryComp<VirtualItemComponent>(ent, out var virtualItemComponent))
                _virtualSystem.DeleteVirtualItem((item,virtualItemComponent), ent);
        }
        ent.Comp.GrabVirtualItems.Clear();
    }
    // Goobstation - Grab Intent Refactor

    // Goobstation - Grab Intent
    private void OnVirtualItemThrown(EntityUid uid, PullerComponent component, VirtualItemThrownEvent args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var throwerPhysics)
            || component.Pulling == null
            || component.Pulling != args.BlockingEntity)
            return;

        if (!TryComp(args.BlockingEntity, out PullableComponent? comp))
            return;

        if (!_combatMode.IsInCombatMode(uid)
            || HasComp<GrabThrownComponent>(args.BlockingEntity)
            || component.GrabStage <= GrabStage.Soft)
            return;

        var distanceToCursor = args.Direction.Length();
        var direction = args.Direction.Normalized() * MathF.Min(distanceToCursor, component.ThrowingDistance);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 5);

        TryStopPull(args.BlockingEntity, comp, uid, true);
        _grabThrown.Throw(args.BlockingEntity,
            uid,
            direction,
            component.GrabThrownSpeed,
            component.StaminaDamageOnThrown,
            damage * component.GrabThrowDamageModifier); // Throwing the grabbed person
        _throwing.TryThrow(uid, -direction * throwerPhysics.InvMass); // Throws back the grabber
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), uid);
        component.NextStageChange.Add(TimeSpan.FromSeconds(4f)); // To avoid grab and throw spamming
    }
    // Goobstation


    private void AddPullVerbs(EntityUid uid, PullableComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Are they trying to pull themselves up by their bootstraps?
        if (args.User == args.Target)
            return;

        //TODO VERB ICONS add pulling icon
        if (component.Puller == args.User)
        {
            Verb verb = new()
            {
                Text = Loc.GetString("pulling-verb-get-data-text-stop-pulling"),
                Act = () => TryStopPull(uid, component, user: args.User),
                DoContactInteraction = false // pulling handle its own contact interaction.
            };
            args.Verbs.Add(verb);
        }
        else if (CanPull(args.User, args.Target))
        {
            Verb verb = new()
            {
                Text = Loc.GetString("pulling-verb-get-data-text"),
                Act = () => TryStartPull(args.User, args.Target),
                DoContactInteraction = false // pulling handle its own contact interaction.
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnRefreshMovespeed(EntityUid uid, PullerComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (TryComp<HeldSpeedModifierComponent>(component.Pulling, out var itemHeldSpeed) && component.Pulling.HasValue)
        {
            var (walkMod, sprintMod) =
                _clothingMoveSpeed.GetHeldMovementSpeedModifiers(component.Pulling.Value, itemHeldSpeed);
            args.ModifySpeed(walkMod, sprintMod);
        }

        if (TryComp<HeldSpeedModifierComponent>(component.Pulling, out var heldMoveSpeed) && component.Pulling.HasValue)
        {
            var (walkMod, sprintMod) = (args.WalkSpeedModifier, args.SprintSpeedModifier);

            switch (component.GrabStage)
            {
                case GrabStage.No:
                    args.ModifySpeed(walkMod, sprintMod);
                    break;
                case GrabStage.Soft:
                    var softGrabSpeedMod = component.SoftGrabSpeedModifier;
                    args.ModifySpeed(walkMod * softGrabSpeedMod, sprintMod * softGrabSpeedMod);
                    break;
                case GrabStage.Hard:
                    var hardGrabSpeedModifier = component.HardGrabSpeedModifier;
                    args.ModifySpeed(walkMod * hardGrabSpeedModifier, sprintMod * hardGrabSpeedModifier);
                    break;
                case GrabStage.Suffocate:
                    var chokeSpeedMod = component.ChokeGrabSpeedModifier;
                    args.ModifySpeed(walkMod * chokeSpeedMod, sprintMod * chokeSpeedMod);
                    break;
                default:
                    args.ModifySpeed(walkMod, sprintMod);
                    break;
            }
            return;
        }

        switch (component.GrabStage)
        {
            case GrabStage.No:
                args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
                break;
            case GrabStage.Soft:
                var softGrabSpeedMod = component.SoftGrabSpeedModifier;
                args.ModifySpeed(component.WalkSpeedModifier * softGrabSpeedMod, component.SprintSpeedModifier * softGrabSpeedMod);
                break;
            case GrabStage.Hard:
                var hardGrabSpeedModifier = component.HardGrabSpeedModifier;
                args.ModifySpeed(component.WalkSpeedModifier * hardGrabSpeedModifier, component.SprintSpeedModifier * hardGrabSpeedModifier);
                break;
            case GrabStage.Suffocate:
                var chokeSpeedMod = component.ChokeGrabSpeedModifier;
                args.ModifySpeed(component.WalkSpeedModifier * chokeSpeedMod, component.SprintSpeedModifier * chokeSpeedMod);
                break;
            default:
                args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
                break;
        }
    }

    // Goobstation - Grab Intent
    private void OnPullableMoveInput(Entity<PullableComponent> ent, ref MoveInputEvent args)
    {
        // If someone moves then break their pulling.
        if (!ent.Comp.BeingPulled)
            return;

        var entity = args.Entity;

        if (ent.Comp.GrabStage == GrabStage.Soft)
            TryStopPull(ent, ent, ent);

        if (!_blocker.CanMove(entity))
            return;

        TryStopPull(ent, ent, user: ent);
    }
    // Goobstation

    private void OnPullableCollisionChange(EntityUid uid, PullableComponent component, ref CollisionChangeEvent args)
    {
        // IDK what this is supposed to be.
        if (!_timing.ApplyingState && component.PullJointId != null && !args.CanCollide)
        {
            _joints.RemoveJoint(uid, component.PullJointId);
        }
    }

    private void OnJointRemoved(EntityUid uid, PullableComponent component, JointRemovedEvent args)
    {
        // Just handles the joint getting nuked without going through pulling system (valid behavior).

        // Not relevant / pullable state handle it.
        if (component.Puller != args.OtherEntity ||
            args.Joint.ID != component.PullJointId ||
            _timing.ApplyingState)
        {
            return;
        }

        if (args.Joint.ID != component.PullJointId || component.Puller == null)
            return;

        StopPulling(uid, component);
    }

    /// <summary>
    /// Forces pulling to stop and handles cleanup.
    /// </summary>
    private void StopPulling(EntityUid pullableUid, PullableComponent pullableComp)
    {
        if (!_timing.ApplyingState)
        {
            // Joint shutdown
            if (pullableComp.PullJointId != null)
            {
                _joints.RemoveJoint(pullableUid, pullableComp.PullJointId);
                pullableComp.PullJointId = null;
            }

            if (TryComp<PhysicsComponent>(pullableUid, out var pullablePhysics))
            {
                _physics.SetFixedRotation(pullableUid, pullableComp.PrevFixedRotation, body: pullablePhysics);
            }
        }

        var oldPuller = pullableComp.Puller;
        if (oldPuller != null)
            RemComp<ActivePullerComponent>(oldPuller.Value);

        pullableComp.PullJointId = null;
        pullableComp.Puller = null;
        // Goobstation - Grab Intent
        pullableComp.GrabStage = GrabStage.No;
        pullableComp.GrabEscapeChance = 1f;
        _blocker.UpdateCanMove(pullableUid);
        // Goobstation

        Dirty(pullableUid, pullableComp);

        // No more joints with puller -> force stop pull.
        if (TryComp<PullerComponent>(oldPuller, out var pullerComp))
        {
            var pullerUid = oldPuller.Value;
            if (_netManager.IsServer)
                _alertsSystem.ClearAlert(pullerUid, pullerComp.PullingAlert);
            pullerComp.Pulling = null;
            // Goobstation - Grab Intent
            pullerComp.GrabStage = GrabStage.No;
            var virtItems = pullerComp.GrabVirtualItems;
            foreach (var item in virtItems)
                QueueDel(item);

            pullerComp.GrabVirtualItems.Clear();
            // Goobstation
            Dirty(oldPuller.Value, pullerComp);

            // Messaging
            var message = new PullStoppedMessage(pullerUid, pullableUid);
            _modifierSystem.RefreshMovementSpeedModifiers(pullerUid);
            _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(pullerUid):user} stopped pulling {ToPrettyString(pullableUid):target}");

            RaiseLocalEvent(pullerUid, message);
            RaiseLocalEvent(pullableUid, message);
        }

        if (_netManager.IsServer)
            _alertsSystem.ClearAlert(pullableUid, pullableComp.PulledAlert);
    }

    public bool IsPulled(EntityUid uid, PullableComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.BeingPulled;
    }

    public bool IsPulling(EntityUid puller, PullerComponent? component = null)
    {
        return Resolve(puller, ref component, false) && component.Pulling != null;
    }

    private void OnReleasePulledObject(ICommonSession? session)
    {
        if (session?.AttachedEntity is not { Valid: true } player)
        {
            return;
        }

        if (!TryComp(player, out PullerComponent? pullerComp) ||
            !TryComp(pullerComp.Pulling, out PullableComponent? pullableComp))
        {
            return;
        }

        TryStopPull(pullerComp.Pulling.Value, pullableComp, user: player, true); // Goobstation
    }

    public bool CanPull(EntityUid puller, EntityUid pullableUid, PullerComponent? pullerComp = null)
    {
        if (!Resolve(puller, ref pullerComp, false))
        {
            return false;
        }

        if (pullerComp.NeedsHands
            && !_handsSystem.TryGetEmptyHand(puller, out _)
            && pullerComp.Pulling == null)
        {
            if (!_grabbingItem.TryGetGrabbingItem(puller, out _)) // Goobstation
                return false;
        }

        if (!_blocker.CanInteract(puller, pullableUid))
        {
            return false;
        }

        if (!TryComp<PhysicsComponent>(pullableUid, out var physics)) // Goobstation
        {
            return false;
        }

        if (physics.BodyType == BodyType.Static)
        {
            return false;
        }

        if (puller == pullableUid)
        {
            return false;
        }

        if (!_containerSystem.IsInSameOrNoContainer(puller, pullableUid))
        {
            return false;
        }

        var getPulled = new BeingPulledAttemptEvent(puller, pullableUid);
        RaiseLocalEvent(pullableUid, getPulled, true);
        var startPull = new StartPullAttemptEvent(puller, pullableUid);
        RaiseLocalEvent(puller, startPull, true);
        return !startPull.Cancelled && !getPulled.Cancelled;
    }

    // Goobstation - Grab Intent
    public bool TogglePull(Entity<PullableComponent?> pullable, EntityUid pullerUid)
    {
        if (!Resolve(pullable, ref pullable.Comp, false))
            return false;

        if (pullable.Comp.Puller != pullerUid)
            return TryStartPull(pullerUid, pullable, pullableComp: pullable.Comp);

        if (TryGrab((pullable, pullable.Comp), pullerUid))
            return true;

        if (!_combatMode.IsInCombatMode(pullable))
            return TryStopPull(pullable, pullable.Comp, ignoreGrab: true);

        return false;
    }
    // Goobstation


    public bool TogglePull(EntityUid pullerUid, PullerComponent puller)
    {
        if (!TryComp<PullableComponent>(puller.Pulling, out var pullable))
            return false;

        return TogglePull((puller.Pulling.Value, pullable), pullerUid);
    }

    public bool TryStartPull(EntityUid pullerUid, EntityUid pullableUid,
        PullerComponent? pullerComp = null, PullableComponent? pullableComp = null)
    {
        if (!Resolve(pullerUid, ref pullerComp, false) ||
            !Resolve(pullableUid, ref pullableComp, false))
        {
            return false;
        }

        if (pullerComp.Pulling == pullableUid)
            return true;

        if (!CanPull(pullerUid, pullableUid))
            return false;

        if (!TryComp(pullerUid, out PhysicsComponent? pullerPhysics) || !TryComp(pullableUid, out PhysicsComponent? pullablePhysics))
            return false;

        // Ensure that the puller is not currently pulling anything.
        if (TryComp<PullableComponent>(pullerComp.Pulling, out var oldPullable)
            && !TryStopPull(pullerComp.Pulling.Value, oldPullable, pullerUid, true)) // Goobstation
            return false;

        // Stop anyone else pulling the entity we want to pull
        if (pullableComp.Puller != null)
        {
            // We're already pulling this item
            if (pullableComp.Puller == pullerUid)
                return false;
            // Goobstation - Grab Intent
            if (!TryStopPull(pullableUid, pullableComp, pullableComp.Puller))
            {
                // Not succeed to retake grabbed entity
                if (_netManager.IsServer)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-fail",
                            ("puller", Identity.Entity(pullableComp.Puller.Value, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullerUid, pullerUid, PopupType.MediumCaution);
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-fail-puller",
                            ("puller", Identity.Entity(pullerUid, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullableComp.Puller.Value, pullableComp.Puller.Value, PopupType.MediumCaution);
                }
                return false;
            }

            else if (pullableComp.GrabStage != GrabStage.No)
            {
                // Successful retake
                if (_netManager.IsServer)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-success",
                            ("puller", Identity.Entity(pullableComp.Puller.Value, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullerUid, pullerUid, PopupType.MediumCaution);
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-success-puller",
                            ("puller", Identity.Entity(pullerUid, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullableComp.Puller.Value, pullableComp.Puller.Value, PopupType.MediumCaution);
                }
            }
            // Goobstation
        }

        var pullAttempt = new PullAttemptEvent(pullerUid, pullableUid);
        RaiseLocalEvent(pullerUid, pullAttempt);

        if (pullAttempt.Cancelled)
            return false;

        RaiseLocalEvent(pullableUid, pullAttempt);

        if (pullAttempt.Cancelled)
            return false;

        // Pulling confirmed

        _interaction.DoContactInteraction(pullableUid, pullerUid);

        // Use net entity so it's consistent across client and server.
        pullableComp.PullJointId = $"pull-joint-{GetNetEntity(pullableUid)}";

        EnsureComp<ActivePullerComponent>(pullerUid);
        pullerComp.Pulling = pullableUid;
        pullableComp.Puller = pullerUid;

        // store the pulled entity's physics FixedRotation setting in case we change it
        pullableComp.PrevFixedRotation = pullablePhysics.FixedRotation;

        // joint state handling will manage its own state
        if (!_timing.ApplyingState)
        {
            var joint = _joints.CreateDistanceJoint(pullableUid, pullerUid,
                    pullablePhysics.LocalCenter, pullerPhysics.LocalCenter,
                    id: pullableComp.PullJointId);
            joint.CollideConnected = false;
            // This maximum has to be there because if the object is constrained too closely, the clamping goes backwards and asserts.
            // Internally, the joint length has been set to the distance between the pivots.
            // Add an additional 15cm (pretty arbitrary) to the maximum length for the hard limit.
            joint.MaxLength = joint.Length + 0.15f;
            joint.MinLength = 0f;
            // Set the spring stiffness to zero. The joint won't have any effect provided
            // the current length is beteen MinLength and MaxLength. At those limits, the
            // joint will have infinite stiffness.
            joint.Stiffness = 0f;

            _physics.SetFixedRotation(pullableUid, pullableComp.FixedRotationOnPull, body: pullablePhysics);
        }

        // Messaging
        var message = new PullStartedMessage(pullerUid, pullableUid);
        _modifierSystem.RefreshMovementSpeedModifiers(pullerUid);
        _alertsSystem.ShowAlert(pullerUid, pullerComp.PullingAlert, 0); // Goobstation
        _alertsSystem.ShowAlert(pullableUid, pullableComp.PulledAlert, 0); // Goobstation

        RaiseLocalEvent(pullerUid, message);
        RaiseLocalEvent(pullableUid, message);

        Dirty(pullerUid, pullerComp);
        Dirty(pullableUid, pullableComp);

        var pullingMessage =
            Loc.GetString("getting-pulled-popup", ("puller", Identity.Entity(pullerUid, EntityManager)));
        _popup.PopupEntity(pullingMessage, pullableUid, pullableUid);

        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(pullerUid):user} started pulling {ToPrettyString(pullableUid):target}");

        if (_combatMode.IsInCombatMode(pullerUid)) // Goobstation
            TryGrab(pullableUid, pullerUid); // Goobstation

        return true;
    }

    public bool TryStopPull(EntityUid pullableUid, PullableComponent pullable, EntityUid? user = null, bool ignoreGrab = false)
    {
        var pullerUidNull = pullable.Puller;

        if (pullerUidNull == null)
            return true;

        if (user != null && !_blocker.CanInteract(user.Value, pullableUid))
            return false;

        var msg = new AttemptStopPullingEvent(user);
        RaiseLocalEvent(pullableUid, ref msg, true); // Goob edit

        if (msg.Cancelled)
            return false;


        // Goobstation - Grab Intent
        if (!ignoreGrab)
        {
            if (_netManager.IsServer && user != null && user.Value == pullableUid)
            {
                var releaseAttempt = AttemptGrabRelease(pullableUid);
                if (!releaseAttempt)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-release-fail-self"),
                        pullableUid,
                        pullableUid,
                        PopupType.SmallCaution);
                    return false;
                }

                _popup.PopupEntity(Loc.GetString("popup-grab-release-success-self"),
                    pullableUid,
                    pullableUid,
                    PopupType.SmallCaution);
                _popup.PopupEntity(
                    Loc.GetString("popup-grab-release-success-puller",
                        ("target", Identity.Entity(pullableUid, EntityManager))),
                    pullerUidNull.Value,
                    pullerUidNull.Value,
                    PopupType.MediumCaution);
            }
        }
        // Goobstation
        StopPulling(pullableUid, pullable);
        return true;
    }

    public void StopAllPulls(EntityUid uid) // Goobstation
    {
        if (TryComp<PullableComponent>(uid, out var pullable) && IsPulled(uid, pullable))
            TryStopPull(uid, pullable);

        if (TryComp<PullerComponent>(uid, out var puller) &&
            TryComp(puller.Pulling, out PullableComponent? pullableEnt))
            TryStopPull(puller.Pulling.Value, pullableEnt);
    }

    // Goobstation - Grab Intent
    /// <summary>
    /// Trying to grab the target
    /// </summary>
    /// <param name="pullable">Target that would be grabbed</param>
    /// <param name="puller">Performer of the grab</param>
    /// <param name="ignoreCombatMode">If true, will ignore disabled combat mode</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public bool TryGrab(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (!Resolve(puller.Owner, ref puller.Comp))
            return false;

        // prevent you from grabbing someone else while being grabbed
        if (TryComp<PullableComponent>(puller.Owner, out var pullerAsPullable) && pullerAsPullable.Puller != null)
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        if (puller.Comp.NextStageChange > _timing.CurTime)
            return true;

        // You can't choke crates
        if (!HasComp<MobStateComponent>(pullable))
            return false;

        // Delay to avoid spamming
        puller.Comp.NextStageChange = _timing.CurTime + puller.Comp.StageChangeCooldown;
        Dirty(puller);

        // Don't grab without grab intent
        if (!ignoreCombatMode)
            if (!_combatMode.IsInCombatMode(puller.Owner))
                return false;

        // It's blocking stage update, maybe better UX?
        if (puller.Comp.GrabStage == GrabStage.Suffocate)
        {
            _stamina.TakeStaminaDamage(pullable, puller.Comp.SuffocateGrabStaminaDamage);

            Dirty(pullable);
            Dirty(puller);
            return true;
        }

        // Update stage
        // TODO: Change grab stage direction
        var nextStageAddition = puller.Comp.GrabStageDirection switch
        {
            GrabStageDirection.Increase => 1,
            GrabStageDirection.Decrease => -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var newStage = puller.Comp.GrabStage + nextStageAddition;
        var ev = new CheckGrabOverridesEvent(newStage); // guh
        RaiseLocalEvent(puller, ev);
        newStage = ev.Stage;

        if (!TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage))
            return false;

        _color.RaiseEffect(Color.Yellow, new List<EntityUid> { pullable }, Filter.Pvs(pullable, entityManager: EntityManager));
        return true;
    }

    private bool TrySetGrabStages(Entity<PullerComponent> puller, Entity<PullableComponent> pullable, GrabStage stage)
    {
        puller.Comp.GrabStage = stage;
        pullable.Comp.GrabStage = stage;

        if (!TryUpdateGrabVirtualItems(puller, pullable))
            return false;

        var filter = Filter.Empty()
            .AddPlayersByPvs(Transform(puller).Coordinates)
            .RemovePlayerByAttachedEntity(puller.Owner)
            .RemovePlayerByAttachedEntity(pullable.Owner);

        var popupType = stage switch
        {
            GrabStage.No => PopupType.Small,
            GrabStage.Soft => PopupType.Small,
            GrabStage.Hard => PopupType.MediumCaution,
            GrabStage.Suffocate => PopupType.LargeCaution,
            _ => throw new ArgumentOutOfRangeException()
        };

        var massModifier = _contests.MassContest(puller, pullable);
        pullable.Comp.GrabEscapeChance = Math.Clamp(puller.Comp.EscapeChances[stage] / massModifier, 0f, 1f);

        _alertsSystem.ShowAlert(puller, puller.Comp.PullingAlert, puller.Comp.PullingAlertSeverity[stage]);
        _alertsSystem.ShowAlert(pullable, pullable.Comp.PulledAlert, pullable.Comp.PulledAlertAlertSeverity[stage]);

        _blocker.UpdateCanMove(pullable);
        _modifierSystem.RefreshMovementSpeedModifiers(puller);

        // I'm lazy to write client code
        if (!_netManager.IsServer)
            return true;

        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-target", ("puller", Identity.Entity(puller, EntityManager))), pullable, pullable, popupType);
        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-self", ("target", Identity.Entity(pullable, EntityManager))), pullable, puller, PopupType.Medium);
        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-others", ("target", Identity.Entity(pullable, EntityManager)), ("puller", Identity.Entity(puller, EntityManager))), pullable, filter, true, popupType);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), pullable);

        var comboEv = new ComboAttackPerformedEvent(puller.Owner, pullable.Owner, puller.Owner, ComboAttackType.Grab);
        RaiseLocalEvent(puller.Owner, comboEv);

        Dirty(pullable);
        Dirty(puller);

        return true;
    }

    private bool TryUpdateGrabVirtualItems(Entity<PullerComponent> puller, Entity<PullableComponent> pullable)
    {
        // Updating virtual items
        var virtualItemsCount = puller.Comp.GrabVirtualItems.Count;

        var newVirtualItemsCount = puller.Comp.NeedsHands ? 0 : 1;
        if (puller.Comp.GrabVirtualItemStageCount.TryGetValue(puller.Comp.GrabStage, out var count))
            newVirtualItemsCount += count;

        if (virtualItemsCount == newVirtualItemsCount)
            return true;
        var delta = newVirtualItemsCount - virtualItemsCount;

        // Adding new virtual items
        if (delta > 0)
        {
            for (var i = 0; i < delta; i++)
            {
                var emptyHand = _handsSystem.TryGetEmptyHand(puller, out _);
                if (!emptyHand)
                {
                    if (_netManager.IsServer)
                        _popup.PopupEntity(Loc.GetString("popup-grab-need-hand"), puller, puller, PopupType.Medium);

                    return false;
                }

                if (!_virtualSystem.TrySpawnVirtualItemInHand(pullable, puller.Owner, out var item, true))
                {
                    // I'm lazy write client code
                    if (_netManager.IsServer)
                        _popup.PopupEntity(Loc.GetString("popup-grab-need-hand"), puller, puller, PopupType.Medium);

                    return false;
                }

                puller.Comp.GrabVirtualItems.Add(item.Value);
            }
        }

        if (delta >= 0)
            return true;
        for (var i = 0; i < Math.Abs(delta); i++)
        {
            if (i >= puller.Comp.GrabVirtualItems.Count)
                break;

            var item = puller.Comp.GrabVirtualItems[i];
            puller.Comp.GrabVirtualItems.Remove(item);
            if(TryComp<VirtualItemComponent>(item, out var virtualItemComponent))
                _virtualSystem.DeleteVirtualItem((item,virtualItemComponent), puller);
        }

        return true;
    }

    /// <summary>
    /// Attempts to release entity from grab
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <returns></returns>
    public bool AttemptGrabRelease(Entity<PullableComponent?> pullable)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (_timing.CurTime < pullable.Comp.NextEscapeAttempt)  // No autoclickers! Mwa-ha-ha
            return false;

        if (_random.Prob(pullable.Comp.GrabEscapeChance))
            return true;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(3));
        Dirty(pullable.Owner, pullable.Comp);
        return false;
    }

    private void OnGrabbedMoveAttempt(EntityUid uid, PullableComponent component, UpdateCanMoveEvent args)
    {
        if (component.GrabStage == GrabStage.No)
            return;

        args.Cancel();

    }

    private void OnGrabbedSpeakAttempt(EntityUid uid, PullableComponent component, SpeakAttemptEvent args)
    {
        if (component.GrabStage != GrabStage.Suffocate)
            return;

        _popup.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"), uid, uid, PopupType.MediumCaution);   // You cant speak while someone is choking you

        args.Cancel();
    }

    /// <summary>
    /// Tries to lower grab stage for target or release it
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <param name="puller">Performer</param>
    /// <param name="ignoreCombatMode">If true, will NOT release target if combat mode is off</param>
    /// <returns></returns>
    public bool TryLowerGrabStage(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (!Resolve(puller.Owner, ref puller.Comp))
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        if (_timing.CurTime < puller.Comp.NextStageChange)
            return true;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1f));
        Dirty(pullable);
        Dirty(puller);

        if (!ignoreCombatMode && _combatMode.IsInCombatMode(puller.Owner))
        {
            TryStopPull(pullable, pullable.Comp, ignoreGrab: true);
            return true;
        }

        if (puller.Comp.GrabStage == GrabStage.No)
        {
            TryStopPull(pullable, pullable.Comp, ignoreGrab: true);
            return true;
        }

        var newStage = puller.Comp.GrabStage - 1;
        TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage);
        return true;
    }
}

public enum GrabStage
{
    No = 0,
    Soft = 1,
    Hard = 2,
    Suffocate = 3,
}

public enum GrabStageDirection
{
    Increase,
    Decrease,
}

// Goobstation
