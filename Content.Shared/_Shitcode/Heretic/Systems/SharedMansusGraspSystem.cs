// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Religion;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.BackStab;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedMansusGraspSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BackStabSystem _backstab = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedVoidCurseSystem _voidCurse = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityLookupSystem _look = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedRatvarianLanguageSystem _language = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedHereticAbilitySystem _ability = default!;

    public static readonly SoundSpecifier DefaultSound = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkOnMeleeHitComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<HereticCombatMarkOnMeleeHitComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<AreaMansusGraspComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<AreaMansusGraspComponent, AreaGraspChannelDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MansusGraspComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<RustGraspComponent, AfterInteractEvent>(OnRustInteract);
    }

    public float GetAreaGraspRange(Entity<AreaMansusGraspComponent> ent, float time)
    {
        var blend = MathF.Pow(time / (float) ent.Comp.ChannelTime.TotalSeconds, ent.Comp.Slope);
        return MathHelper.Lerp(ent.Comp.MinRange, ent.Comp.MaxRange, blend);
    }

    public TimeSpan CalculateAreaGraspCooldown(float baseCooldown, int hitCount, float range)
    {
        var cd = baseCooldown * (1f + 2f * MathF.Pow(range, 0.8f) * (1f - 1f / (MathF.Pow(hitCount, 0.8f) + 1f)));
        return TimeSpan.FromSeconds(cd);
    }

    private void OnDoAfter(Entity<AreaMansusGraspComponent> ent, ref AreaGraspChannelDoAfterEvent args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (!TryComp(args.User, out HereticComponent? heretic) ||
            !TryComp(ent, out MansusGraspComponent? grasp) || args.Handled)
        {
            PredictedQueueDel(ent.Owner);
            return;
        }

        args.Handled = true;

        var time = args.DoAfter.CancelledTime is { } cancelTime
            ? cancelTime - args.DoAfter.StartTime
            : ent.Comp.ChannelTime;
        var range = GetAreaGraspRange(ent, (float) time.TotalSeconds);

        var pos = Transform(args.User).Coordinates;
        var look = _look.GetEntitiesInRange<MobStateComponent>(pos, range, LookupFlags.Dynamic);
        var hitCount = 0;

        foreach (var uid in look)
        {
            if (uid.Owner == args.User)
                continue;

            if (_examine.InRangeUnOccluded(args.User, uid, range) &&
                GraspTarget((ent, grasp), args.User, uid, false))
                hitCount++;
        }

        var cooldown = CalculateAreaGraspCooldown((float) grasp.CooldownAfterUse.TotalSeconds, hitCount, range);
        _actions.SetCooldown(heretic.MansusGrasp, cooldown);
        heretic.MansusGrasp = EntityUid.Invalid;
        InvokeGrasp(args.User, (ent, grasp));
        var spawned = PredictedSpawnAtPosition(ent.Comp.VisualEffect, pos);
        var effect = EnsureComp<AreaGraspEffectComponent>(spawned);
        effect.Size = range;
        effect.SpawnTime = _timing.CurTime;
        Dirty(spawned, effect);
        PredictedQueueDel(ent.Owner);
    }

    private void OnUseInHand(Entity<AreaMansusGraspComponent> ent, ref UseInHandEvent args)
    {
        args.Handled = true;

        var doArgs = new DoAfterArgs(EntityManager,
            args.User,
            ent.Comp.ChannelTime,
            new AreaGraspChannelDoAfterEvent(),
            ent)
        {
            ColorOverride = ent.Comp.EffectColor,
            BreakOnHandChange = false,
            MultiplyDelay = false,
        };

        if (!_doAfter.TryStartDoAfter(doArgs))
            return;

        _audio.PlayPredicted(ent.Comp.ChannelSound, args.User, args.User);

        ent.Comp.ChannelStartTime = _timing.CurTime;
        Dirty(ent);
    }

    private void OnMapInit(Entity<HereticCombatMarkOnMeleeHitComponent> ent, ref MapInitEvent args)
    {
        ResetPath(ent);
    }

    private void OnMelee(Entity<HereticCombatMarkOnMeleeHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !HasComp<HereticComponent>(args.User) && !HasComp<GhoulComponent>(args.User))
            return;

        foreach (var uid in args.HitEntities)
        {
            if (uid == args.User)
                continue;

            ApplyMark(uid, ent.Comp.NextPath);
        }

        ResetPath(ent);
    }

    private void ResetPath(Entity<HereticCombatMarkOnMeleeHitComponent> ent)
    {
        if (_net.IsClient)
            return;

        ent.Comp.NextPath = _random.Pick(ent.Comp.Paths);
        Dirty(ent);
    }

    public bool TryApplyGraspEffectAndMark(EntityUid user,
        HereticComponent hereticComp,
        EntityUid target,
        EntityUid? grasp,
        out bool triggerGrasp)
    {
        triggerGrasp = true;

        if (hereticComp.CurrentPath == null)
            return true;

        if (hereticComp.PathStage >= 2)
        {
            if (!ApplyGraspEffect((user, hereticComp), target, grasp, out var applyMark, out triggerGrasp))
                return false;

            if (!applyMark)
                return true;
        }

        if (hereticComp.PathStage >= 4)
            ApplyMark(target, hereticComp.CurrentPath);

        return true;
    }

    public bool GraspTarget(Entity<MansusGraspComponent> grasp,
        EntityUid user,
        EntityUid target,
        bool deleteGraspOnUse = true)
    {
        if (!TryComp<HereticComponent>(user, out var hereticComp))
        {
            if (!deleteGraspOnUse)
                return false;
            PredictedQueueDel(grasp.Owner);
            return true;
        }

        if (_whitelist.IsBlacklistPass(grasp.Comp.Blacklist, target))
            return false;

        var beforeEvent = new BeforeHarmfulActionEvent(user, HarmfulActionType.MansusGrasp);
        RaiseLocalEvent(target, beforeEvent);
        var cancelled = beforeEvent.Cancelled;
        if (!cancelled)
        {
            var ev = new BeforeCastTouchSpellEvent(target);
            RaiseLocalEvent(target, ev, true);
            cancelled = ev.Cancelled;
        }

        if (cancelled)
        {
            if (!deleteGraspOnUse)
                return false;
            _actions.SetCooldown(hereticComp.MansusGrasp, grasp.Comp.CooldownAfterUse);
            hereticComp.MansusGrasp = EntityUid.Invalid;
            InvokeGrasp(user, grasp);
            PredictedQueueDel(grasp.Owner);
            return true;
        }

        // upgraded grasp
        if (!TryApplyGraspEffectAndMark(user, hereticComp, target, grasp, out var triggerGrasp))
            return false;

        if (triggerGrasp && TryComp(target, out StatusEffectsComponent? status))
        {
            _stun.KnockdownOrStun(target, grasp.Comp.KnockdownTime, true, status);
            _stamina.TakeStaminaDamage(target, grasp.Comp.StaminaDamage);
            _language.DoRatvarian(target, grasp.Comp.SpeechTime, true, status);
            _statusEffect.TryAddStatusEffect<MansusGraspAffectedComponent>(target,
                "MansusGraspAffected",
                grasp.Comp.AffectedTime,
                true,
                status);
        }

        if (!deleteGraspOnUse)
            return true;

        _actions.SetCooldown(hereticComp.MansusGrasp, grasp.Comp.CooldownAfterUse);
        hereticComp.MansusGrasp = EntityUid.Invalid;
        InvokeGrasp(user, grasp);
        PredictedQueueDel(grasp.Owner);
        return true;
    }

    public void ApplyMark(EntityUid target, string path)
    {
        if (!HasComp<MobStateComponent>(target))
            return;

        RemComp<HereticCosmicMarkComponent>(target);
        var markComp = EnsureComp<HereticCombatMarkComponent>(target);
        markComp.DisappearTime = markComp.MaxDisappearTime;
        markComp.Path = path;
        markComp.Repetitions = path == "Ash" ? 5 : 1;
        Dirty(target, markComp);
        var ev = new UpdateCombatMarkAppearanceEvent();
        RaiseLocalEvent(target, ref ev);

        if (_net.IsClient || path != "Cosmos")
            return;

        var cosmosMark = EnsureComp<HereticCosmicMarkComponent>(target);
        cosmosMark.CosmicDiamondUid = Spawn(cosmosMark.CosmicDiamond, Transform(target).Coordinates);
        _transform.AttachToGridOrMap(cosmosMark.CosmicDiamondUid.Value);
    }

    public bool ApplyGraspEffect(Entity<HereticComponent> user,
        EntityUid target,
        EntityUid? grasp,
        out bool applyMark,
        out bool triggerGrasp)
    {
        applyMark = true;
        triggerGrasp = true;
        var (performer, heretic) = user;

        switch (heretic.CurrentPath)
        {
            case "Ash":
            {
                var timeSpan = TimeSpan.FromSeconds(5f);
                _statusEffect.TryAddStatusEffect(target,
                    TemporaryBlindnessSystem.BlindingStatusEffect,
                    timeSpan,
                    false,
                    TemporaryBlindnessSystem.BlindingStatusEffect);
                break;
            }

            case "Blade":
            {
                if (grasp != null && heretic.PathStage >= 7 && _tag.HasTag(target, "HereticBladeBlade"))
                {
                    // empowering blades and shit
                    var infusion = EnsureComp<MansusInfusedComponent>(target);
                    infusion.AvailableCharges = infusion.MaxCharges;
                    break;
                }

                // small stun if the person is looking away or laying down
                if (_backstab.TryBackstab(target, performer, Angle.FromDegrees(45d)))
                {
                    _stun.TryParalyze(target, TimeSpan.FromSeconds(1.5f), true);
                    _damage.TryChangeDamage(target,
                        new DamageSpecifier(_proto.Index<DamageTypePrototype>("Slash"), 10),
                        ignoreResistances: true,
                        origin: performer,
                        targetPart: TargetBodyPart.Chest);
                }

                break;
            }

            case "Lock":
            {
                if (!TryComp<DoorComponent>(target, out var door))
                    break;

                if (TryComp<DoorBoltComponent>(target, out var doorBolt))
                    _door.SetBoltsDown((target, doorBolt), false);

                _door.StartOpening(target, door);
                _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hereticknock.ogg"),
                    target,
                    user);
                break;
            }

            case "Flesh":
            {
                if (TryComp<MobStateComponent>(target, out var mobState) && mobState.CurrentState != MobState.Alive &&
                    !HasComp<BorgChassisComponent>(target))
                {
                    if (HasComp<GhoulComponent>(target))
                    {
                        _popup.PopupClient(Loc.GetString("heretic-ability-fail-target-ghoul"), user, user);
                        break;
                    }

                    if (!_mind.TryGetMind(target, out _, out _))
                    {
                        _popup.PopupClient(Loc.GetString("heretic-ability-fail-target-no-mind"), user, user);
                        break;
                    }

                    var ghoul = _compFactory.GetComponent<GhoulComponent>();
                    ghoul.BoundHeretic = performer;
                    ghoul.GiveBlade = true;

                    AddComp(target, ghoul);
                    applyMark = false;
                    triggerGrasp = false;
                }

                break;
            }

            case "Void":
            {
                _voidCurse.DoCurse(target, 2);
                break;
            }

            case "Rust":
            {
                if (TryComp(target, out StationAiHolderComponent? aiHolder)) // Kill AI
                    PredictedQueueDel(aiHolder.Slot.ContainerSlot?.ContainedEntity);
                else if (HasComp<RustGraspComponent>(grasp) && _tag.HasAnyTag(target, "Wall", "Catwalk") ||
                         HasComp<HereticRitualRuneComponent>(
                             target)) // If we have rust grasp and targeting a wall (or a catwalk) - do nothing, let other methods handle that. Also don't damage transmutation rune.
                    return false;
                else if (TryComp(target, out DamageableComponent? damageable) && // Is it even damageable?
                         !_tag.HasTag(target, "Meat") && // Is it not organic body part or organ?
                         !HasComp<ShadowCloakEntityComponent>(target) && // No instakilling shadow cloak heretics
                         (!HasComp<MobStateComponent>(target) || HasComp<SiliconComponent>(target) ||
                          HasComp<BorgChassisComponent>(target) ||
                          _tag.HasTag(target, "Bot"))) // Check for ingorganic target
                {
                    _damage.TryChangeDamage(target,
                        new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), 500),
                        ignoreResistances: true,
                        damageable: damageable,
                        origin: performer,
                        targetPart: TargetBodyPart.Chest);
                }

                break;
            }

            case "Cosmos":
            {
                if (_starMark.TryApplyStarMark(target))
                    _starMark.SpawnCosmicField(Transform(performer).Coordinates, heretic.PathStage);
                break;
            }
        }

        return true;
    }

    private void OnRustInteract(EntityUid uid, RustGraspComponent comp, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || !TryComp<HereticComponent>(args.User, out var heretic) ||
            !TryComp(uid, out UseDelayComponent? delay) || _delay.IsDelayed((uid, delay), comp.Delay) ||
            !TryComp(uid, out MansusGraspComponent? grasp))
            return;

        if (args.Target == null || _whitelist.IsBlacklistPass(grasp.Blacklist, args.Target.Value))
        {
            RustTile();
            return;
        }

        // Death to catwalks
        if (_tag.HasTag(args.Target.Value, "Catwalk"))
        {
            args.Handled = true;
            InvokeGrasp(args.User, (uid, grasp));
            ResetDelay(comp.CatwalkDelayMultiplier);
            Del(args.Target);
            return;
        }

        if (!_ability.TryMakeRustWall(args.Target.Value, (args.User, heretic)))
            return;

        args.Handled = true;
        InvokeGrasp(args.User, (uid, grasp));
        ResetDelay();

        return;

        void RustTile()
        {
            if (!args.ClickLocation.IsValid(EntityManager))
                return;

            if (!_mapManager.TryFindGridAt(_transform.ToMapCoordinates(args.ClickLocation), out var gridUid, out var mapGrid))
                return;

            var tileRef = _mapSystem.GetTileRef(gridUid, mapGrid, args.ClickLocation);
            var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];

            if (!_ability.CanRustTile(tileDef))
                return;

            args.Handled = true;
            ResetDelay();
            InvokeGrasp(args.User, (uid, grasp));

            _ability.MakeRustTile(gridUid, mapGrid, tileRef, comp.TileRune);
        }

        void ResetDelay(float multiplier = 1f)
        {
            // Less delay the higher the path stage is
            var length = float.Lerp(comp.MaxUseDelay, comp.MinUseDelay, heretic.PathStage / 10f) * multiplier;
            _delay.SetLength((uid, delay), TimeSpan.FromSeconds(length), comp.Delay);
            _delay.TryResetDelay((uid, delay), false, comp.Delay);
        }
    }

    private void OnMelee(Entity<MansusGraspComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;
        // blocked from wide attacks in YAML. should never have more than 1
        if (args.HitEntities.Count > 1)
            return;
        var target = args.HitEntities.First();
        // no fumbling!
        if (target == args.User)
            return;
        args.Handled = GraspTarget(ent, args.User, target);
    }

    private void OnAfterInteract(Entity<MansusGraspComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        args.Handled = GraspTarget(ent, args.User,args.Target.Value);
    }

    public virtual void InvokeGrasp(EntityUid user, Entity<MansusGraspComponent>? ent)
    {
        var sound = ent == null ? DefaultSound : ent.Value.Comp.Sound;
        _audio.PlayPredicted(sound, user, user);
    }
}

[Serializable, NetSerializable]
public sealed partial class AreaGraspChannelDoAfterEvent : SimpleDoAfterEvent;
