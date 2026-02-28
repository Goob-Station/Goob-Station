using Content.Goobstation.Shared.PhaseShift;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Network;
using Content.Shared.Damage;
using Content.Shared.Tag;
using Content.Shared.Doors.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Speech.Muting;
using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Body.Components;
using Content.Goobstation.Common.Temperature.Components;
using Content.Shared.Physics;
using Content.Shared.DoAfter;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Components;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Hands;
using Content.Shared.Standing;
using Content.Goobstation.Shared.Supermatter.Components;
using Content.Shared.Body.Part;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics;
using Content.Shared.Throwing;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed class SlasherIncorporealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly SlasherObserverCheckSystem _observerCheck = default!;

    private const string FootstepSoundTag = "FootstepSound";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherIncorporealComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherIncorporealComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherIncorporealizeEvent>(OnIncorporealize);
        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherCorporealizeEvent>(OnCorporealize);
        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherIncorporealizeDoAfterEvent>(OnIncorporealizeDoAfter);

        SubscribeLocalEvent<SlasherIncorporealComponent, BeforeThrowEvent>(OnBeforeThrow);
        SubscribeLocalEvent<SlasherIncorporealComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, InteractionAttemptEvent>(OnAttemptInteract);
        SubscribeLocalEvent<SlasherIncorporealComponent, PullStoppedMessage>(OnPullStopped);
        SubscribeLocalEvent<DamageableComponent, BeforeDamageChangedEvent>(OnBeforeDamageBodyPart);
        SubscribeLocalEvent<ActionComponent, ActionAttemptEvent>(OnAnyActionAttempt);
    }

    private void OnMapInit(Entity<SlasherIncorporealComponent> ent, ref MapInitEvent args)
    {
        if (!_net.IsServer)
            return;

        _actions.AddAction(ent.Owner, ref ent.Comp.IncorporealizeActionEnt, ent.Comp.IncorporealizeActionId);
        _actions.AddAction(ent.Owner, ref ent.Comp.CorporealizeActionEnt, ent.Comp.CorporealizeActionId);
        _actions.SetEnabled(ent.Comp.CorporealizeActionEnt, false);

        EnsureComp<SlasherObserverCheckComponent>(ent);
    }

    private void OnShutdown(Entity<SlasherIncorporealComponent> ent, ref ComponentShutdown args)
    {
        if (!_net.IsServer)
            return;

        _actions.RemoveAction(ent.Owner, ent.Comp.IncorporealizeActionEnt);
        _actions.RemoveAction(ent.Owner, ent.Comp.CorporealizeActionEnt);
    }

    private void OnIncorporealize(Entity<SlasherIncorporealComponent> ent, ref SlasherIncorporealizeEvent args)
    {
        if (args.Handled || ent.Comp.IsIncorporeal)
            return;

        // Check if anyone can see them
        if (_observerCheck.IsObservedByPlayers(ent.Owner, ent.Comp.ObserverCheckRange))
        {
            _popup.PopupPredicted(Loc.GetString("slasher-incorporealize-fail-seen"), ent.Owner, ent.Owner);
            args.Handled = true;
            return;
        }

        var doArgs = new DoAfterArgs(EntityManager,
            ent.Owner,
            ent.Comp.IncorporealizeDelay,
            new SlasherIncorporealizeDoAfterEvent(),
            ent.Owner,
            target: ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BlockDuplicate = true,
            Hidden = false,
            DistanceThreshold = 1f,
        };
        _doAfter.TryStartDoAfter(doArgs);

        args.Handled = true;
    }

    private void OnCorporealize(Entity<SlasherIncorporealComponent> ent, ref SlasherCorporealizeEvent args)
    {
        if (args.Handled)
            return;

        if (!ent.Comp.IsIncorporeal)
        {
            args.Handled = true;
            return;
        }

        if (_net.IsServer)
        {
            // Check if anyone can see them.
            if (_observerCheck.IsObservedByPlayers(ent.Owner, ent.Comp.ObserverCheckRange))
            {
                _popup.PopupEntity(Loc.GetString("slasher-corporealize-fail-nearby"), ent.Owner, ent.Owner);
                args.Handled = true;
                return;
            }

            // Check if any active surveillance cameras have line of sight.
            var camEv = new SlasherIncorporealCameraCheckEvent(GetNetEntity(ent.Owner), ent.Comp.ObserverCheckRange);
            RaiseLocalEvent(ref camEv);
            if (camEv.Cancelled)
            {
                _popup.PopupEntity(Loc.GetString("slasher-corporealize-fail-camera"), ent.Owner, ent.Owner);
                args.Handled = true;
                return;
            }

            // Check if the slasher is currently inside a wall or solid object.
            if (IsInsideSolidObject(ent.Owner))
            {
                _popup.PopupEntity(Loc.GetString("slasher-corporealize-fail-inside-wall"), ent.Owner, ent.Owner);
                args.Handled = true;
                return;
            }
        }

        ExitIncorporeal(ent.Owner, ent);
        args.Handled = true;
    }

    private void OnIncorporealizeDoAfter(Entity<SlasherIncorporealComponent> ent, ref SlasherIncorporealizeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (_observerCheck.IsObservedByPlayers(ent.Owner, ent.Comp.ObserverCheckRange))
        {
            _popup.PopupPredicted(Loc.GetString("slasher-incorporealize-fail-seen"), ent.Owner, ent.Owner);
            return;
        }

        EnterIncorporeal(ent.Owner, ent);
        args.Handled = true;
    }

    private void EnterIncorporeal(EntityUid uid, Entity<SlasherIncorporealComponent> ent)
    {
        ent.Comp.IsIncorporeal = true;
        ent.Comp.IncorporealStartTime = _timing.CurTime;
        Dirty(ent);

        // Spawn jaunt in effect.
        var coords = Transform(uid).Coordinates;
        Spawn(ent.Comp.JauntInEffect, coords);
        _audio.PlayPredicted(ent.Comp.JauntDisappear, coords, ent.Owner);

        // Freeze all action cooldowns.
        FreezeCooldowns((uid, ent.Comp));

        // Force stand up when entering incorporeal.
        _standing.Stand(uid, force: true);

        var phase = new PhaseShiftedComponent
        {
            SpawnEffects = false,
            MovementSpeedBuff = 5.5f
        };
        AddComp(uid, phase);

        // don't wanna let people see them obviously.
        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, stealth.MinVisibility, stealth);
        _stealth.SetThermalsImmune(uid, true, stealth);

        _actions.SetEnabled(ent.Comp.IncorporealizeActionEnt, false);
        _actions.SetEnabled(ent.Comp.CorporealizeActionEnt, true);

        // Prevent doors from opening.
        if (_tags.HasTag(uid, SharedDoorSystem.DoorBumpTag))
            _tags.RemoveTag(uid, SharedDoorSystem.DoorBumpTag);

        // Remove footstep sounds while incorporeal.
        if (_tags.HasTag(uid, FootstepSoundTag))
            _tags.RemoveTag(uid, FootstepSoundTag);

        // Mute and block vocal emotes.
        _ = EnsureComp<MutedComponent>(uid);

        // Disable FOV for full vision while incorporeal.
        _eye.SetDrawFov(uid, false);

        // Space immunity
        _ = EnsureComp<MovementIgnoreGravityComponent>(uid);
        _ = EnsureComp<SpecialPressureImmunityComponent>(uid);
        _ = EnsureComp<SpecialBreathingImmunityComponent>(uid);
        _ = EnsureComp<SpecialLowTempImmunityComponent>(uid);
        _ = EnsureComp<SpecialHighTempImmunityComponent>(uid);

        // Supermatter immunity
        _ = EnsureComp<SupermatterImmuneComponent>(uid);

        // Raise event for server systems to handle additional logic (like disabling lights)
        var enteredEv = new SlasherIncorporealEnteredEvent();
        RaiseLocalEvent(uid, ref enteredEv);
    }

    private void ExitIncorporeal(EntityUid uid, Entity<SlasherIncorporealComponent> ent)
    {
        ent.Comp.IsIncorporeal = false;
        Dirty(ent);

        // Spawn jaunt out effect
        var coords = Transform(uid).Coordinates;
        Spawn(ent.Comp.JauntOutEffect, coords);
        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.JauntAppear, coords);

        // Restore frozen cooldowns
        UnfreezeCooldowns((uid, ent.Comp));

        ent.Comp.IncorporealStartTime = null;

        if (TryComp<PhaseShiftedComponent>(uid, out var phase))
        {
            phase.SpawnEffects = false;
            Dirty(uid, phase);
        }

        if (HasComp<PhaseShiftedComponent>(uid))
            RemComp<PhaseShiftedComponent>(uid);

        if (HasComp<StealthComponent>(uid))
            RemComp<StealthComponent>(uid);

        _actions.SetEnabled(ent.Comp.IncorporealizeActionEnt, true);
        _actions.SetEnabled(ent.Comp.CorporealizeActionEnt, false);

        _tags.AddTag(uid, SharedDoorSystem.DoorBumpTag);

        _tags.AddTag(uid, FootstepSoundTag);

        // Let them speak
        _ = RemComp<MutedComponent>(uid);

        // Restore FOV
        _eye.SetDrawFov(uid, true);

        // Remove space immunity
        _ = RemComp<MovementIgnoreGravityComponent>(uid);
        _ = RemComp<SpecialPressureImmunityComponent>(uid);
        _ = RemComp<SpecialBreathingImmunityComponent>(uid);
        _ = RemComp<SpecialLowTempImmunityComponent>(uid);
        _ = RemComp<SpecialHighTempImmunityComponent>(uid);

        // Remove supermatter immunity
        _ = RemComp<SupermatterImmuneComponent>(uid);
    }

    // Goida as shit.. I couldn't find a better way stop cooldowns
    private void FreezeCooldowns(Entity<SlasherIncorporealComponent> ent)
    {
        if (!_net.IsServer)
            return;

        ent.Comp.FrozenCooldowns.Clear();

        var curTime = _timing.CurTime;
        foreach (var action in _actions.GetActions(ent.Owner))
            if (action.Comp.Cooldown is { } cooldown && cooldown.End > curTime)
            {
                // Store the remaining cooldown duration
                var remaining = cooldown.End - curTime;
                ent.Comp.FrozenCooldowns[action.Owner] = remaining;

                // Make the cooldown basically never end to pause it
                _actions.SetCooldown(action.Owner, cooldown.Start, TimeSpan.MaxValue);

            }
    }

    private void UnfreezeCooldowns(Entity<SlasherIncorporealComponent> ent)
    {
        if (!_net.IsServer)
            return;

        var curTime = _timing.CurTime;
        foreach (var (actionId, remainingTime) in ent.Comp.FrozenCooldowns)
            _actions.SetCooldown(actionId, curTime, curTime + remainingTime);

        ent.Comp.FrozenCooldowns.Clear();
    }

    // Do note that you can grab someone that's in crit or dead and then use incorporealize to drag their body around while invisible but this is hilarious so I'm keeping it
    private void OnPullAttempt(EntityUid uid, SlasherIncorporealComponent comp, PullAttemptEvent args)
    {
        if (comp.IsIncorporeal
            && args.PullerUid == uid)
            args.Cancelled = true;
    }

    private void OnBeforeDamageBodyPart(EntityUid uid, DamageableComponent damageable, ref BeforeDamageChangedEvent args)
    {
        // Check if this is a body part, and if so, check if the parent body is an incorporeal slasher
        if (!TryComp<BodyPartComponent>(uid, out var bodyPart) || bodyPart.Body == null)
            return;

        // Check if the parent body has the incorporeal component and is incorporeal
        if (TryComp<SlasherIncorporealComponent>(bodyPart.Body.Value, out var slasherComp) && slasherComp.IsIncorporeal)
            args.Cancelled = true;
    }

    private void OnDropAttempt(EntityUid uid, SlasherIncorporealComponent comp, DropAttemptEvent args)
    {
        if (!comp.IsIncorporeal
            || HasComp<VirtualItemComponent>(args.Uid))
            return;

        args.Cancel();
    }

    private void OnAttemptInteract(EntityUid uid, SlasherIncorporealComponent comp, ref InteractionAttemptEvent args)
    {
        if (!comp.IsIncorporeal || args.Target == null)
            return;

        // Allow them to stop pulling things
        if (TryComp<PullerComponent>(uid, out var puller)
            && puller.Pulling == args.Target
            || HasComp<VirtualItemComponent>(args.Target.Value))
            return;

        args.Cancelled = true;
    }

    private void OnPullStopped(EntityUid uid, SlasherIncorporealComponent comp, PullStoppedMessage args)
    {
        // Manually clean up virtual items used for pulling if incorporeal
        if (!comp.IsIncorporeal
            || !TryComp<HandsComponent>(uid, out var handsComp))
            return;

        foreach (var hand in handsComp.Hands.Keys)
        {
            if (!_hands.TryGetHeldItem((uid, handsComp), hand, out var held))
                continue;

            if (!TryComp<VirtualItemComponent>(held, out var virtItem)
                || virtItem.BlockingEntity != args.PulledUid)
                continue;

            _virtualItem.DeleteVirtualItem((held.Value, virtItem), uid);
        }
    }

    private void OnAnyActionAttempt(Entity<ActionComponent> action, ref ActionAttemptEvent args)
    {
        var user = args.User;
        if (!TryComp<SlasherIncorporealComponent>(user, out var comp) || !comp.IsIncorporeal)
            return;

        // Allow nightvision / corporealize.
        if (comp.CorporealizeActionEnt == action.Owner
            || _actions.GetEvent(action.Owner) is ToggleNightVisionEvent)
            return;

        args.Cancelled = true;
    }

    private void OnBeforeThrow(Entity<SlasherIncorporealComponent> ent, ref BeforeThrowEvent args)
    {
        if (ent.Comp.IsIncorporeal
            && ent.Owner == args.PlayerUid)
            args.Cancelled = true;
    }

    private void OnAttackAttempt(EntityUid uid, SlasherIncorporealComponent comp, AttackAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    private bool IsInsideSolidObject(EntityUid uid)
    {
        if (!TryComp<PhysicsComponent>(uid, out _))
            return false;

        var slasherTransform = _physics.GetPhysicsTransform(uid);
        var entities = _lookup.GetEntitiesInRange(uid, 0.5f, LookupFlags.Static | LookupFlags.Dynamic | LookupFlags.Sundries);

        foreach (var entity in entities)
        {
            if (entity == uid)
                continue;

            if (!TryComp<PhysicsComponent>(entity, out var physics))
                continue;

            if (!physics.CanCollide || !physics.Hard)
                continue;

            if ((physics.CollisionLayer & (int) CollisionGroup.Impassable) == 0
                && (physics.CollisionLayer & (int) CollisionGroup.MidImpassable) == 0
                && (physics.CollisionLayer & (int) CollisionGroup.HighImpassable) == 0
                && (physics.CollisionLayer & (int) CollisionGroup.TabletopMachineLayer) == 0)
                continue;

            var entityTransform = _physics.GetPhysicsTransform(entity);

            if (!TryComp<FixturesComponent>(entity, out var fixturesComp))
                continue;

            foreach (var fixture in fixturesComp.Fixtures.Values)
            {
                if (!fixture.Hard)
                    continue;

                if (_fixtures.TestPoint(fixture.Shape, entityTransform, slasherTransform.Position))
                    return true;
            }
        }

        return false;
    }
}
