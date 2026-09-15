using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;

using Content.Shared._Oskarrr.Yautja.Components;

namespace Content.Shared._Oskarrr.Yautja.Systems;

public sealed class SharedYautjaBracerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    /// <summary>Claws currently being moved into the bracer — ignore DroppedEvent delete.</summary>
    private readonly HashSet<EntityUid> _storingClaws = [];

    /// <summary>Shield currently being moved into the bracer — ignore DroppedEvent delete.</summary>
    private readonly HashSet<EntityUid> _storingShield = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YautjaBracerComponent, MapInitEvent>(OnBracerMapInit);
        SubscribeLocalEvent<YautjaBracerComponent, ToggleYautjaClawsEvent>(OnToggleClaws);
        SubscribeLocalEvent<YautjaBracerComponent, ToggleYautjaShieldEvent>(OnToggleShield);
        SubscribeLocalEvent<YautjaBracerComponent, ToggleYautjaCloakEvent>(OnToggleCloak);
        SubscribeLocalEvent<YautjaBracerComponent, YautjaBracerSelfDestructEvent>(OnSelfDestruct);
        SubscribeLocalEvent<YautjaBracerComponent, GotUnequippedEvent>(OnBracerUnequipped);
        SubscribeLocalEvent<YautjaBracerComponent, ComponentShutdown>(OnBracerShutdown);
        SubscribeLocalEvent<YautjaBracerClawsComponent, ComponentShutdown>(OnClawsShutdown);
        SubscribeLocalEvent<YautjaBracerClawsComponent, DroppedEvent>(OnClawsDropped);
        SubscribeLocalEvent<YautjaBracerShieldComponent, ComponentShutdown>(OnShieldShutdown);
        SubscribeLocalEvent<YautjaBracerShieldComponent, DroppedEvent>(OnShieldDropped);
        SubscribeLocalEvent<YautjaBracerCloakTrackerComponent, MoveEvent>(OnCloakTrackerMove);
        SubscribeLocalEvent<YautjaBracerCloakTrackerComponent, AttackAttemptEvent>(OnCloakedAttackAttempt);
        SubscribeLocalEvent<YautjaBracerCloakTrackerComponent, ShotAttemptedEvent>(OnCloakedShotAttempt);
        SubscribeLocalEvent<YautjaCloakPackComponent, GotUnequippedEvent>(OnCloakPackUnequipped);
        SubscribeLocalEvent<MobStateChangedEvent>(OnWearerMobStateChanged);
    }

    private void OnCloakedAttackAttempt(
        Entity<YautjaBracerCloakTrackerComponent> ent,
        ref AttackAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnCloakedShotAttempt(
        Entity<YautjaBracerCloakTrackerComponent> ent,
        ref ShotAttemptedEvent args)
    {
        args.Cancel();
    }

    private void OnBracerMapInit(Entity<YautjaBracerComponent> ent, ref MapInitEvent args)
    {
        _containers.EnsureContainer<Container>(ent.Owner, YautjaBracerComponent.ClawsContainerId);
        _containers.EnsureContainer<Container>(ent.Owner, YautjaBracerComponent.ShieldContainerId);
    }

    private void OnCloakPackUnequipped(Entity<YautjaCloakPackComponent> ent, ref GotUnequippedEvent args)
    {
        if (args.Slot != "back")
            return;

        DecloakUser(args.Equipee);
    }

    private void OnCloakTrackerMove(Entity<YautjaBracerCloakTrackerComponent> ent, ref MoveEvent args)
    {
        if (_timing.ApplyingState)
            return;

        KeepCloaked(ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<YautjaBracerComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var ent = (uid, comp);

            // Keep cloak locked at full invis without Dirty-spamming every client frame.
            if (comp.Cloaked && comp.CloakUser is { } user && !TerminatingOrDeleted(user))
                KeepCloaked(user);

            if (!comp.SelfDestructing || comp.SelfDestructAt == null || now < comp.SelfDestructAt)
                continue;

            switch (comp.SelfDestructPhase)
            {
                case YautjaBracerSelfDestructPhase.Arming:
                    BeginSelfDestructCountdown(ent);
                    break;
                case YautjaBracerSelfDestructPhase.Countdown:
                    Detonate(ent);
                    break;
            }
        }
    }

    private void OnToggleClaws(Entity<YautjaBracerComponent> ent, ref ToggleYautjaClawsEvent args)
    {
        if (args.Handled)
            return;

        var extended = AreClawsExtended(ent, args.Performer);

        if (!extended)
        {
            if (!TryExtendClaws(ent, args.Performer))
                return;
        }
        else
        {
            RetractClaws(ent);
        }

        args.Toggle = true;
        args.Handled = true;
    }

    private void OnToggleShield(Entity<YautjaBracerComponent> ent, ref ToggleYautjaShieldEvent args)
    {
        if (args.Handled)
            return;

        var extended = IsShieldExtended(ent, args.Performer);

        if (!extended)
        {
            if (!TryExtendShield(ent, args.Performer))
                return;
        }
        else
        {
            RetractShield(ent);
        }

        args.Toggle = true;
        args.Handled = true;
    }

    private void OnToggleCloak(Entity<YautjaBracerComponent> ent, ref ToggleYautjaCloakEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;

        if (!ent.Comp.Cloaked)
        {
            if (!HasEquippedCloakPack(user))
            {
                _popup.PopupPredicted(Loc.GetString("yautja-bracer-cloak-need-pack"), user, user);
                args.Handled = true;
                return;
            }

            ActivateCloak(ent, user);
        }
        else
        {
            Decloak(ent);
        }

        args.Toggle = true;
        args.Handled = true;
    }

    private bool HasEquippedCloakPack(EntityUid user)
    {
        return _inventory.TryGetSlotEntity(user, "back", out var back)
               && HasComp<YautjaCloakPackComponent>(back);
    }

    private void DecloakUser(EntityUid user)
    {
        var query = EntityQueryEnumerator<YautjaBracerComponent>();
        while (query.MoveNext(out var uid, out var bracer))
        {
            if (!bracer.Cloaked || bracer.CloakUser != user)
                continue;

            Decloak((uid, bracer));
            SyncCloakActionToggle(user, false);
        }
    }

    private void ActivateCloak(Entity<YautjaBracerComponent> ent, EntityUid user)
    {
        RemComp<StealthOnMoveComponent>(user);

        var stealth = EnsureComp<StealthComponent>(user);
        _stealth.SetRevealOnAttack((user, stealth), false);
        _stealth.SetRevealOnDamage((user, stealth), false);
        _stealth.SetThermalsImmune(user, true, stealth);

        // Force client shader rebind on every re-cloak (SetEnabled no-ops if already true).
        _stealth.SetEnabled(user, false, stealth);
        _stealth.SetEnabled(user, true, stealth);
        _stealth.SetVisibility(user, stealth.MinVisibility, stealth);
        Dirty(user, stealth);

        var tracker = EnsureComp<YautjaBracerCloakTrackerComponent>(user);
        tracker.Bracer = ent.Owner;
        Dirty(user, tracker);

        ent.Comp.Cloaked = true;
        ent.Comp.CloakUser = user;
        Dirty(ent);

        if (_net.IsServer)
            Spawn(ent.Comp.CloakDisappearEffect, Transform(user).Coordinates);

        _audio.PlayPredicted(ent.Comp.CloakOnSound, user, user);
    }

    private void Decloak(Entity<YautjaBracerComponent> ent)
    {
        if (!ent.Comp.Cloaked)
            return;

        if (ent.Comp.CloakUser is { } target && !TerminatingOrDeleted(target))
        {
            RemComp<YautjaBracerCloakTrackerComponent>(target);
            RemComp<StealthOnMoveComponent>(target);

            if (TryComp<StealthComponent>(target, out var stealth))
            {
                _stealth.SetEnabled(target, false, stealth);
                RemCompDeferred<StealthComponent>(target);
            }

            _audio.PlayPredicted(ent.Comp.CloakOffSound, target, target);
        }

        ent.Comp.Cloaked = false;
        ent.Comp.CloakUser = null;
        Dirty(ent);
    }

    private void SyncCloakActionToggle(EntityUid user, bool toggled)
    {
        SyncInstantActionToggle<ToggleYautjaCloakEvent>(user, toggled);
    }

    private void SyncInstantActionToggle<TEvent>(EntityUid user, bool toggled) where TEvent : InstantActionEvent
    {
        foreach (var (actionUid, _) in _actions.GetActions(user))
        {
            if (!TryComp<InstantActionComponent>(actionUid, out var instant)
                || instant.Event is not TEvent)
            {
                continue;
            }

            _actions.SetToggled(actionUid, toggled);
        }
    }

    private void KeepCloaked(EntityUid user)
    {
        if (!TryComp<StealthComponent>(user, out var stealth))
            return;

        if (!stealth.Enabled)
            _stealth.SetEnabled(user, true, stealth);

        if (_stealth.GetVisibility(user, stealth) > stealth.MinVisibility)
            _stealth.SetVisibility(user, stealth.MinVisibility, stealth);
    }

    private void OnSelfDestruct(Entity<YautjaBracerComponent> ent, ref YautjaBracerSelfDestructEvent args)
    {
        if (args.Handled)
            return;

        if (!TryBeginSelfDestruct(ent, args.Performer, args.Action))
            return;

        args.Handled = true;
    }

    private void OnWearerMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Critical || args.OldMobState == MobState.Critical)
            return;

        if (!_inventory.TryGetSlotEntity(args.Target, "gloves", out var gloves)
            || !TryComp<YautjaBracerComponent>(gloves, out var bracer))
        {
            return;
        }

        TryBeginSelfDestruct((gloves.Value, bracer), args.Target);
    }

    /// <summary>
    /// Starts the bracer self-destruct sequence. Returns false if already running or invalid.
    /// </summary>
    private bool TryBeginSelfDestruct(Entity<YautjaBracerComponent> ent, EntityUid user, EntityUid? action = null)
    {
        if (ent.Comp.SelfDestructing)
            return false;

        RetractClaws(ent);
        RetractShield(ent);

        _audio.PlayPredicted(ent.Comp.SelfDestructDoAfterSound, user, user);
        _popup.PopupPredicted(Loc.GetString("yautja-bracer-self-destruct-started"), user, user);

        if (_net.IsServer)
        {
            var doAfterSound = _audio.ResolveSound(ent.Comp.SelfDestructDoAfterSound);
            ent.Comp.SelfDestructing = true;
            ent.Comp.SelfDestructPhase = YautjaBracerSelfDestructPhase.Arming;
            ent.Comp.SelfDestructUser = user;
            ent.Comp.SelfDestructAction = action;
            ent.Comp.SelfDestructAt = _timing.CurTime + _audio.GetAudioLength(doAfterSound);
            Dirty(ent);
        }

        return true;
    }

    private void BeginSelfDestructCountdown(Entity<YautjaBracerComponent> ent)
    {
        var user = ent.Comp.SelfDestructUser;
        if (user is not { } target || TerminatingOrDeleted(target))
        {
            CancelSelfDestruct(ent);
            return;
        }

        ent.Comp.SelfDestructPhase = YautjaBracerSelfDestructPhase.Countdown;
        ent.Comp.SelfDestructAt = _timing.CurTime + ent.Comp.SelfDestructCountdown;
        Dirty(ent);

        _audio.PlayPvs(ent.Comp.SelfDestructCountdownSound, target);
        _popup.PopupEntity(Loc.GetString("yautja-bracer-self-destruct-countdown"), target, target);

        if (ent.Comp.SelfDestructAction is { } action)
            _actions.SetCooldown(action, ent.Comp.SelfDestructCountdown);
    }

    private void OnBracerUnequipped(Entity<YautjaBracerComponent> ent, ref GotUnequippedEvent args)
    {
        Decloak(ent);
        SyncCloakActionToggle(args.Equipee, false);
        CancelSelfDestruct(ent);
        RetractClaws(ent);
        RetractShield(ent);
    }

    private void OnBracerShutdown(Entity<YautjaBracerComponent> ent, ref ComponentShutdown args)
    {
        Decloak(ent);
        CancelSelfDestruct(ent);
        RetractClaws(ent);
        RetractShield(ent);
    }

    private void OnClawsShutdown(Entity<YautjaBracerClawsComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Bracer is not { } bracerUid
            || !TryComp(bracerUid, out YautjaBracerComponent? bracer)
            || bracer.ClawsEntity != ent.Owner)
        {
            return;
        }

        bracer.ClawsEntity = null;
        Dirty(bracerUid, bracer);
    }

    private void OnClawsDropped(Entity<YautjaBracerClawsComponent> ent, ref DroppedEvent args)
    {
        // Retract removes from hand then inserts into bracer — Dropped fires mid-transfer.
        if (_storingClaws.Contains(ent.Owner))
            return;

        if (ent.Comp.Bracer is { } bracerUid
            && _containers.TryGetContainer(bracerUid, YautjaBracerComponent.ClawsContainerId, out var container)
            && container.Contains(ent.Owner))
        {
            return;
        }

        // Unexpected floor drop: put back into bracer instead of leaving trash / blocking hands.
        if (ent.Comp.Bracer is { } owner
            && TryComp(owner, out YautjaBracerComponent? bracer)
            && !TerminatingOrDeleted(owner))
        {
            StoreClawsInBracer((owner, bracer), ent.Owner);
            return;
        }

        // Never predict-delete networked claw entities — only the server may cull orphans.
        if (_net.IsServer)
            QueueDel(ent.Owner);
    }

    private void OnShieldShutdown(Entity<YautjaBracerShieldComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Bracer is not { } bracerUid
            || !TryComp(bracerUid, out YautjaBracerComponent? bracer)
            || bracer.ShieldEntity != ent.Owner)
        {
            return;
        }

        bracer.ShieldEntity = null;
        Dirty(bracerUid, bracer);
    }

    private void OnShieldDropped(Entity<YautjaBracerShieldComponent> ent, ref DroppedEvent args)
    {
        if (_storingShield.Contains(ent.Owner))
            return;

        if (ent.Comp.Bracer is { } bracerUid
            && _containers.TryGetContainer(bracerUid, YautjaBracerComponent.ShieldContainerId, out var container)
            && container.Contains(ent.Owner))
        {
            return;
        }

        if (ent.Comp.Bracer is { } owner
            && TryComp(owner, out YautjaBracerComponent? bracer)
            && !TerminatingOrDeleted(owner))
        {
            StoreShieldInBracer((owner, bracer), ent.Owner);
            return;
        }

        if (_net.IsServer)
            QueueDel(ent.Owner);
    }

    private void Detonate(Entity<YautjaBracerComponent> ent)
    {
        ent.Comp.SelfDestructing = false;
        ent.Comp.SelfDestructPhase = YautjaBracerSelfDestructPhase.None;
        ent.Comp.SelfDestructAt = null;

        var user = ent.Comp.SelfDestructUser;
        ent.Comp.SelfDestructUser = null;
        ent.Comp.SelfDestructAction = null;
        Dirty(ent);

        if (user is not { } target || TerminatingOrDeleted(target))
            return;

        RetractClaws(ent);
        RetractShield(ent);

        var coords = Transform(target).Coordinates;
        var boom = Spawn(ent.Comp.SelfDestructExplosionPrototype, coords);
        _explosion.TriggerExplosive(boom);

        foreach (var item in _inventory.GetHandOrInventoryEntities(target))
            QueueDel(item);

        _body.GibBody(target, true);
    }

    private void CancelSelfDestruct(Entity<YautjaBracerComponent> ent)
    {
        if (!ent.Comp.SelfDestructing)
            return;

        ent.Comp.SelfDestructing = false;
        ent.Comp.SelfDestructPhase = YautjaBracerSelfDestructPhase.None;
        ent.Comp.SelfDestructAt = null;
        ent.Comp.SelfDestructUser = null;
        ent.Comp.SelfDestructAction = null;
        Dirty(ent);
    }

    private bool AreClawsExtended(Entity<YautjaBracerComponent> ent, EntityUid user) =>
        ent.Comp.ClawsEntity is { } uid
        && !TerminatingOrDeleted(uid)
        && _hands.IsHolding(user, uid);

    private bool TryExtendClaws(Entity<YautjaBracerComponent> ent, EntityUid user)
    {
        if (AreClawsExtended(ent, user))
            return true;

        // Не можна тримати кігті і щит одночасно.
        if (IsShieldExtended(ent, user))
        {
            RetractShield(ent);
            SyncInstantActionToggle<ToggleYautjaShieldEvent>(user, false);
        }

        if (!TryComp<HandsComponent>(user, out var hands))
            return false;

        // Do NOT drop held items onto the floor — require a free hand.
        if (_hands.CountFreeHands((user, hands)) <= 0)
        {
            _popup.PopupPredicted(Loc.GetString("yautja-bracer-claws-no-hands"), user, user);
            return false;
        }

        if (!TryGetOrSpawnClaws(ent, out var claws))
        {
            _popup.PopupPredicted(Loc.GetString("yautja-bracer-claws-no-hands"), user, user);
            return false;
        }

        RemComp<UnremoveableComponent>(claws);

        if (!_hands.TryPickupAnyHand(user, claws, checkActionBlocker: false))
        {
            StoreClawsInBracer(ent, claws);
            _popup.PopupPredicted(Loc.GetString("yautja-bracer-claws-no-hands"), user, user);
            return false;
        }

        EnsureComp<UnremoveableComponent>(claws);
        ent.Comp.ClawsEntity = claws;
        Dirty(ent);

        _audio.PlayPredicted(ent.Comp.ClawsExtendSound, user, user);
        return true;
    }

    private bool TryGetOrSpawnClaws(Entity<YautjaBracerComponent> ent, out EntityUid claws)
    {
        claws = default;

        var container = _containers.EnsureContainer<Container>(ent.Owner, YautjaBracerComponent.ClawsContainerId);

        if (ent.Comp.ClawsEntity is { } existing && !TerminatingOrDeleted(existing))
        {
            claws = existing;
            return true;
        }

        if (container.ContainedEntities.Count > 0)
        {
            claws = container.ContainedEntities[0];
            LinkClaws(ent, claws);
            return true;
        }

        if (!PredictedTrySpawnInContainer(ent.Comp.ClawsPrototype, ent.Owner, YautjaBracerComponent.ClawsContainerId, out var spawned))
            return false;

        claws = spawned.Value;
        LinkClaws(ent, claws);
        return true;
    }

    private void LinkClaws(Entity<YautjaBracerComponent> ent, EntityUid claws)
    {
        var clawsComp = EnsureComp<YautjaBracerClawsComponent>(claws);
        clawsComp.Bracer = ent.Owner;
        Dirty(claws, clawsComp);

        ent.Comp.ClawsEntity = claws;
        Dirty(ent);
    }

    private void RetractClaws(Entity<YautjaBracerComponent> ent)
    {
        if (ent.Comp.ClawsEntity is not { } claws || TerminatingOrDeleted(claws))
        {
            ent.Comp.ClawsEntity = null;
            Dirty(ent);
            return;
        }

        StoreClawsInBracer(ent, claws);
    }

    private void StoreClawsInBracer(Entity<YautjaBracerComponent> ent, EntityUid claws)
    {
        if (TerminatingOrDeleted(claws))
        {
            ent.Comp.ClawsEntity = null;
            Dirty(ent);
            return;
        }

        _storingClaws.Add(claws);
        try
        {
            RemComp<UnremoveableComponent>(claws);

            var container = _containers.EnsureContainer<Container>(ent.Owner, YautjaBracerComponent.ClawsContainerId);
            if (container.Contains(claws))
            {
                ent.Comp.ClawsEntity = claws;
                Dirty(ent);
                return;
            }

            if (!_containers.Insert(claws, container))
            {
                // Insert can briefly fail during prediction; never predict-delete networked claws.
                if (_net.IsServer)
                {
                    QueueDel(claws);
                    ent.Comp.ClawsEntity = null;
                    Dirty(ent);
                }
                return;
            }

            ent.Comp.ClawsEntity = claws;
            Dirty(ent);
        }
        finally
        {
            _storingClaws.Remove(claws);
        }
    }

    private bool IsShieldExtended(Entity<YautjaBracerComponent> ent, EntityUid user) =>
        ent.Comp.ShieldEntity is { } uid
        && !TerminatingOrDeleted(uid)
        && _hands.IsHolding(user, uid);

    private bool TryExtendShield(Entity<YautjaBracerComponent> ent, EntityUid user)
    {
        if (IsShieldExtended(ent, user))
            return true;

        // Не можна тримати кігті і щит одночасно.
        if (AreClawsExtended(ent, user))
        {
            RetractClaws(ent);
            SyncInstantActionToggle<ToggleYautjaClawsEvent>(user, false);
        }

        if (!TryComp<HandsComponent>(user, out var hands))
            return false;

        if (_hands.CountFreeHands((user, hands)) <= 0)
        {
            _popup.PopupPredicted(Loc.GetString("yautja-bracer-shield-no-hands"), user, user);
            return false;
        }

        if (!TryGetOrSpawnShield(ent, out var shield))
        {
            _popup.PopupPredicted(Loc.GetString("yautja-bracer-shield-no-hands"), user, user);
            return false;
        }

        RemComp<UnremoveableComponent>(shield);

        if (!_hands.TryPickupAnyHand(user, shield, checkActionBlocker: false))
        {
            StoreShieldInBracer(ent, shield);
            _popup.PopupPredicted(Loc.GetString("yautja-bracer-shield-no-hands"), user, user);
            return false;
        }

        EnsureComp<UnremoveableComponent>(shield);
        ent.Comp.ShieldEntity = shield;
        Dirty(ent);

        _audio.PlayPredicted(ent.Comp.ShieldExtendSound, user, user);
        return true;
    }

    private bool TryGetOrSpawnShield(Entity<YautjaBracerComponent> ent, out EntityUid shield)
    {
        shield = default;

        var container = _containers.EnsureContainer<Container>(ent.Owner, YautjaBracerComponent.ShieldContainerId);

        if (ent.Comp.ShieldEntity is { } existing && !TerminatingOrDeleted(existing))
        {
            shield = existing;
            return true;
        }

        if (container.ContainedEntities.Count > 0)
        {
            shield = container.ContainedEntities[0];
            LinkShield(ent, shield);
            return true;
        }

        if (!PredictedTrySpawnInContainer(ent.Comp.ShieldPrototype, ent.Owner, YautjaBracerComponent.ShieldContainerId, out var spawned))
            return false;

        shield = spawned.Value;
        LinkShield(ent, shield);
        return true;
    }

    private void LinkShield(Entity<YautjaBracerComponent> ent, EntityUid shield)
    {
        var shieldComp = EnsureComp<YautjaBracerShieldComponent>(shield);
        shieldComp.Bracer = ent.Owner;
        Dirty(shield, shieldComp);

        ent.Comp.ShieldEntity = shield;
        Dirty(ent);
    }

    private void RetractShield(Entity<YautjaBracerComponent> ent)
    {
        if (ent.Comp.ShieldEntity is not { } shield || TerminatingOrDeleted(shield))
        {
            ent.Comp.ShieldEntity = null;
            Dirty(ent);
            return;
        }

        StoreShieldInBracer(ent, shield);
    }

    private void StoreShieldInBracer(Entity<YautjaBracerComponent> ent, EntityUid shield)
    {
        if (TerminatingOrDeleted(shield))
        {
            ent.Comp.ShieldEntity = null;
            Dirty(ent);
            return;
        }

        _storingShield.Add(shield);
        try
        {
            RemComp<UnremoveableComponent>(shield);

            var container = _containers.EnsureContainer<Container>(ent.Owner, YautjaBracerComponent.ShieldContainerId);
            if (container.Contains(shield))
            {
                ent.Comp.ShieldEntity = shield;
                Dirty(ent);
                return;
            }

            if (!_containers.Insert(shield, container))
            {
                if (_net.IsServer)
                {
                    QueueDel(shield);
                    ent.Comp.ShieldEntity = null;
                    Dirty(ent);
                }
                return;
            }

            ent.Comp.ShieldEntity = shield;
            Dirty(ent);
        }
        finally
        {
            _storingShield.Remove(shield);
        }
    }
}
