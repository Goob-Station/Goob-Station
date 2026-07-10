using System.Numerics;
using Content.Server.Actions;
using Content.Goobstation.Common.Religion;
using Content.Server.Light.EntitySystems;
using Content.Server.Temperature.Systems;
using Content.Pirate.Shared.Vampire;
using Content.Pirate.Shared.Vampire.Components;
using Content.Pirate.Shared.Vampire.Components.Classes;
using Content.Pirate.Shared.Vampire.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Light.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Temperature.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Mobs;

namespace Content.Pirate.Server.Vampire.Systems;

public sealed class UmbraeSystem : EntitySystem
{
    private static readonly ProtoId<DamageTypePrototype> _bluntTypeId = "Blunt";

    [Dependency] private readonly VampireSystem _vampire = default!;

    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    [Dependency] private readonly PoweredLightSystem _poweredLightSystem = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedUmbraeSystem _sharedUmbrae = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, VampireDarkPassageActionEvent>(OnDarkPassage);
        SubscribeLocalEvent<VampireComponent, VampireExtinguishActionEvent>(OnExtinguish);
        SubscribeLocalEvent<VampireComponent, VampireEternalDarknessActionEvent>(OnEternalDarkness);
        SubscribeLocalEvent<VampireComponent, VampireShadowAnchorActionEvent>(OnShadowAnchor);
        SubscribeLocalEvent<VampireComponent, VampireShadowAnchorDoAfterEvent>(OnShadowAnchorDoAfter);
        SubscribeLocalEvent<VampireComponent, VampireShadowSnareActionEvent>(OnShadowSnare);
        SubscribeLocalEvent<VampireShadowBoxingStartAttemptEvent>(OnShadowBoxingStartAttempt);

        SubscribeLocalEvent<UmbraeComponent, VampireBloodDrankEvent>(OnBloodDrank);
        SubscribeLocalEvent<UmbraeComponent, VampireFullPowerAchievedEvent>(OnFullPower);
        SubscribeLocalEvent<UmbraeComponent, MobStateChangedEvent>(OnUmbraeMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        ProcessShadowAnchorAutoReturns(now);
        ProcessActiveEternalDarkness(now);
        ProcessActiveShadowBoxing(now);
    }

    private void OnBloodDrank(EntityUid uid, UmbraeComponent umbrae, ref VampireBloodDrankEvent args)
    {
        if (!TryComp<VampireComponent>(uid, out var vampire))
            return;

        if (vampire.TotalBlood < umbrae.BreakLightBloodThreshold)
            return;

        TryBreakRandomLightNear(uid, umbrae.BreakLightRange);
    }

    private void OnUmbraeMobStateChanged(EntityUid uid, UmbraeComponent umbrae, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Critical)
            return;

        if (!umbrae.CloakOfDarknessActive)
            return;

        _sharedUmbrae.DeactivateCloakOfDarkness(uid, umbrae);

