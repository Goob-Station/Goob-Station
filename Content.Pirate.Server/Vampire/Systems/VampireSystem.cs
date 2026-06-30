using Content.Server.Actions;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Pirate.Server.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Goobstation.Shared.Religion;
using Content.Pirate.Shared.Vampire;
using Content.Pirate.Shared.Vampire.Components;
using Content.Pirate.Server.Traits.Vampirism;
using Content.Pirate.Server.Traits.Vampirism.Components;
using Content.Pirate.Shared.Vampire.Components.Classes;
using Content.Pirate.Shared.Vampire.Prototypes;
using Content.Shared.Body.Components;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Alert;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Server.Body.Components;
using Content.Server.GameTicking;
using Content.Shared.Nutrition.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Prayer;
using Content.Goobstation.Shared.Overlays;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Prometheus;

namespace Content.Pirate.Server.Vampire.Systems;

public sealed partial class VampireSystem : EntitySystem
{
    # region Starlight data collection
    private static readonly Counter _vampireClasses = Metrics.CreateCounter(
        "Vampire_Classes",
        "Numbers of vampire classes chosen by players",
        ["class"]
    );
    #endregion
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly ILogManager _log = default!;
    //[Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly VampirismSystem _vampirism = default!;

    private ISawmill? _sawmill;
    private static readonly ProtoId<DamageGroupPrototype> _bruteGroupId = "Brute";
    private static readonly ProtoId<DamageGroupPrototype> _burnGroupId = "Burn";
    private static readonly ProtoId<DamageGroupPrototype> _geneticGroupId = "Genetic";
    private static readonly ProtoId<DamageTypePrototype> _cellularTypeId = "Cellular";
    private static readonly ProtoId<DamageTypePrototype> _poisonTypeId = "Poison";
    private static readonly ProtoId<DamageTypePrototype> _oxyLossTypeId = "Asphyxiation";
    private static readonly ProtoId<DamageTypePrototype> _heatTypeId = "Heat";
    private static readonly ProtoId<DamageTypePrototype> _pierceTypeId = "Piercing";
    private static readonly SoundSpecifier _spaceBurnSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _log.GetSawmill("Vampire");

