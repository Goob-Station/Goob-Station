// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.HisGrace;
using Content.Goobstation.Shared.Overlays;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.HisGrace;

public sealed class HisGraceSystem : SharedHisGraceSystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly PopupSystem _popup = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MobStateSystem _state = null!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = null!;
    [Dependency] private readonly TransformSystem _transform = null!;
    [Dependency] private readonly AudioSystem _audio = null!;
    [Dependency] private readonly MindSystem _mind = null!;
    [Dependency] private readonly StunSystem _stun = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;
    [Dependency] private readonly ChatSystem _chat = null!;
    [Dependency] private readonly MobThresholdSystem _threshold = null!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HisGraceComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<HisGraceComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<HisGraceComponent, GotUnequippedHandEvent>(OnUnequipped);

        SubscribeLocalEvent<HisGraceComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<HisGraceComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<HisGraceComponent, HisGraceStateChangedEvent>(OnStateChanged);
        SubscribeLocalEvent<HisGraceComponent, HisGraceEntityConsumedEvent>(OnEntityConsumed);

        SubscribeLocalEvent<HisGraceComponent, HisGraceHungerChangedEvent>(UpdateHungerState);

        SubscribeLocalEvent<HisGraceUserComponent, MapInitEvent>(OnUserInit);
        SubscribeLocalEvent<HisGraceUserComponent, ComponentRemove>(OnUserRemoved);
        SubscribeLocalEvent<HisGraceUserComponent, RefreshMovementSpeedModifiersEvent>(OnModifierRefresh);
    }

    private void OnInit(Entity<HisGraceComponent> hisGrace, ref MapInitEvent args)
    {
        hisGrace.Comp.Stomach = _containerSystem.EnsureContainer<Container>(hisGrace, "stomach");

        if (!TryComp<MeleeWeaponComponent>(hisGrace, out var melee))
            return;

        hisGrace.Comp.BaseDamage = melee.Damage;
        hisGrace.Comp.OrderedStates = hisGrace.Comp.StateThresholds.OrderByDescending(t => t.Value.Threshold).ToList();

        Dirty(hisGrace, melee);
    }

    private void OnUserInit(Entity<HisGraceUserComponent> user, ref MapInitEvent args)
    {
        if (!TryComp<StaminaComponent>(user, out var stamina))
            return;

        user.Comp.BaseStamCritThreshold = stamina.CritThreshold;
        stamina.CritThreshold = user.Comp.HoldingStamCritThreshold;

        Dirty(user, stamina);
    }

    private void OnUserRemoved(Entity<HisGraceUserComponent> user, ref ComponentRemove args)
    {
        if (TryComp<StaminaComponent>(user, out var stamina))
            stamina.CritThreshold = user.Comp.BaseStamCritThreshold;
    }

    private void OnEquipped(Entity<HisGraceComponent> hisGrace, ref GotEquippedHandEvent args)
    {
        hisGrace.Comp.IsHeld = true;
        hisGrace.Comp.Holder = args.User;
    }

    private void OnUnequipped(Entity<HisGraceComponent> hisGrace, ref GotUnequippedHandEvent args)
    {
        hisGrace.Comp.IsHeld = false;
        hisGrace.Comp.Holder = null;
    }

    private void OnMeleeHit(Entity<HisGraceComponent> hisGrace, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
            TryDevour(hisGrace, hitEntity);
    }

    private void OnModifierRefresh(Entity<HisGraceUserComponent> hisGrace, ref RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(hisGrace.Comp.SpeedMultiplier);

    private void UpdateSpeedMultiplier(HisGraceComponent hisGrace, float bonus)
    {
        if (hisGrace.User is not { } user
        || !TryComp<HisGraceUserComponent>(user, out var userComp))
            return;

        userComp.SpeedMultiplier = userComp.BaseSpeedMultiplier + bonus;
    }


    private void OnUse(Entity<HisGraceComponent> hisGrace, ref UseInHandEvent args)
    {
        if (hisGrace.Comp.CurrentState != HisGraceState.Dormant)
            return;

        hisGrace.Comp.User = args.User;
        EnsureComp<HisGraceUserComponent>(args.User).HisGrace = hisGrace;

        var popUp = Loc.GetString("hisgrace-use-start");
        _popup.PopupEntity(popUp, args.User, args.User, PopupType.MediumCaution);

        // Log activation with actor and tool format
        _adminLog.Add(LogType.AdminMessage, LogImpact.Extreme,
            $"HIS GRACE ACTIVATED: {ToPrettyString(args.User):actor} activated {ToPrettyString(hisGrace):tool} at {Transform(hisGrace).Coordinates}");

        ChangeState(hisGrace, HisGraceState.Peckish);
        SetUnremovable(hisGrace, true);
    }

    private void OnEntityConsumed(Entity<HisGraceComponent> hisGrace, ref HisGraceEntityConsumedEvent args)
    {
        hisGrace.Comp.EntitiesAbsorbed++;

        if (hisGrace.Comp.EntitiesAbsorbed >= hisGrace.Comp.AscensionThreshold)
        {
            var user = hisGrace.Comp.User ?? hisGrace.Owner;
            _adminLog.Add(LogType.AdminMessage, LogImpact.Extreme,
                $"HIS GRACE ASCENSION: {ToPrettyString(user):actor} reached ascension with {ToPrettyString(hisGrace):tool}. " +
                $"Total entities consumed: {hisGrace.Comp.EntitiesAbsorbed}");

            ChangeState(hisGrace, HisGraceState.Ascended);
        }

        if (!TryComp<MeleeWeaponComponent>(hisGrace, out var melee))
            return;

        // 5 blunt per entity consumed
        hisGrace.Comp.CurrentDamageIncrease.DamageDict["Blunt"] = hisGrace.Comp.EntitiesAbsorbed * 5;
        melee.Damage = hisGrace.Comp.BaseDamage + hisGrace.Comp.CurrentDamageIncrease;

        Dirty(hisGrace, melee);
    }

    private void OnStateChanged(Entity<HisGraceComponent> hisGrace, ref HisGraceStateChangedEvent args)
    {
        if (hisGrace.Comp.User is not { } user)
            return;

        _speedModifier.RefreshMovementSpeedModifiers(user);

        if (HandleAscendedState(hisGrace, args))
            return;

        ShowHungerChangePopup(hisGrace, args);
        HandleHungerState(hisGrace, user, args.NewState);
    }

    private bool HandleAscendedState(Entity<HisGraceComponent> hisGrace, HisGraceStateChangedEvent args)
    {
        if (args.NewState != HisGraceState.Ascended
            || args.OldState == HisGraceState.Ascended)
            return false;

        SetUnremovable(hisGrace, true);

        DoAscension(hisGrace);
        DoAscensionVisuals(hisGrace, "ascended");
        return true;
    }

    private void ShowHungerChangePopup(EntityUid uid, HisGraceStateChangedEvent args)
    {
        // Prevents pop-up clutter.
        if (args.OldState == HisGraceState.Dormant)
            return;

        // if the new state is bigger than the old state, increase popup
        // else, decrease
        // we dont count for ascended since too many popups will clutter it.
        var (messageKey, popupType) = args.NewState > args.OldState
            && args.NewState != HisGraceState.Ascended
            ? ("hisgrace-hunger-increased", PopupType.MediumCaution)
            : ("hisgrace-hunger-decreased", PopupType.Medium);

        _popup.PopupEntity(Loc.GetString(messageKey), uid, popupType);
    }

    private void HandleHungerState(Entity<HisGraceComponent> hisGrace, EntityUid user, HisGraceState newState)
    {
        switch (newState)
        {
            case HisGraceState.Dormant:
                HandleDormantState(hisGrace);
                break;
            case HisGraceState.Peckish:
                HandlePeckishState(hisGrace);
                break;
            case HisGraceState.Ravenous:
            case HisGraceState.Starving:
                HandleRavenousState(hisGrace);
                break;
            case HisGraceState.Death:
                HandleDeathState(hisGrace, user);
                break;
        }
    }
    private void HandleDormantState(Entity<HisGraceComponent> hisGrace)
    {
        SetUnremovable(hisGrace, false);
        _popup.PopupEntity(Loc.GetString("hisgrace-hunger-sated"), hisGrace, PopupType.MediumCaution);
        hisGrace.Comp.User = null;
        ReleaseContainedEntities(hisGrace);
    }

    private void HandlePeckishState(Entity<HisGraceComponent> hisGrace) =>
        UpdateSpeedMultiplier(hisGrace, hisGrace.Comp.SpeedAddition);

    private void HandleRavenousState(Entity<HisGraceComponent> hisGrace) =>
        UpdateSpeedMultiplier(hisGrace, hisGrace.Comp.SpeedAddition * hisGrace.Comp.SpeedIncrementMultiplier);

    private void HandleDeathState(Entity<HisGraceComponent> hisGrace, EntityUid user)
    {
        _damageable.TryChangeDamage(user,
            hisGrace.Comp.DamageOnFail,
            targetPart: TargetBodyPart.Chest,
            origin: hisGrace,
            ignoreResistances: true);

        // Log the death state activation
        _adminLog.Add(LogType.AdminMessage, LogImpact.High,
            $"HIS GRACE DEATH: {ToPrettyString(user):actor} was killed by {ToPrettyString(hisGrace):tool} due to hunger");

        var popup = Loc.GetString("hisgrace-death", ("target", Name(user)));
        _popup.PopupEntity(popup, user, user, PopupType.LargeCaution);

        ChangeState(hisGrace, HisGraceState.Dormant);
        RemComp<HisGraceUserComponent>(user);
    }


    #region Update Loop

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HisGraceComponent, MeleeWeaponComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var hisGrace, out var melee, out var xform))
            UpdateHisGrace((uid, hisGrace), melee, xform);

    }

    private void UpdateHisGrace(Entity<HisGraceComponent> hisGrace, MeleeWeaponComponent melee, TransformComponent xform)
    {
        if (hisGrace.Comp.CurrentState is HisGraceState.Dormant or HisGraceState.Death or HisGraceState.Ascended)
            return;

        if (TerminatingOrDeleted(hisGrace)
            || hisGrace.Comp.User is not { } user)
        {
            hisGrace.Comp.CurrentState = HisGraceState.Dormant;
            return;
        }

        if (_timing.CurTime < hisGrace.Comp.NextTick)
            return;

        HandleUserDistance(hisGrace, user);
        HandleGroundAttacks(hisGrace, melee, xform);
        ProcessHungerTick(hisGrace, user);

        // do healing
        _damageable.TryChangeDamage(user,
            hisGrace.Comp.Healing,
            true,
            false,
            targetPart: TargetBodyPart.All,
            splitDamage: SplitDamageBehavior.SplitEnsureAll,
            ignoreBlockers: true);

        // revive if dead
        if (_state.IsDead(user)
            && _threshold.TryGetDeadThreshold(user, out var deadThreshold)
            && TryComp<DamageableComponent>(user, out var damageable)
            && _threshold.CheckVitalDamage(user, damageable) < deadThreshold
            && hisGrace.Comp.IsHeld)
        {
            _state.ChangeMobState(user, MobState.Critical);
        }

        hisGrace.Comp.NextTick = _timing.CurTime + hisGrace.Comp.TickDelay;
    }

    private void HandleUserDistance(Entity<HisGraceComponent> hisGrace, EntityUid user)
    {
        if (_lookup.GetEntitiesInRange(hisGrace, 1f).Contains(user))
            return;

        var popUp = Loc.GetString("hisgrace-too-far");
        _popup.PopupEntity(popUp, user, user, PopupType.LargeCaution);

        _damageable.TryChangeDamage(user, hisGrace.Comp.BaseDamage, targetPart: TargetBodyPart.Chest, ignoreResistances: true);
    }

    private void HandleGroundAttacks(Entity<HisGraceComponent> hisGrace, MeleeWeaponComponent melee, TransformComponent xform)
    {
        if (hisGrace.Comp.IsHeld
            && hisGrace.Comp.Holder == hisGrace.Comp.User)
            return;

        var nearbyEnts = _lookup.GetEntitiesInRange(hisGrace, 1f);

        // dont attack if the entity is the user, and dont if the entity is in a container (e.g, already devoured)
        foreach (var entity in nearbyEnts.Where(entity => HasComp<MobStateComponent>(entity)
            && entity != hisGrace.Comp.User
            && !_containerSystem.IsEntityOrParentInContainer(entity)))
        {
            // Log ground attack
            _adminLog.Add(LogType.AdminMessage, LogImpact.Medium,
                $"HIS GRACE GROUND ATTACK: {ToPrettyString(hisGrace):tool} attacked {ToPrettyString(entity):target} at {Transform(hisGrace).Coordinates}");

            // get co-ordinates for animation
            var coordinates = _transform.GetMapCoordinates(hisGrace);
            var angle = _transform.GetRelativePosition(xform, entity, GetEntityQuery<TransformComponent>()).ToAngle();

            // do damage and animation
            _damageable.TryChangeDamage(entity, melee.Damage, targetPart: TargetBodyPart.Chest, origin: hisGrace);
            _melee.DoLunge(hisGrace, hisGrace, angle, coordinates.Position, null, angle, false, false);

            _audio.PlayPvs(melee.HitSound, hisGrace);
            _popup.PopupEntity(Loc.GetString("hisgrace-attack-popup", ("target", Name(entity))), hisGrace, PopupType.LargeCaution);

            TryDevour(hisGrace, entity);

            break;
        }
    }

    private void UpdateHungerState(Entity<HisGraceComponent> hisGrace, ref HisGraceHungerChangedEvent args)
    {
        foreach (var stateThreshold in hisGrace.Comp.OrderedStates)
        {
            if (hisGrace.Comp.Hunger < stateThreshold.Value.Threshold)
                continue;

            if (stateThreshold.Key == hisGrace.Comp.CurrentState)
                return;

            hisGrace.Comp.HungerIncrement = stateThreshold.Value.Increment;
            ChangeState(hisGrace, stateThreshold.Key);

            return;
        }
    }

    // increases hunger and heals user every tick
    private void ProcessHungerTick(Entity<HisGraceComponent> hisGrace, EntityUid user)
    {
        hisGrace.Comp.Hunger += hisGrace.Comp.HungerIncrement;

        var ev = new HisGraceHungerChangedEvent();
        RaiseLocalEvent(hisGrace, ref ev);
    }

    #endregion

    #region Helpers

    private void DoAscension(Entity<HisGraceComponent> hisGrace)
    {
        if (hisGrace.Comp.User is not { } user
            || TerminatingOrDeleted(user))
            return;

        var ascensionPopup = Loc.GetString("hisgrace-ascension");
        _popup.PopupEntity(ascensionPopup, user, user, PopupType.Large);

        // Log ascension with all relevant details
        _adminLog.Add(LogType.AdminMessage, LogImpact.Extreme,
            $"HIS GRACE ASCENSION ACHIEVED: {ToPrettyString(user):actor} achieved ascension with {ToPrettyString(hisGrace):tool} at {Transform(hisGrace).Coordinates}. " +
            $"Total entities consumed: {hisGrace.Comp.EntitiesAbsorbed}");

        // Apply ascension effects
        EnsureComp<ThermalVisionComponent>(user);
        EnsureComp<PressureImmunityComponent>(user);
        EnsureComp<BreathingImmunityComponent>(user);

        UpdateSpeedMultiplier(hisGrace, hisGrace.Comp.SpeedAddition * hisGrace.Comp.SpeedIncrementMultiplier * hisGrace.Comp.SpeedIncrementMultiplier);

        // le funny ascension
        _chat.DispatchGlobalAnnouncement(Loc.GetString("hisgrace-ascension-announcement"), Name(user), true, hisGrace.Comp.AscendSound, Color.PaleGoldenrod);
    }

    private void ChangeState(Entity<HisGraceComponent> hisGrace, HisGraceState newState)
    {
        // Store the old state before changing it
        var oldState = hisGrace.Comp.CurrentState;
        hisGrace.Comp.CurrentState = newState;

        var ev = new HisGraceStateChangedEvent(newState, oldState);
        RaiseLocalEvent(hisGrace, ref ev);
    }
    private bool TryDevour(Entity<HisGraceComponent> hisGrace, EntityUid target)
    {
        if (!_state.IsIncapacitated(target))
        {
            _adminLog.Add(LogType.AdminMessage, LogImpact.Medium,
                $"HIS GRACE DEVOUR FAILED: {ToPrettyString(hisGrace.Comp.User ?? hisGrace.Owner):actor} failed to devour {ToPrettyString(target):target} with {ToPrettyString(hisGrace):tool} - Target not incapacitated");
            return false;
        }

        if (!_containerSystem.Insert(target, hisGrace.Comp.Stomach))
        {
            _adminLog.Add(LogType.AdminMessage, LogImpact.Medium,
                $"HIS GRACE DEVOUR FAILED: {ToPrettyString(hisGrace.Comp.User ?? hisGrace.Owner):actor} failed to devour {ToPrettyString(target):target} with {ToPrettyString(hisGrace):tool} - Container insertion failed");
            return false;
        }

        // Log successful devour attempt
        _adminLog.Add(LogType.AdminMessage, LogImpact.High,
            $"HIS GRACE DEVOUR: {ToPrettyString(hisGrace.Comp.User ?? hisGrace.Owner):actor} devoured {ToPrettyString(target):target} with {ToPrettyString(hisGrace):tool}");

        // Hunger gained from eating an entity is 20% of their crit state.
        hisGrace.Comp.Hunger -= GetHungerValue(target, hisGrace).Value;

        var devourPopup = Loc.GetString("hisgrace-devour", ("target", Name(target)));
        _audio.PlayPvs(hisGrace.Comp.SoundDevour, target);
        _popup.PopupEntity(devourPopup, target, PopupType.LargeCaution);

        // don't apply bonuses for entities consumed that don't have minds or aren't human (no farming sentient mice)
        if (_mind.TryGetMind(target, out _, out _)
            && HasComp<HumanoidAppearanceComponent>(target))
        {
            var ev = new HisGraceEntityConsumedEvent();
            RaiseLocalEvent(hisGrace, ref ev);
        }
        else
        {
            _adminLog.Add(LogType.AdminMessage, LogImpact.Low,
                $"HIS GRACE NON-HUMANOID: {ToPrettyString(hisGrace.Comp.User ?? hisGrace.Owner):actor} consumed non-humanoid {ToPrettyString(target):target} with {ToPrettyString(hisGrace):tool}");
        }

        return true;
    }

    private FixedPoint2 GetHungerValue(EntityUid target, HisGraceComponent comp)
    {
        if (!_threshold.TryGetThresholdForState(target, MobState.Critical, out var criticalThreshold))
            return comp.HungerOnDevourDefault;

        // hunger value is equal to the mutiplier times the crit threshold.
        // this is 100 for humans, so the hunger returned is 20.
        return (FixedPoint2)(comp.HungerOnDevourMultiplier * criticalThreshold); // solstice try not to cast challenge (impossible)
    }

    private void SetUnremovable(Entity<HisGraceComponent> hisGrace, bool enabled)
    {
        if (enabled)
        {
            // hisGrace.Comp.PreventDrop = true; - Disabled until someone fixes it :P
            EnsureComp<JitteringComponent>(hisGrace);
        }
        else
        {
            // hisGrace.Comp.PreventDrop = false;
            RemComp<JitteringComponent>(hisGrace);
        }
    }

    private void ReleaseContainedEntities(HisGraceComponent hisGrace)
    {
        var released = _containerSystem.EmptyContainer(hisGrace.Stomach, true);

        foreach (var ent in released)
            _stun.TryParalyze(ent, TimeSpan.FromSeconds(8), true);
    }

    #endregion

}
