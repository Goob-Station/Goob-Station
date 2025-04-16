// SPDX-FileCopyrightText: 2024 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 github-actions <github-actions@github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Hands.Systems;
using Content.Server.Heretic.Abilities;
using Content.Server.Heretic.Components;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Item;
using Content.Server.Popups;
using Content.Server.Speech.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Heretic.Components;
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
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.EntitySystems;

public sealed class MansusGraspSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly BackStabSystem _backstab = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly HereticAbilitySystem _ability = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<RustGraspComponent, AfterInteractEvent>(OnRustInteract);
        SubscribeLocalEvent<HereticComponent, DrawRitualRuneDoAfterEvent>(OnRitualRuneDoAfter);
        SubscribeLocalEvent<MansusGraspBlockTriggerComponent, BeforeTriggerEvent>(OnTriggerAttempt);

        SubscribeLocalEvent<MansusInfusedComponent, ExaminedEvent>(OnInfusedExamine);
        SubscribeLocalEvent<MansusInfusedComponent, InteractHandEvent>(OnInfusedInteract);
        SubscribeLocalEvent<MansusInfusedComponent, MeleeHitEvent>(OnInfusedMeleeHit);
        SubscribeLocalEvent<MansusInfusedComponent, ComponentStartup>(OnInfusedStartup);
        SubscribeLocalEvent<MansusInfusedComponent, ComponentShutdown>(OnInfusedShutdown);
    }

    private void OnTriggerAttempt(Entity<MansusGraspBlockTriggerComponent> ent, ref BeforeTriggerEvent args)
    {
        if (HasComp<MansusGraspAffectedComponent>(args.User))
        {
            args.Cancel();
            _popup.PopupEntity(Loc.GetString("mansus-grasp-trigger-fail"), args.User.Value, args.User.Value);
        }
        else if (HasComp<MansusGraspAffectedComponent>(Transform(ent).ParentUid))
            args.Cancel();
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

        // Already rusted walls are destroyed
        if (HasComp<RustedWallComponent>(args.Target))
        {
            if (!_ability.CanSurfaceBeRusted(args.Target.Value, (args.User, heretic)))
                return;

            args.Handled = true;
            InvokeGrasp(args.User, (uid, grasp));
            ResetDelay();
            Del(args.Target);
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

    private void OnAfterInteract(Entity<MansusGraspComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        var (uid, comp) = ent;

        if (!TryComp<HereticComponent>(args.User, out var hereticComp))
        {
            QueueDel(uid);
            args.Handled = true;
            return;
        }

        var target = args.Target.Value;

        if ((TryComp<HereticComponent>(target, out var th) && th.CurrentPath == ent.Comp.Path))
            return;

        if (_whitelist.IsBlacklistPass(comp.Blacklist, target))
            return;

        // upgraded grasp
        if (!TryApplyGraspEffectAndMark( args.User, hereticComp, target, ent))
            return;

        if (TryComp(target, out StatusEffectsComponent? status))
        {
            _stun.KnockdownOrStun(target, comp.KnockdownTime, true, status);
            _stamina.TakeStaminaDamage(target, comp.StaminaDamage);
            _language.DoRatvarian(target, comp.SpeechTime, true, status);
            _statusEffect.TryAddStatusEffect<MansusGraspAffectedComponent>(target,
                "MansusGraspAffected",
                ent.Comp.AffectedTime,
                true,
                status);
        }

        _actions.SetCooldown(hereticComp.MansusGrasp, ent.Comp.CooldownAfterUse);
        hereticComp.MansusGrasp = EntityUid.Invalid;
        InvokeGrasp(args.User, ent);
        QueueDel(ent);
        args.Handled = true;
    }

    public bool TryApplyGraspEffectAndMark( EntityUid user,
        HereticComponent hereticComp,
        EntityUid target,
        EntityUid? grasp)
    {
        if (hereticComp.CurrentPath == null)
            return false;

        if (hereticComp.PathStage >= 2)
        {
            if (!ApplyGraspEffect((user, hereticComp), target, grasp))
                return false;
        }

        if (hereticComp.PathStage >= 4 && HasComp<StatusEffectsComponent>(target))
        {
            var markComp = EnsureComp<HereticCombatMarkComponent>(target);
            markComp.Path = hereticComp.CurrentPath;
            markComp.Repetitions = hereticComp.CurrentPath == "Ash" ? 5 : 1;
        }

        return true;
    }

    private void InvokeGrasp(EntityUid user, Entity<MansusGraspComponent> ent)
    {
        _audio.PlayPvs(ent.Comp.Sound, user);
        _chat.TrySendInGameICMessage(user, Loc.GetString(ent.Comp.Invocation), InGameICChatType.Speak, false);
    }

    private void OnAfterInteract(Entity<TagComponent> ent, ref AfterInteractEvent args)
    {
        var tags = ent.Comp.Tags;

        if (!args.CanReach
            || !args.ClickLocation.IsValid(EntityManager)
            || !TryComp<HereticComponent>(args.User, out var heretic) // not a heretic - how???
            || HasComp<ActiveDoAfterComponent>(args.User)) // prevent rune shittery
            return;

        var runeProto = "HereticRuneRitualDrawAnimation";
        float time = 14;

        if (TryComp(ent, out TransmutationRuneScriberComponent? scriber)) // if it is special rune scriber
        {
            runeProto = scriber.RuneDrawingEntity;
            time = scriber.Time;
        }
        else if (heretic.MansusGrasp == EntityUid.Invalid // no grasp - not special
                 || !tags.Contains("Write") || !tags.Contains("Pen")) // not a pen
            return;

        args.Handled = true;

        // remove our rune if clicked
        if (args.Target != null && HasComp<HereticRitualRuneComponent>(args.Target))
        {
            // todo: add more fluff
            QueueDel(args.Target);
            return;
        }

        // spawn our rune
        var rune = Spawn(runeProto, args.ClickLocation);
        _transform.AttachToGridOrMap(rune);
        var dargs = new DoAfterArgs(EntityManager, args.User, time, new DrawRitualRuneDoAfterEvent(rune, args.ClickLocation), args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            CancelDuplicate = false,
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnRitualRuneDoAfter(Entity<HereticComponent> ent, ref DrawRitualRuneDoAfterEvent ev)
    {
        // delete the animation rune regardless
        QueueDel(ev.RitualRune);

        if (!ev.Cancelled)
            _transform.AttachToGridOrMap(Spawn("HereticRuneRitual", ev.Coords));
    }

    public bool ApplyGraspEffect(Entity<HereticComponent> user, EntityUid target, EntityUid? grasp)
    {
        var (performer, heretic) = user;

        switch (heretic.CurrentPath)
        {
            case "Ash":
                {
                    var timeSpan = TimeSpan.FromSeconds(5f);
                    _statusEffect.TryAddStatusEffect(target, TemporaryBlindnessSystem.BlindingStatusEffect, timeSpan, false, TemporaryBlindnessSystem.BlindingStatusEffect);
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
                            targetPart: TargetBodyPart.Torso);
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
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hereticknock.ogg"), target);
                    break;
                }

            case "Flesh":
                {
                    if (TryComp<MobStateComponent>(target, out var mobState) && mobState.CurrentState == Shared.Mobs.MobState.Dead)
                    {
                        var ghoul = EnsureComp<GhoulComponent>(target);
                        ghoul.BoundHeretic = GetNetEntity(performer);
                    }
                    break;
                }

            case "Void":
                {
                    if (TryComp<TemperatureComponent>(target, out var temp))
                        _temperature.ForceChangeTemperature(target, temp.CurrentTemperature - 20f, temp);
                    _statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(8), false);
                    break;
                }

            case "Rust":
                {
                    if (TryComp(target, out StationAiHolderComponent? aiHolder)) // Kill AI
                        QueueDel(aiHolder.Slot.ContainerSlot?.ContainedEntity);
                    else if (HasComp<RustGraspComponent>(grasp) && _tag.HasAnyTag(target, "Wall", "Catwalk") ||
                             HasComp<HereticRitualRuneComponent>( target)) // If we have rust grasp and targeting a wall (or a catwalk) - do nothing, let other methods handle that. Also don't damage transmutation rune.
                        return false;
                    else if (TryComp(target, out DamageableComponent? damageable) && // Is it even damageable?
                             !_tag.HasTag(target, "Meat") && // Is it not organic body part or organ?
                             (!HasComp<MobStateComponent>(target) || HasComp<SiliconComponent>(target) ||
                              HasComp<BorgChassisComponent>(target) || _tag.HasTag(target, "Bot"))) // Check for ingorganic target
                    {
                        _damage.TryChangeDamage(target,
                            new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), 500),
                            ignoreResistances: true,
                            damageable: damageable,
                            origin: performer,
                            targetPart: TargetBodyPart.Torso);
                    }
                    break;
                }

            default:
                return true;
        }

        return true;
    }

    #region Infused items

    private void OnInfusedExamine(Entity<MansusInfusedComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mansus-infused-item-examine"));
    }
    private void OnInfusedInteract(Entity<MansusInfusedComponent> ent, ref InteractHandEvent args)
    {
        var target = args.User;

        if (HasComp<HereticComponent>(target) || HasComp<GhoulComponent>(target))
            return;

        if (HasComp<StatusEffectsComponent>(target))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryParalyze(target, TimeSpan.FromSeconds(5f), true);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(10f), true);
        }

        if (TryComp<HandsComponent>(target, out var hands))
            _hands.TryDrop(target, Transform(target).Coordinates, handsComp: hands);

        SpendInfusionCharges(ent);
    }
    private void OnInfusedMeleeHit(Entity<MansusInfusedComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (!TryComp(args.User, out HereticComponent? heretic))
            return;

        var success = false;
        foreach (var target in args.HitEntities)
        {
            if (target == args.User)
                continue;

            if (!HasComp<StatusEffectsComponent>(target) && !HasComp<MobStateComponent>(target))
                continue;

            if ((TryComp<HereticComponent>(target, out var th) && th.CurrentPath == heretic.CurrentPath))
                continue;

            if (!TryApplyGraspEffectAndMark(args.User, heretic, target, null))
                continue;

            success = true;
        }

        if (success)
            SpendInfusionCharges(ent);
    }

    private void SpendInfusionCharges(Entity<MansusInfusedComponent> ent)
    {
        ent.Comp.AvailableCharges -= 1;
        if (ent.Comp.AvailableCharges <= 0)
            RemComp<MansusInfusedComponent>(ent);
    }

    private void OnInfusedStartup(Entity<MansusInfusedComponent> ent, ref ComponentStartup args)
    {
        _appearance.SetData(ent, InfusedBladeVisuals.Infused, true);
        _item.SetHeldPrefix(ent, ent.Comp.HeldPrefix);
    }

    private void OnInfusedShutdown(Entity<MansusInfusedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _appearance.SetData(ent, InfusedBladeVisuals.Infused, false);
        _item.SetHeldPrefix(ent, null);
    }


    #endregion
}