        SubscribeLocalEvent<VampireComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VampireComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<VampireComponent, VampireProgressionChangedEvent>(OnProgressionChanged);
        SubscribeLocalEvent<ActionsComponent, ComponentStartup>(OnActionsComponentStartup);
        SubscribeLocalEvent<VampireComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<VampireActionUseAttemptEvent>(OnVampireActionUseAttempt);
        InitializeAbilities();
        InitializeObjectives();
    }

    private void OnVampireActionUseAttempt(ref VampireActionUseAttemptEvent args)
    {
        args.Allowed = CheckAndConsumeGrantedVampireAction(args.User, args.ActionEntity, args.BloodCost);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (!TryComp(ev.Entity, out VampireComponent? vampire))
            return;

        SyncVampireActions(ev.Entity, vampire);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<VampireComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now <= comp.NextUpdate)
                continue;

            var elapsed = comp.LastUpdate == TimeSpan.Zero
                ? (float) comp.UpdateDelay.TotalSeconds
                : MathF.Max(0f, (float) (now - comp.LastUpdate).TotalSeconds);

            comp.LastUpdate = now;
            comp.NextUpdate = now + comp.UpdateDelay;

            ProcessBloodDecay((uid, comp), elapsed);
            HandleHolyWater(uid, comp);
            HandleHolyPlace(uid, comp);
        }

        var sunlightQuery = EntityQueryEnumerator<VampireSunlightComponent, TransformComponent>();
        while (sunlightQuery.MoveNext(out var uid, out var sunlight, out var xform))
        {
            if (!TryComp<VampireComponent>(uid, out var vampire))
                continue;

            HandleSpaceExposure(uid, vampire, sunlight, xform);
        }

        ProcessActiveVampireEffects(now);
    }

    private void HandleSpaceExposure(EntityUid uid, VampireComponent vampire, VampireSunlightComponent sunlight, TransformComponent xform)
    {
        if (_container.IsEntityInContainer(uid))
        {
            ResetSpaceExposure(sunlight);
            return;
        }

        if (!IsInSpace(xform))
        {
            ResetSpaceExposure(sunlight);
            return;
        }

        if (TryComp<MobStateComponent>(uid, out var mobState) &&
            mobState.CurrentState == MobState.Dead)
        {
            ResetSpaceExposure(sunlight);
            return;
        }

        var now = _timing.CurTime;

        var damageInterval = sunlight.DamageInterval;
        if (damageInterval < TimeSpan.FromSeconds(0.1f))
            damageInterval = TimeSpan.FromSeconds(0.1f);

        if (sunlight.TimeEnteredSpace == null)
        {
            sunlight.TimeEnteredSpace = now;
            sunlight.NextWarningPopup = now + sunlight.GracePeriod;
            sunlight.NextDamageTime = now + sunlight.GracePeriod + damageInterval;
        }

        var timeInSpace = now - sunlight.TimeEnteredSpace.Value;

        if (timeInSpace < sunlight.GracePeriod)
            return;

        if (_timing.CurTime >= sunlight.NextWarningPopup)
        {
            _popup.PopupEntity(Loc.GetString(sunlight.WarningPopup), uid, uid, PopupType.LargeCaution);
            sunlight.NextWarningPopup = _timing.CurTime + sunlight.WarningPopupCooldown;
        }

        var nextDamage = sunlight.NextDamageTime;
        if (nextDamage == null)
        {
            sunlight.NextDamageTime = now + damageInterval;
            nextDamage = sunlight.NextDamageTime;
        }
        if (_timing.CurTime < nextDamage)
            return;

        if (!ProcessSpaceExposureTick(uid, vampire, sunlight))
            return;

        sunlight.NextDamageTime = now + damageInterval;
    }

    private void ResetSpaceExposure(VampireSunlightComponent sunlight)
    {
        sunlight.TimeEnteredSpace = null;
        sunlight.NextDamageTime = null;
        sunlight.NextWarningPopup = TimeSpan.Zero;
    }

    private bool ProcessSpaceExposureTick(EntityUid uid, VampireComponent vampire, VampireSunlightComponent sunlight)
    {
        var hadBlood = vampire.DrunkBlood > 0;

        if (hadBlood)
        {
            DrainBlood(uid, vampire, sunlight);

        }
        else
        {
            if (!ApplyGeneticSpaceDamage(uid, sunlight))
                return false;
        }

        var damageable = CompOrNull<DamageableComponent>(uid);
        var thresholds = CompOrNull<MobThresholdsComponent>(uid);
        var healthy = IsAboveHalfHealth(uid, damageable, thresholds);

        var chance = hadBlood ? sunlight.BloodEffectChance : sunlight.BloodlessEffectChance;
        TryApplySpaceDamage(uid, healthy, chance, sunlight);

        return true;
    }

    private void DrainBlood(EntityUid uid, VampireComponent vampire, VampireSunlightComponent sunlight)
    {
        var drain = Math.Min(sunlight.BloodDrainPerInterval, vampire.DrunkBlood);
        if (drain <= 0)
            return;

        TrySpendBlood(uid, vampire, drain, showPopup: false);
    }

    private bool ApplyGeneticSpaceDamage(EntityUid uid, VampireSunlightComponent sunlight)
    {
        if (!_proto.TryIndex<DamageGroupPrototype>(_geneticGroupId, out var damageGroup))
            return true;

        var spec = new DamageSpecifier(damageGroup, sunlight.GeneticDamagePerInterval);
        _damageableSystem.TryChangeDamage(uid, spec, true);

        if (!TryComp(uid, out DamageableComponent? damageable) ||
            damageable == null ||
            !damageable.DamagePerGroup.TryGetValue(_geneticGroupId, out var geneticDamage))
        {
            return true;
        }

        _audio.PlayPvs(_spaceBurnSound, uid);

        if (geneticDamage < sunlight.GeneticDustThreshold)
            return true;

        DustEntity(uid);
        return false;
    }

    private void TryApplySpaceDamage(EntityUid uid, bool isHealthy, float chance, VampireSunlightComponent sunlight)
    {
        if (!_rand.Prob(Math.Clamp(chance, 0f, 1f)))
            return;

        if (isHealthy)
        {
            if (_proto.TryIndex(_heatTypeId, out var heat))
            {
                var spec = new DamageSpecifier(heat, sunlight.BurnDamage);
                _damageableSystem.TryChangeDamage(uid, spec, true);
            }
        }
        else
        {
            _flammable.AdjustFireStacks(uid, sunlight.FireStacksOnIgnite, ignite: true);
        }

        _audio.PlayPvs(_spaceBurnSound, uid);
    }

    private bool IsAboveHalfHealth(EntityUid uid, DamageableComponent? damageable, MobThresholdsComponent? thresholds)
    {
        damageable ??= CompOrNull<DamageableComponent>(uid);
        thresholds ??= CompOrNull<MobThresholdsComponent>(uid);

        if (damageable == null)
            return true;

        if (!_mobThreshold.TryGetDeadThreshold(uid, out var deadThreshold, thresholds) ||
            deadThreshold == null ||
            deadThreshold.Value == FixedPoint2.Zero)
        {
            return true;
        }

        var max = deadThreshold.Value.Float();
        if (max <= 0f)
            return true;

        var current = damageable.TotalDamage.Float();
        return current <= max * 0.5f;
    }

    private void DustEntity(EntityUid uid)
    {
        var coords = Transform(uid).Coordinates;
        _popup.PopupEntity(Loc.GetString("admin-smite-turned-ash-other", ("name", uid)), uid, PopupType.LargeCaution);
        QueueDel(uid);
        Spawn("Ash", coords);
    }

    private bool IsInSpace(TransformComponent xform)
    {
        if (xform.GridUid == null)
            return true;

        if (!TryComp(xform.GridUid.Value, out MapGridComponent? grid))
            return true;

        if (!_map.TryGetTileRef(xform.GridUid.Value, grid, xform.Coordinates, out var tileRef))
            return true;

        return _turf.IsSpace(tileRef);
    }

    private bool ProcessBloodDecay(Entity<VampireComponent> ent, float elapsed)
    {
        var (uid, comp) = ent;
        var before = comp.BloodFullness;
        var wasStarving = before <= 0f;
        var changed = false;

        if (before > 0f && _gameTicker.RunLevel < GameRunLevel.PostRound) // No hunger EOR
        {
            comp.StarvationDrunkBloodDrainAccumulator = 0f;
            comp.BloodFullness = MathF.Max(0f, before - (comp.FullnessDecayPerSecond * elapsed));
            changed = !MathF.Abs(comp.BloodFullness - before).Equals(0f);

            if (changed)
            {
                Dirty(uid, comp);
                UpdateVampireFedAlert(uid, comp);
            }
        }

        var isStarving = comp.BloodFullness <= 0f;
        if (wasStarving != isStarving)
            _movementSpeed.RefreshMovementSpeedModifiers(uid);

        // When blood fullness is empty, burn stored blood
        if (comp.BloodFullness <= 0f && comp.StarvationDrunkBloodDrainPerSecond > 0 && comp.DrunkBlood > 0)
        {
            comp.StarvationDrunkBloodDrainAccumulator += comp.StarvationDrunkBloodDrainPerSecond * elapsed;
            var drained = Math.Min(comp.DrunkBlood, (int) comp.StarvationDrunkBloodDrainAccumulator);
            if (drained <= 0)
                return changed;

            comp.StarvationDrunkBloodDrainAccumulator -= drained;
            TrySpendBlood(uid, comp, drained, showPopup: false);
            changed = true;
        }

        return changed;
    }

    private void RefreshAllActions(EntityUid uid, VampireComponent comp)
    {
        comp.LastRefreshedBloodLevel = comp.TotalBlood;
        foreach (var (_, actionEntity) in comp.ActionEntities)
            TryRefreshVampireAction(uid, actionEntity);
    }

    private void HandleClassSelection(EntityUid uid, VampireComponent comp)
    {
        if (HasChosenClass(uid))
            return;

        var classSelectAction = comp.ClassSelectActionId;

        if (comp.TotalBlood >= comp.ClassSelectThreshold && !comp.ActionEntities.ContainsKey(classSelectAction))
        {
            EntityUid? actionEntity = null;
            _actions.AddAction(uid, ref actionEntity, classSelectAction, uid);
            if (actionEntity != null)
            {
                comp.ActionEntities[classSelectAction] = actionEntity.Value;
                Dirty(uid, comp);
            }
        }

        if (comp.ActionEntities.TryGetValue(classSelectAction, out var classSelectActionEntity))
            TryRefreshVampireAction(uid, classSelectActionEntity);
    }

    private void OnProgressionChanged(EntityUid uid, VampireComponent comp, ref VampireProgressionChangedEvent args) =>
        SyncVampireActions(uid, comp);

    private void OnActionsComponentStartup(EntityUid uid, ActionsComponent _, ComponentStartup args)
    {
        if (!TryComp(uid, out VampireComponent? vampire))
            return;
        SyncVampireActions(uid, vampire);
    }

    private void SyncVampireActions(EntityUid uid, VampireComponent comp)
    {
        CleanMissingActions(comp);
        HandleClassSelection(uid, comp);
        EnsureRejuvenateUpgrade(uid, comp);
        TryGrantClassAbilities(uid, comp);
        RefreshAllActions(uid, comp);
    }

    private void CleanMissingActions(VampireComponent comp)
    {
        if (comp.ActionEntities.Count == 0)
            return;

        var snapshot = new List<KeyValuePair<EntProtoId, EntityUid>>(comp.ActionEntities);
        foreach (var pair in snapshot)
        {
            if (Exists(pair.Value))
                continue;

            comp.ActionEntities.Remove(pair.Key);
        }
    }

    private void OnStartup(EntityUid uid, VampireComponent comp, ComponentStartup args)
    {
       comp.HadWeakToHoly = TryComp<WeakToHolyComponent>(uid, out var weakToHoly);
        comp.HadAlwaysTakeHoly = weakToHoly?.AlwaysTakeHoly ?? false;

        weakToHoly ??= EnsureComp<WeakToHolyComponent>(uid);
        weakToHoly.AlwaysTakeHoly = true;
        Dirty(uid, weakToHoly);

        EnsureComp<VampireSunlightComponent>(uid);
        foreach (var actionId in comp.BaseVampireActions)
        {
            EntityUid? action = null;

            _actions.AddAction(uid, ref action, actionId, uid);

            if (action != null)
                comp.ActionEntities[actionId] = action.Value;
        }
        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);
        RemComp<RespiratorComponent>(uid);

        _alerts.ClearAlertCategory(uid, "Hunger");

        UpdateVampireAlert(uid);
        UpdateVampireFedAlert(uid, comp);

        SyncVampireActions(uid, comp);
        EnsureComp<BloodSuckerComponent>(uid);

        _vampirism.SetMetabolizerTypes(uid, comp.MetabolizerPrototypes);

        _movementSpeed.RefreshMovementSpeedModifiers(uid);

    }

    private void OnShutdown(EntityUid uid, VampireComponent comp, ComponentShutdown args)
    {
        if (TryComp<WeakToHolyComponent>(uid, out var weakToHoly))
        {
            if (comp.HadWeakToHoly)
            {
                weakToHoly.AlwaysTakeHoly = comp.HadAlwaysTakeHoly;
                Dirty(uid, weakToHoly);
            }
            else
            {
                RemComp<WeakToHolyComponent>(uid);
            }
        }

        RemComp<NightVisionComponent>(uid);
        if (TryComp<VampireDrainBeamComponent>(uid, out var drainBeamComp))
        {
            foreach (var connection in drainBeamComp.ActiveBeams.Values)
            {
                var removeEvent = new VampireDrainBeamEvent(GetNetEntity(connection.Source), GetNetEntity(connection.Target), false, drainBeamComp.VisualPrototype);
                RaiseNetworkEvent(removeEvent);
            }
            drainBeamComp.ActiveBeams.Clear();
        }

        if (TryComp<UmbraeComponent>(uid, out var umbrae))
        {
            umbrae.ShadowBoxingActive = false;
            umbrae.ShadowBoxingTarget = null;
            umbrae.ShadowBoxingEndTime = null;
            umbrae.ShadowBoxingLoopRunning = false;

            if (umbrae.EternalDarknessAuraEntity is { } aura && Exists(aura))
                QueueDel(aura);

            if (umbrae.SpawnedShadowAnchorBeacon is { } beacon && Exists(beacon))
                QueueDel(beacon);

            foreach (var snare in umbrae.PlacedSnares.ToArray())
            {
                if (Exists(snare))
                    QueueDel(snare);
            }

            umbrae.PlacedSnares.Clear();
            umbrae.EternalDarknessAuraEntity = null;
            umbrae.SpawnedShadowAnchorBeacon = null;
            umbrae.ShadowAnchorAutoReturnTime = null;
        }

        if (_playerShadowSnares.TryGetValue(uid, out var snares))
        {
            foreach (var trap in snares.ToArray())
            {
                if (Exists(trap))
                    QueueDel(trap);
            }
            _playerShadowSnares.Remove(uid);
        }
    }

    partial void UpdateVampireAlert(EntityUid uid);
    partial void UpdateVampireFedAlert(EntityUid uid, VampireComponent? comp);

    private void TryRefreshVampireAction(EntityUid owner, EntityUid? actionEntity)
    {
        if (actionEntity == null
            || _actions.GetAction(actionEntity) is not { } action
            || !TryComp<VampireComponent>(owner, out var vamp))
            return;

        if (!TryComp<VampireActionComponent>(actionEntity.Value, out var vac))
        {
            _actions.SetEnabled(action.AsNullable(), true);
            return;
        }

        // Pirate: keep full-power actions clickable so the server can explain why they fail.
        var enabled = vamp.TotalBlood >= vac.BloodToUnlock
             && (vac.RequiredClass == null || ValidateVampireClass(owner, vamp, vac.RequiredClass));

        _actions.SetEnabled(action.AsNullable(), enabled);
    }

    private void TryGrantClassAbilities(EntityUid uid, VampireComponent comp)
    {
        if (string.IsNullOrWhiteSpace(comp.ChosenClassId))
            return;

        if (!_proto.TryIndex<VampireClassPrototype>(comp.ChosenClassId, out var classProto))
            return;

        foreach (var actionId in classProto.Actions)
            GrantAbility(uid, comp, actionId);
    }

    private void GrantAbility(EntityUid uid, VampireComponent comp, EntProtoId actionId)
    {
        if (comp.ActionEntities.ContainsKey(actionId))
            return;

        EntityUid? field = null;
        GrantAbility(uid, comp, ref field, actionId);
    }

    private void GrantAbility(EntityUid uid, VampireComponent comp, ref EntityUid? field, EntProtoId actionId)
    {
        if (field != null)
            return;

        var threshold = GetActionBloodThreshold(actionId);

        if (comp.TotalBlood >= threshold)
        {
            _actions.AddAction(uid, ref field, actionId, uid);
            if (field != null)
            {
                comp.ActionEntities[actionId] = field.Value;
                Dirty(uid, comp);
            }
        }
    }

    private void OnComponentRemove(EntityUid uid, VampireComponent comp, ComponentRemove _)
        => TryRemoveAbilities(uid, comp);

    private void TryRemoveAbilities(EntityUid uid, VampireComponent comp)
    {
        foreach (var (_, action) in comp.ActionEntities)
            _actions.RemoveAction(uid, action);
        comp.ActionEntities.Clear();
        Dirty(uid, comp);
    }

    private int GetActionBloodThreshold(EntProtoId actionId)
    {
        if (_proto.TryIndex<EntityPrototype>(actionId, out var proto) &&
            proto.TryGetComponent<VampireActionComponent>(out var vac, _componentFactory))
            return vac.BloodToUnlock;
        return 0;
    }
    private void EnsureRejuvenateUpgrade(EntityUid uid, VampireComponent comp)
    {
        if (comp.RejuvenateActions.Count < 2)
        {
            _sawmill?.Error($"Vampire {ToPrettyString(uid)} missing rejuvenate action config");
            return;
        }

        var rejuvenateI = comp.RejuvenateActions[0];
        var rejuvenateII = comp.RejuvenateActions[1];

        var unlockThreshold = GetActionBloodThreshold(rejuvenateII);
        if (comp.TotalBlood < unlockThreshold)
            return;

        if (!comp.ActionEntities.ContainsKey(rejuvenateII))
        {
            EntityUid? action = null;
            _actions.AddAction(uid, ref action, rejuvenateII, uid);
            if (action != null)
                comp.ActionEntities[rejuvenateII] = action.Value;
        }

        TryRefreshVampireAction(uid, comp.ActionEntities[rejuvenateII]);
        if (comp.ActionEntities.TryGetValue(rejuvenateI, out var firstAction))
        {
            _actions.RemoveAction(uid, firstAction);
            comp.ActionEntities.Remove(rejuvenateI);
        }

        Dirty(uid, comp);
    }

    private void HandleHolyWater(EntityUid uid, VampireComponent comp)
    {
        if (comp.UniqueHumanoidVictims < 1)
            return;

        if (_timing.CurTime < comp.NextHolyWaterTick)
            return;

        var holywater = _solution.GetTotalPrototypeQuantity(uid, comp.HolyWaterReagentId);
        if (holywater <= FixedPoint2.Zero)
            return;

        if (TryComp(uid, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
            return;

        comp.NextHolyWaterTick = _timing.CurTime + comp.HolyTickDelay;

        if (comp.DrunkBlood > 0)
        {
            TrySpendBlood(uid, comp, Math.Min(3, comp.DrunkBlood), showPopup: false);

            ApplyGroupDamage(uid, _bruteGroupId, 3f);

            if (TryComp(uid, out StaminaComponent? stamina))
                _stamina.TakeStaminaDamage(uid, 5f, stamina);

            return;
        }

        ApplyGroupDamage(uid, _burnGroupId, 2f);
        if (_rand.Prob(0.25f))
            _flammable.AdjustFireStacks(uid, 2f, ignite: true);
    }

    private void HandleHolyPlace(EntityUid uid, VampireComponent comp)
    {
        if (comp.UniqueHumanoidVictims < 1)
            return;

        if (_timing.CurTime < comp.NextHolyPlaceTick)
            return;

        if (!IsInHolyPlace(uid, comp))
            return;

        if (TryComp(uid, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
            return;

        comp.NextHolyPlaceTick = _timing.CurTime + comp.HolyTickDelay;

        if (_timing.CurTime >= comp.NextHolyPlacePopup)
        {
            _popup.PopupEntity(Loc.GetString("vampire-holy-place-burn"), uid, uid, PopupType.MediumCaution);
            comp.NextHolyPlacePopup = _timing.CurTime + TimeSpan.FromSeconds(5);
        }

        var health = GetApproximateHealth(uid);
        if (health <= 50f)
        {
            _flammable.AdjustFireStacks(uid, 3f, ignite: true);
            return;
        }

        if (_proto.TryIndex<DamageTypePrototype>(_heatTypeId, out var heat))
        {
            var spec = new DamageSpecifier(heat, FixedPoint2.New(3f));
            _damageableSystem.TryChangeDamage(uid, spec, true);
        }
    }

    private bool IsInHolyPlace(EntityUid uid, VampireComponent comp)
    {
        if (_container.IsEntityInContainer(uid))
            return false;

        var coords = Transform(uid).Coordinates;
        foreach (var ent in _lookup.GetEntitiesInRange(coords, comp.HolyPlaceRange, LookupFlags.Static))
        {
            if (ent == uid)
                continue;

            if (!HasComp<PrayableComponent>(ent))
                continue;

            if (!Transform(ent).Anchored)
                continue;

            if (!_interaction.InRangeUnobstructed(uid, ent, comp.HolyPlaceRange))
                continue;

            return true;
        }

        return false;
    }

    private float GetApproximateHealth(EntityUid uid)
    {
        if (!TryComp(uid, out DamageableComponent? damageable))
            return 100f;

        if (!_mobThreshold.TryGetDeadThreshold(uid, out var deadThreshold, CompOrNull<MobThresholdsComponent>(uid))
            || deadThreshold == null
            || deadThreshold.Value == FixedPoint2.Zero)
        {
            return 100f - damageable.TotalDamage.Float();
        }

        return deadThreshold.Value.Float() - damageable.TotalDamage.Float();
    }

    private void ApplyGroupDamage(EntityUid uid, ProtoId<DamageGroupPrototype> groupId, float amount)
    {
        if (!_proto.TryIndex<DamageGroupPrototype>(groupId, out var group))
            return;

        var spec = new DamageSpecifier(group, FixedPoint2.New(amount));
        _damageableSystem.TryChangeDamage(uid, spec, true);
    }

    private void OpenClassUi(EntityUid uid, VampireComponent comp)
    {
        if (!string.IsNullOrWhiteSpace(comp.ChosenClassId))
            return;
        _ui.CloseUi(uid, VampireClassUiKey.Key);
        _ui.OpenUi(uid, VampireClassUiKey.Key, uid);
    }

    private void OnVampireClassChosen(EntityUid uid, VampireComponent comp, VampireClassChosenBuiMsg msg)
    {
        if (!string.IsNullOrWhiteSpace(comp.ChosenClassId))
        {
            _ui.CloseUi(uid, VampireClassUiKey.Key);
            return;
        }

        if (string.IsNullOrWhiteSpace(msg.Choice) || !_proto.TryIndex<VampireClassPrototype>(msg.Choice, out var classProto))
        {
            _ui.CloseUi(uid, VampireClassUiKey.Key);
            return;
        }

        var reg = _componentFactory.GetRegistration(classProto.ClassComponent, ignoreCase: true);
        var classComp = _componentFactory.GetComponent(reg.Type);
        EntityManager.AddComponent(uid, classComp);

        if (classProto.ID == "Umbrae")
            EnsureComp<NightVisionComponent>(uid);

        comp.ChosenClassId = classProto.ID;
        _vampireClasses.WithLabels(classProto.ID).Inc();

        var classSelectAction = comp.ClassSelectActionId;
        if (comp.ActionEntities.TryGetValue(classSelectAction, out var actionEntity))
        {
            _actions.RemoveAction(uid, actionEntity);
            comp.ActionEntities.Remove(classSelectAction);
        }

        _ui.CloseUi(uid, VampireClassUiKey.Key);

        SyncVampireActions(uid, comp);

        Dirty(uid, comp);
    }

    private void OnVampireClassClosed(EntityUid uid, VampireComponent comp, VampireClassClosedBuiMsg _)
    {
        if (!string.IsNullOrWhiteSpace(comp.ChosenClassId))
            return;

        _sawmill?.Debug($"Vampire class UI closed without selection for {ToPrettyString(uid)} (blood={comp.TotalBlood})");
    }

    #region Objectives
    private void InitializeObjectives()
        => SubscribeLocalEvent<BloodDrainConditionComponent, ObjectiveGetProgressEvent>(OnBloodDrainGetProgress);

    private void OnBloodDrainGetProgress(EntityUid uid, BloodDrainConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        if (args.Mind.OwnedEntity != null && TryComp<VampireComponent>(args.Mind.OwnedEntity.Value, out var vampComp))
            args.Progress = target > 0 ? MathF.Min(vampComp.TotalBlood / target, 1f) : 1f;
        else
            args.Progress = 0f;
    }

    #endregion
}