        if (TryComp<VampireComponent>(uid, out var vampire)
            && vampire.ActionEntities.TryGetValue("ActionVampireCloakOfDarkness", out var actionEntity)
            && _actions.GetAction(actionEntity) is { } action)
        {
            _actions.SetToggled(action.AsNullable(), false);
        }
    }

    private void TryBreakRandomLightNear(EntityUid uid, float range)
    {
        var center = Transform(uid).Coordinates;
        var list = new List<EntityUid>();

        foreach (var ent in _lookup.GetEntitiesInRange(center, range))
        {
            if (TryComp<PoweredLightComponent>(ent, out var light) && light.On)
                list.Add(ent);
        }

        if (list.Count == 0)
            return;

        var pick = _rand.Pick(list);

        if (TryComp<PoweredLightComponent>(pick, out var pl))
            _poweredLightSystem.TryDestroyBulb(pick, pl);
    }

    private void OnShadowSnare(EntityUid uid, VampireComponent comp, ref VampireShadowSnareActionEvent args)
    {
        if (args.Handled
            || !TryComp<UmbraeComponent>(uid, out var umbrae)
            || !HasComp<UmbraeComponent>(uid))
            return;

        var target = args.Target;
        var curXform = Transform(uid);
        if (curXform.MapID != _transform.GetMapId(target)
            || !_transform.GetGrid(target).HasValue)
            return;

        if (!_vampire.IsValidTile(target))
        {
            _popup.PopupEntity(Loc.GetString("action-vampire-shadow-snare-wrong-place"), uid, uid);
            return;
        }

        if (!_vampire.CheckAndConsumeBloodCost(uid, comp, args.Action.Owner))
            return;

        umbrae.PlacedSnares.RemoveAll(e => !Exists(e));

        if (umbrae.PlacedSnares.Count >= umbrae.MaxSnares)
        {
            var oldestSnare = umbrae.PlacedSnares[0];
            umbrae.PlacedSnares.RemoveAt(0);
            if (Exists(oldestSnare))
            {
                QueueDel(oldestSnare);
                _popup.PopupEntity(Loc.GetString("vampire-shadow-snare-oldest-removed"), uid, uid);
            }
        }

        var snare = EntityManager.SpawnEntity(args.SnarePrototype, target);
        umbrae.PlacedSnares.Add(snare);
        Dirty(uid, umbrae);

        _popup.PopupEntity(Loc.GetString("action-vampire-shadow-snare-placed"), uid, uid);
        args.Handled = true;
    }

    private void OnDarkPassage(EntityUid uid, VampireComponent comp, ref VampireDarkPassageActionEvent args)
    {
        if (args.Handled
            || !HasComp<UmbraeComponent>(uid))
            return;

        var target = args.Target;
        var curXform = Transform(uid);
        if (curXform.MapID != _transform.GetMapId(target)
            || !_transform.GetGrid(target).HasValue)
            return;

        if (!_vampire.IsValidTile(target)
            || (!comp.FullPower
                && !_interaction.InRangeUnobstructed(uid, target, range: 100, collisionMask: CollisionGroup.Impassable, popup: false)))
        {
            _popup.PopupEntity(Loc.GetString("action-vampire-dark-passage-wrong-place"), uid, uid);
            return;
        }

        if (!_vampire.CheckAndConsumeBloodCost(uid, comp, args.Action.Owner))
            return;

        EntityManager.SpawnEntity(args.MistInPrototype, curXform.Coordinates);

        _transform.SetCoordinates(uid, target);
        _transform.AttachToGridOrMap(uid, curXform);

        EntityManager.SpawnEntity(args.MistOutPrototype, target);

        _popup.PopupEntity(Loc.GetString("action-vampire-dark-passage-activated"), uid, uid);
        _audio.PlayPvs(args.Sound, uid, AudioParams.Default.WithVolume(-1f));
        args.Handled = true;
    }

    private void OnExtinguish(EntityUid uid, VampireComponent comp, ref VampireExtinguishActionEvent args)
    {
        if (args.Handled
            || !comp.ActionEntities.TryGetValue("ActionVampireExtinguish", out var actionEntity)
            || !HasComp<UmbraeComponent>(uid)
            || !_vampire.CheckAndConsumeBloodCost(uid, comp, actionEntity))
            return;

        var center = Transform(uid).Coordinates;

        var toProcess = _lookup.GetEntitiesInRange(center, args.Radius);
        var count = 0;
        foreach (var ent in toProcess)
        {
            if (ent == uid)
                continue;

            if (TryComp<PoweredLightComponent>(ent, out var light))
            {
                _poweredLightSystem.TryDestroyBulb(ent, light);
                count++;
            }
        }

        _popup.PopupEntity(Loc.GetString("action-vampire-extinguish-activated", ("count", count)), uid, uid);
        args.Handled = true;
    }

    private void OnEternalDarkness(EntityUid uid, VampireComponent comp, ref VampireEternalDarknessActionEvent args)
    {
        if (args.Handled
            || !TryComp<UmbraeComponent>(uid, out var umbrae)
            || !HasComp<UmbraeComponent>(uid))
            return;

        if (!umbrae.EternalDarknessActive)
        {
            if (!comp.FullPower)
            {
                _popup.PopupEntity(Loc.GetString("action-vampire-not-enough-power"), uid, uid);
                args.Handled = true;
                return;
            }

            umbrae.EternalDarknessActive = true;
        }
        else
        {
            umbrae.EternalDarknessActive = false;
        }

        Dirty(uid, umbrae);

        if (_actions.GetAction(args.Action.Owner) is { } action)
            _actions.SetToggled(action.AsNullable(), umbrae.EternalDarknessActive);

        if (umbrae.EternalDarknessActive)
        {
            _popup.PopupEntity(Loc.GetString("action-vampire-eternal-darkness-start"), uid, uid);
            umbrae.EternalDarknessLoopId++;
            if (umbrae.EternalDarknessAuraEntity == null || !Exists(umbrae.EternalDarknessAuraEntity))
            {
                var aura = Spawn(args.AuraPrototype, Transform(uid).Coordinates);
                umbrae.EternalDarknessAuraEntity = aura;
                _transform.SetParent(aura, uid);
            }

            var active = EnsureComp<ActiveVampireEternalDarknessComponent>(uid);
            active.TicksRemaining = Math.Max(1, args.MaxTicks);
            active.CurrentTick = 0;
            active.BloodPerTick = args.BloodPerTick;
            active.TempDropInterval = args.TempDropInterval;
            active.FreezeRadius = args.FreezeRadius;
            active.TargetFreezeTemp = args.TargetFreezeTemp;
            active.TempDropPerInterval = args.TempDropPerInterval;
            active.NextTick = _timing.CurTime;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("action-vampire-eternal-darkness-stop"), uid, uid);
            if (umbrae.EternalDarknessAuraEntity != null && Exists(umbrae.EternalDarknessAuraEntity))
                QueueDel(umbrae.EternalDarknessAuraEntity.Value);
            umbrae.EternalDarknessAuraEntity = null;
            RemComp<ActiveVampireEternalDarknessComponent>(uid);
        }

        args.Handled = true;
    }

    private void ProcessActiveEternalDarkness(TimeSpan now)
    {
        var query = EntityQueryEnumerator<ActiveVampireEternalDarknessComponent, VampireComponent, UmbraeComponent>();
        while (query.MoveNext(out var uid, out var active, out var comp, out var umbrae))
        {
            if (now < active.NextTick)
                continue;

            if (active.TicksRemaining <= 0)
            {
                DeactivateEternalDarkness(uid, comp, umbrae);
                continue;
            }

            if (!umbrae.EternalDarknessActive
                || !ValidateEternalDarknessConditions(uid, comp, umbrae)
                || !ConsumeEternalDarknessBlood(uid, comp, umbrae, active.BloodPerTick))
            {
                continue;
            }

            ProcessEternalDarknessEffects(uid, active.CurrentTick, active.TempDropInterval, active.FreezeRadius, active.TargetFreezeTemp,
                active.TempDropPerInterval);

            active.CurrentTick++;
            active.TicksRemaining--;

            if (active.TicksRemaining <= 0)
            {
                DeactivateEternalDarkness(uid, comp, umbrae);
                continue;
            }

            active.NextTick = now + TimeSpan.FromSeconds(1);
        }
    }

    private bool ValidateEternalDarknessConditions(EntityUid uid, VampireComponent comp, UmbraeComponent umbrae)
    {
        if (TryComp<MobStateComponent>(uid, out var mob) && mob.CurrentState == MobState.Dead)
        {
            DeactivateEternalDarkness(uid, comp, umbrae);
            return false;
        }

        return true;
    }

    private bool ConsumeEternalDarknessBlood(EntityUid uid, VampireComponent comp, UmbraeComponent umbrae, int bloodPerTick)
    {
        if (comp.DrunkBlood < bloodPerTick)
        {
            DeactivateEternalDarkness(uid, comp, umbrae, Loc.GetString("action-vampire-eternal-darkness-not-enough-blood"));
            return false;
        }

        return _vampire.TrySpendBlood(uid, comp, bloodPerTick);
    }

    private void DeactivateEternalDarkness(EntityUid uid, VampireComponent comp, UmbraeComponent umbrae, string? message = null)
    {
        umbrae.EternalDarknessActive = false;

        if (comp.ActionEntities.TryGetValue("ActionVampireEternalDarkness", out var actionEntity) && _actions.GetAction(actionEntity) is { } action)
            _actions.SetToggled(action.AsNullable(), false);

        if (umbrae.EternalDarknessAuraEntity != null && Exists(umbrae.EternalDarknessAuraEntity))
            QueueDel(umbrae.EternalDarknessAuraEntity.Value);

        umbrae.EternalDarknessAuraEntity = null;
        RemComp<ActiveVampireEternalDarknessComponent>(uid);

        if (message != null)
            _popup.PopupEntity(message, uid, uid);

        Dirty(uid, umbrae);
    }

    private void ProcessEternalDarknessEffects(EntityUid uid,
        int tick,
        int dropInterval,
        float freezeRadius,
        float targetTemp,
        float tempDrop)
    {
        var vampXform = Transform(uid);
        var center = _transform.GetWorldPosition(vampXform);

        var doCoolingThisTick = (tick % dropInterval) == 0;
        if (doCoolingThisTick)
            ProcessTemperatureEffects(uid, vampXform, center, freezeRadius, targetTemp, tempDrop);
    }

    private void ProcessTemperatureEffects(EntityUid uid,
        TransformComponent vampXform,
        Vector2 center,
        float freezeRadius,
        float targetTemp,
        float tempDrop)
    {
        foreach (var ent in _lookup.GetEntitiesInRange(vampXform.Coordinates, freezeRadius))
        {
            if (ent == uid || !HasComp<HumanoidAppearanceComponent>(ent) || HasComp<VampireComponent>(ent))
                continue;

            if (!TryComp<TemperatureComponent>(ent, out var temp))
                continue;

            var targetXform = Transform(ent);
            var distance = (_transform.GetWorldPosition(targetXform) - center).Length();

            if (distance > freezeRadius || temp.CurrentTemperature <= targetTemp)
                continue;

            var remaining = temp.CurrentTemperature - targetTemp;
            var drop = Math.Min(tempDrop, remaining);

            _temperatureSystem.ForceChangeTemperature(ent, temp.CurrentTemperature - drop, temp);
        }
    }

    private void OnShadowAnchor(EntityUid uid, VampireComponent comp, ref VampireShadowAnchorActionEvent args)
    {
        if (args.Handled
            || !TryComp<UmbraeComponent>(uid, out var umbrae)
            || !HasComp<UmbraeComponent>(uid))
            return;

        if (umbrae.SpawnedShadowAnchorBeacon != null && Exists(umbrae.SpawnedShadowAnchorBeacon))
        {
            ReturnToShadowAnchor(uid, umbrae);
            args.Handled = true;
            return;
        }

        if (umbrae.ShadowAnchorPlacementInProgress)
        {
            args.Handled = true;
            return;
        }

        if (!TryComp<VampireActionComponent>(args.Action.Owner, out var vac))
            return;

        if (comp.TotalBlood < vac.BloodToUnlock)
            return;

        var bloodCost = (int)vac.BloodCost;
        if (bloodCost > 0 && comp.DrunkBlood < bloodCost)
        {
            _popup.PopupEntity(Loc.GetString("vampire-not-enough-blood"), uid, uid);
            return;
        }

        var pressedCoords = Transform(uid).Coordinates;
        var tileCoords = pressedCoords.WithPosition(pressedCoords.Position.Floored() + new Vector2(0.5f, 0.5f));

        var ev = new VampireShadowAnchorDoAfterEvent(GetNetCoordinates(tileCoords), args.BeaconPrototype, bloodCost, args.AutoReturnDelay);
        var doAfter = new DoAfterArgs(EntityManager, uid, args.PlaceDelay, ev, uid)
        {
            DistanceThreshold = null,
            BreakOnDamage = false,
            BreakOnMove = false,
            RequireCanInteract = false,
            BlockDuplicate = true,
            CancelDuplicate = true
        };

        umbrae.ShadowAnchorPlacementInProgress = true;

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            umbrae.ShadowAnchorPlacementInProgress = false;
            return;
        }

        args.Handled = true;
    }

    private void OnShadowAnchorDoAfter(EntityUid uid, VampireComponent comp, ref VampireShadowAnchorDoAfterEvent args)
    {
        if (!TryComp<UmbraeComponent>(uid, out var umbrae))
            return;

        umbrae.ShadowAnchorPlacementInProgress = false;

        if (args.Handled || args.Cancelled)
            return;

        if (!HasComp<UmbraeComponent>(uid))
            return;

        if (!_vampire.CheckAndConsumeBloodCost(uid, comp, null, args.BloodCost))
            return;

        if (umbrae.SpawnedShadowAnchorBeacon != null && Exists(umbrae.SpawnedShadowAnchorBeacon))
            return;

        var coords = GetCoordinates(args.TargetCoordinates);
        var newBeacon = EntityManager.SpawnEntity(args.BeaconPrototype, coords);
        umbrae.SpawnedShadowAnchorBeacon = newBeacon;
        umbrae.ShadowAnchorLoopId++;
        umbrae.ShadowAnchorAutoReturnTime = _timing.CurTime + args.AutoReturnDelay;
        Dirty(uid, umbrae);

        _popup.PopupEntity(Loc.GetString("action-vampire-shadow-anchor-installed"), uid, uid);
    }

    private void ProcessShadowAnchorAutoReturns(TimeSpan now)
    {
        var query = EntityQueryEnumerator<UmbraeComponent>();
        while (query.MoveNext(out var uid, out var umbrae))
        {
            if (umbrae.ShadowAnchorAutoReturnTime is not { } returnTime || now < returnTime)
                continue;

            AutoReturnToShadowAnchor(uid, umbrae.ShadowAnchorLoopId);
        }
    }

    private void AutoReturnToShadowAnchor(EntityUid uid, int expectedLoopId)
    {
        if (!Exists(uid) || !TryComp<UmbraeComponent>(uid, out var umbrae))
            return;

        if (umbrae.ShadowAnchorLoopId != expectedLoopId)
            return;

        if (umbrae.SpawnedShadowAnchorBeacon == null || !Exists(umbrae.SpawnedShadowAnchorBeacon))
            return;

        ReturnToShadowAnchor(uid, umbrae);
    }

    private void ReturnToShadowAnchor(EntityUid uid, UmbraeComponent umbrae)
    {
        if (umbrae.SpawnedShadowAnchorBeacon == null || !Exists(umbrae.SpawnedShadowAnchorBeacon))
        {
            umbrae.SpawnedShadowAnchorBeacon = null;
            umbrae.ShadowAnchorAutoReturnTime = null;
            Dirty(uid, umbrae);
            return;
        }

        var beacon = umbrae.SpawnedShadowAnchorBeacon.Value;
        var coords = Transform(beacon).Coordinates;
        _transform.SetCoordinates(uid, coords);
        _transform.AttachToGridOrMap(uid, Transform(uid));

        QueueDel(beacon);
        umbrae.SpawnedShadowAnchorBeacon = null;
        umbrae.ShadowAnchorAutoReturnTime = null;
        umbrae.ShadowAnchorLoopId++;
        Dirty(uid, umbrae);

        _popup.PopupEntity(Loc.GetString("action-vampire-shadow-anchor-returned"), uid, uid);
    }

    private void OnShadowBoxingStartAttempt(ref VampireShadowBoxingStartAttemptEvent ev)
    {
        var uid = ev.Performer;
        var target = ev.Target;
        if (!HasComp<BibleUserComponent>(target)
            || TryComp<VampireComponent>(uid, out var vampire) && vampire.FullPower
            || !HasComp<VampireComponent>(uid))
            return;

        _popup.PopupEntity(Loc.GetString("vampire-target-protected-by-faith"), uid, uid, PopupType.MediumCaution);
        ev.Cancelled = true;
    }

    private void ProcessActiveShadowBoxing(TimeSpan now)
    {
        var query = EntityQueryEnumerator<ActiveVampireShadowBoxingComponent, UmbraeComponent>();
        while (query.MoveNext(out var uid, out var active, out var umbrae))
        {
            if (now < active.NextTick)
                continue;

            if (now >= active.EndTime || !umbrae.ShadowBoxingActive)
            {
                _sharedUmbrae.StopShadowBoxing(uid, umbrae, "action-vampire-shadow-boxing-ends");
                continue;
            }

            var target = active.Target;
            if (!Exists(target)
                || !HasComp<DamageableComponent>(target)
                || (TryComp<MobStateComponent>(target, out var mob) && mob.CurrentState == MobState.Dead))
            {
                active.NextTick = now + active.TickInterval;
                continue;
            }

            var sourceXform = Transform(uid);
            var targetXform = Transform(target);
            if (sourceXform.MapID != targetXform.MapID)
            {
                active.NextTick = now + active.TickInterval;
                continue;
            }

            var curDist = (_transform.GetWorldPosition(sourceXform) - _transform.GetWorldPosition(targetXform)).Length();
            if (curDist <= active.Range)
            {
                var spec = new DamageSpecifier(_proto.Index<DamageTypePrototype>(_bluntTypeId), FixedPoint2.New(active.BrutePerTick));
                _damageableSystem.TryChangeDamage(target, spec, true, origin: uid);

                if (active.HitSound != null)
                    _audio.PlayPvs(active.HitSound, target);

                var punchEffect = Spawn(active.PunchEffectPrototype, Transform(target).Coordinates);
                _transform.SetParent(punchEffect, target);
                RaiseNetworkEvent(new VampireShadowBoxingPunchEvent(GetNetEntity(uid), GetNetEntity(target)));
            }

            active.NextTick = now + active.TickInterval;
        }
    }

    private void OnFullPower(EntityUid uid, UmbraeComponent umbrae, VampireFullPowerAchievedEvent args)
    {
        _eye.SetDrawFov(uid, false);
        _popup.PopupEntity(Loc.GetString("vampire-umbrae-full-power-fov"), uid, uid, PopupType.Large);
    }
}
