using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.DoAfter;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.GrabIntent;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Pirate.Shared._JustDecor.MartialArts.Events;
using Content.Pirate.Shared._JustDecor.MartialArts.Components;
using Content.Shared.ActionBlocker;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.Grab;
using Content.Shared.Weapons.Melee;
using Content.Goobstation.Shared.MartialArts;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Throwing;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Pirate.Shared._JustDecor.MartialArts;

public sealed class SharedLegendaryCQCSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrowing = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SleepingSystem _sleeping = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCTakedownPerformedEvent>(OnLegendaryCQCTakedown);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCDisarmPerformedEvent>(OnLegendaryCQCDisarm);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCThrowPerformedEvent>(OnLegendaryCQCThrow);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCChokePerformedEvent>(OnLegendaryCQCChoke);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCChainPerformedEvent>(OnLegendaryCQCChain);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCCounterPerformedEvent>(OnLegendaryCQCCounter);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCInterrogationPerformedEvent>(OnLegendaryCQCInterrogation);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCStealthTakedownPerformedEvent>(OnLegendaryCQCStealthTakedown);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCRushPerformedEvent>(OnLegendaryCQCRush);
        SubscribeLocalEvent<CanPerformComboComponent, LegendaryCQCFinisherPerformedEvent>(OnLegendaryCQCFinisher);

        SubscribeLocalEvent<LegendaryCQCInterrogationDoAfterEvent>(OnInterrogationComplete);
        SubscribeLocalEvent<LegendaryCQCChokeDoAfterEvent>(OnChokeComplete);

        SubscribeLocalEvent<LegendaryCQCRushBuffComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<LegendaryCQCKnowledgeComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshCombatMovespeed);
        SubscribeLocalEvent<LegendaryCQCCounterBuffComponent, DamageModifyEvent>(OnCounterDamageModify);
        SubscribeLocalEvent<LegendaryCQCKnowledgeComponent, ComponentStartup>(OnKnowledgeStartup);
        SubscribeLocalEvent<CombatModeToggledEvent>(OnCombatModeToggled);

        SubscribeLocalEvent<LegendaryCQCKnowledgeComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<Components.GrantLegendaryCQCComponent, MapInitEvent>(OnGrantMapInit);
        SubscribeLocalEvent<Components.GrantLegendaryCQCComponent, UseInHandEvent>(OnLegendaryManualUsed);

        SubscribeLocalEvent<LegendaryCQCKnowledgeComponent, ComponentShutdown>(OnKnowledgeShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateBuffs(frameTime);
        UpdateCombatModes(frameTime);
        UpdateChokeholds(frameTime);
    }


    private void OnLegendaryCQCTakedown(Entity<CanPerformComboComponent> ent, ref LegendaryCQCTakedownPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto) || IsDown(target))
            return;

        if (!CheckCooldown(ent, "Takedown"))
            return;

        DoDamage(ent, target, "Blunt", proto.ExtraDamage);
        _stun.TryKnockdown(target, proto.ParalyzeTime, true);

        var staminaDamage = new DamageSpecifier();
        staminaDamage.DamageDict.Add("Stamina", 40f);
        _damageable.TryChangeDamage(target, staminaDamage, origin: ent);

        DropAllItems(target);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, "legendary-cqc-takedown-name");
        SetCooldown(ent, "Takedown", TimeSpan.FromSeconds(1.5));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCDisarm(Entity<CanPerformComboComponent> ent, ref LegendaryCQCDisarmPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Disarm"))
            return;

        var stoleAnyItems = StealAllItems(ent.Owner, target);
        if (stoleAnyItems && TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), target);
        ComboPopup(ent, target, "legendary-cqc-disarm-name");
        SetCooldown(ent, "Disarm", TimeSpan.FromSeconds(1));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCThrow(Entity<CanPerformComboComponent> ent, ref LegendaryCQCThrowPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Throw"))
            return;

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = (hitPos - mapPos).Normalized();

        DoDamage(ent, target, "Blunt", proto.ExtraDamage);
        _stun.TryKnockdown(target, proto.ParalyzeTime, true);
        _grabThrowing.Throw(target, ent, dir, 8f);

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        ComboPopup(ent, target, "legendary-cqc-throw-name");
        SetCooldown(ent, "Throw", TimeSpan.FromSeconds(1));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCChoke(Entity<CanPerformComboComponent> ent, ref LegendaryCQCChokePerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Choke"))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(3), new LegendaryCQCChokeDoAfterEvent(), ent, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        ComboPopup(ent, target, "legendary-cqc-popup-choke-started");
        SetCooldown(ent, "Choke", TimeSpan.FromSeconds(2));
        ClearLastAttacks(ent);
    }

    private void OnChokeComplete(LegendaryCQCChokeDoAfterEvent args)
    {
        if (args.Cancelled || args.Args.Target == null)
            return;

        var ent = args.Args.User;
        var target = args.Args.Target.Value;

        var staminaDamage = new DamageSpecifier();
        staminaDamage.DamageDict.Add("Stamina", 100f);
        _damageable.TryChangeDamage(target, staminaDamage, origin: ent);

        // Force sleep like a stealth takedown
        if (_netManager.IsServer)
        {
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(15), true);
            _sleeping.TrySleeping(target);
            _statusEffects.TryAddStatusEffectDuration(target, "StatusEffectForcedSleeping", out _, TimeSpan.FromSeconds(8));
        }

        ComboPopup(ent, target, "legendary-cqc-popup-choke-complete");
    }

    private void OnLegendaryCQCChain(Entity<CanPerformComboComponent> ent, ref LegendaryCQCChainPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Chain"))
            return;

        DoDamage(ent, target, "Blunt", proto.ExtraDamage);

        var staminaDamage = new DamageSpecifier();
        staminaDamage.DamageDict.Add("Stamina", proto.StaminaDamage);
        _damageable.TryChangeDamage(target, staminaDamage, origin: ent);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);

        Timer.Spawn(TimeSpan.FromMilliseconds(300), () =>
        {
            if (TerminatingOrDeleted(target) || TerminatingOrDeleted(ent))
                return;

            DoDamage(ent, target, "Blunt", proto.ExtraDamage / 2);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
        });

        ComboPopup(ent, target, "legendary-cqc-chain-name");
        SetCooldown(ent, "Chain", TimeSpan.FromSeconds(1.5));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCCounter(Entity<CanPerformComboComponent> ent, ref LegendaryCQCCounterPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Counter"))
            return;

        DoDamage(ent, target, "Blunt", 20f);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(8), true);

        var staminaDamage = new DamageSpecifier();
        staminaDamage.DamageDict.Add("Stamina", 60f);
        _damageable.TryChangeDamage(target, staminaDamage, origin: ent);

        var counterBuff = EnsureComp<LegendaryCQCCounterBuffComponent>(ent);
        counterBuff.EndTime = _timing.CurTime + TimeSpan.FromSeconds(2);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, "legendary-cqc-counter-name");
        SetCooldown(ent, "Counter", TimeSpan.FromSeconds(2));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCInterrogation(Entity<CanPerformComboComponent> ent, ref LegendaryCQCInterrogationPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!TryComp<NpcInterrogatableComponent>(target, out var interrogatable) || interrogatable.Prototype == null)
        {
            if (_netManager.IsServer)
                _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-invalid-interrogation-target"), ent, ent);
            return;
        }

        if (!CheckCooldown(ent, "Interrogation"))
            return;

        interrogatable.PendingLinkedAnswers = null;
        var questionToSay = Loc.GetString("legendary-cqc-say-interrogation-default-question");

        if (_proto.TryIndex(interrogatable.Prototype.Value, out var interrogationProto))
        {
            // 0 = General Question (weight based on count)
            // 1 = Linked Question (weight based on count)

            var options = new List<(string Question, List<string>? specificAnswers)>();

            foreach (var q in interrogationProto.Questions)
            {
                options.Add((q, null));
            }

            foreach (var linked in interrogationProto.Linked)
            {
                if (!string.IsNullOrWhiteSpace(linked.Question))
                {
                    options.Add((linked.Question, linked.Answers));
                }
            }

            if (options.Count > 0)
            {
                var selected = _random.Pick(options);
                questionToSay = selected.Question;
                interrogatable.PendingLinkedAnswers = selected.specificAnswers;
            }
        }

        TrySay(ent, questionToSay);

        // Start interrogation DoAfter
        var doAfterArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(3), new LegendaryCQCInterrogationDoAfterEvent(), ent, target)
        {
            BreakOnMove = true,
            BreakOnDamage = false,
            NeedHand = false
        };

        ApplyLegendaryPacification(target);

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
        {
            ClearLegendaryPacification(target);
            return;
        }

        ComboPopup(ent, target, "legendary-cqc-popup-interrogation-started");
        SetCooldown(ent, "Interrogation", TimeSpan.FromSeconds(2));
        ClearLastAttacks(ent);
    }

    private void OnInterrogationComplete(LegendaryCQCInterrogationDoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        if (args.Cancelled)
        {
            ClearLegendaryPacification(target);
            return;
        }

        var ent = args.Args.User;

        var staminaDamage = new DamageSpecifier();
        staminaDamage.DamageDict.Add("Stamina", 40f);
        _damageable.TryChangeDamage(target, staminaDamage, origin: ent);

        if (!TryComp<NpcInterrogatableComponent>(target, out var interrogatable))
        {
            ClearLegendaryPacification(target);
            return;
        }

        string? answer = null;

        if (interrogatable.Prototype != null && _proto.TryIndex(interrogatable.Prototype.Value, out var interrogationProto))
        {
            var potentialAnswers = interrogatable.PendingLinkedAnswers ?? interrogationProto.Answers;
            if (potentialAnswers.Count > 0)
            {
                answer = _random.Pick(potentialAnswers);
            }
        }

        if (string.IsNullOrEmpty(answer))
        {
            answer = Loc.GetString("legendary-cqc-say-interrogation-default-answer");
        }

        if (_netManager.IsServer)
        {
            Timer.Spawn(TimeSpan.FromMilliseconds(400), () =>
            {
                if (TerminatingOrDeleted(target))
                    return;
                TrySay(target, answer);
            });
        }

        interrogatable.Interrogated = true;
        interrogatable.PendingLinkedAnswers = null;

        EnsureComp<NpcEliminatedComponent>(target);
        if (HasComp<LegendaryCQCPacifiedComponent>(target))
            EnsureComp<Content.Shared.CombatMode.Pacification.PacifiedComponent>(target);

        _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-interrogation-success"), ent, ent);

        ComboPopup(ent, target, "legendary-cqc-popup-interrogation-complete");
    }

    private void OnLegendaryCQCStealthTakedown(Entity<CanPerformComboComponent> ent, ref LegendaryCQCStealthTakedownPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "StealthTakedown"))
            return;

        if (TryComp<CombatModeComponent>(target, out var combatMode) && combatMode.IsInCombatMode)
        {
            _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-stealth-fail-alert"), ent, ent);
            return;
        }

        DoDamage(ent, target, "Blunt", 25f);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(12), true);
        _statusEffects.TryAddStatusEffectDuration(target, "StatusEffectForcedSleeping", out _, TimeSpan.FromSeconds(8));

        ComboPopup(ent, target, "legendary-cqc-stealth-name");
        SetCooldown(ent, "StealthTakedown", TimeSpan.FromSeconds(2));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCRush(Entity<CanPerformComboComponent> ent, ref LegendaryCQCRushPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Rush"))
            return;

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = (hitPos - mapPos).Normalized();

        DoDamage(ent, target, "Blunt", proto.ExtraDamage);

        var staminaDamage = new DamageSpecifier();
        staminaDamage.DamageDict.Add("Stamina", proto.StaminaDamage);
        _damageable.TryChangeDamage(target, staminaDamage, origin: ent);
        _grabThrowing.Throw(target, ent, dir, 5f);

        var rushBuff = EnsureComp<LegendaryCQCRushBuffComponent>(ent);
        rushBuff.EndTime = _timing.CurTime + TimeSpan.FromSeconds(3);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        ComboPopup(ent, target, "legendary-cqc-rush-name");
        SetCooldown(ent, "Rush", TimeSpan.FromSeconds(1));
        ClearLastAttacks(ent);
    }

    private void OnLegendaryCQCFinisher(Entity<CanPerformComboComponent> ent, ref LegendaryCQCFinisherPerformedEvent args)
    {
        if (!TryGetTarget(ent, out var target, out var proto))
            return;

        if (!CheckCooldown(ent, "Finisher"))
            return;

        if (TryComp<DamageableComponent>(target, out var damageable))
        {
            damageable.Damage.DamageDict.TryGetValue("Stamina", out var staminaDamageValue);
            var staminaDamageAmount = staminaDamageValue;

            if (staminaDamageAmount <= 80f)
                return;

            DoDamage(ent, target, "Blunt", 40f);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(20), true);
            _statusEffects.TryAddStatusEffectDuration(target, "StatusEffectForcedSleeping", out _, TimeSpan.FromSeconds(15));

            if (TryComp(ent, out PullerComponent? puller) && puller.Pulling == target &&
                TryComp(ent, out GrabIntentComponent? grabIntent) &&
                TryComp(target, out PullableComponent? pullable) &&
                TryComp(target, out BodyComponent? body) &&
                grabIntent.GrabStage == GrabStage.Suffocate &&
                TryComp(ent, out TargetingComponent? targeting) &&
                targeting.Target == TargetBodyPart.Head &&
                _mobThreshold.TryGetDeadThreshold(target, out var damageToKill))
            {
                _pulling.TryStopPull(target, pullable);
                var blunt = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Blunt"), damageToKill.Value);
                _damageable.TryChangeDamage(target, blunt, true, targetPart: TargetBodyPart.Head);

                ComboPopup(ent, target, "legendary-cqc-popup-finishing-move");
            }
            else
            {
                ComboPopup(ent, target, "legendary-cqc-popup-devastator");
            }
        }
        else
        {
            DoDamage(ent, target, "Blunt", proto.ExtraDamage);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(8), true);
            ComboPopup(ent, target, "legendary-cqc-popup-heavy-strike");
        }

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        SetCooldown(ent, "Finisher", TimeSpan.FromSeconds(20));
        ClearLastAttacks(ent);
    }


    private void OnGrantMapInit(EntityUid uid, Components.GrantLegendaryCQCComponent component, MapInitEvent args)
    {
        if (!HasComp<MobStateComponent>(uid))
            return;

        GrantLegendaryCQC(uid);
    }

    private void OnLegendaryManualUsed(EntityUid uid, Components.GrantLegendaryCQCComponent comp, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!_netManager.IsServer)
            return;

        var user = args.User;

        if (HasComp<LegendaryCQCKnowledgeComponent>(user))
        {
            _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-already-known"), user, user);
            return;
        }

        GrantLegendaryCQC(user);

        _popup.PopupEntity(Loc.GetString("legendary-cqc-knowledge-gained"), user, user);

        var coords = Transform(user).Coordinates;
        _audio.PlayPvs(comp.SoundOnUse, coords);

        if (comp.MultiUse)
            return;

        QueueDel(uid);
        if (comp.SpawnedProto != null)
            Spawn(comp.SpawnedProto, coords);
    }

    private void GrantLegendaryCQC(EntityUid user)
    {
        if (HasComp<LegendaryCQCKnowledgeComponent>(user))
            return;

        var legendaryKnowledge = EnsureComp<LegendaryCQCKnowledgeComponent>(user);
        EnsureComp<LegendaryCQCCooldownsComponent>(user);
        var canPerformCombo = EnsureComp<CanPerformComboComponent>(user);
        EnsureComp<MartialArtsKnowledgeComponent>(user);
        EnsureComp<PullerComponent>(user);
        EnsureComp<MeleeWeaponComponent>(user);

        if (_proto.TryIndex<MartialArtPrototype>("LegendaryCloseQuartersCombat", out var martialArtsPrototype))
        {
            if (_proto.TryIndex(martialArtsPrototype.RoundstartCombos, out var comboListPrototype))
            {
                canPerformCombo.AllowedCombos.Clear();
                foreach (var item in comboListPrototype.Combos)
                {
                    canPerformCombo.AllowedCombos.Add(_proto.Index(item));
                }
            }

            if (TryComp<MartialArtsKnowledgeComponent>(user, out var knowledge))
            {
                knowledge.MartialArtsForm = MartialArtsForms.LegendaryCloseQuartersCombat;
                Dirty(user, knowledge);
            }
        }

        legendaryKnowledge.CombatMode = CompOrNull<CombatModeComponent>(user)?.IsInCombatMode ?? false;
        legendaryKnowledge.LastMoveTime = _timing.CurTime;
        Dirty(user, legendaryKnowledge);
        Dirty(user, canPerformCombo);
        _movementSpeed.RefreshMovementSpeedModifiers(user);
    }

    private void OnRefreshMovespeed(EntityUid uid, LegendaryCQCRushBuffComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (_timing.CurTime < comp.EndTime)
        {
            args.ModifySpeed(comp.SpeedMultiplier, comp.SpeedMultiplier);
        }
    }

    private void OnRefreshCombatMovespeed(EntityUid uid, LegendaryCQCKnowledgeComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (comp.CombatMode)
        {
            args.ModifySpeed(1.15f, 1.15f);
        }
    }

    private void OnCounterDamageModify(EntityUid uid, LegendaryCQCCounterBuffComponent comp, DamageModifyEvent args)
    {
        if (_timing.CurTime < comp.EndTime)
        {
            args.Damage *= comp.DamageReduction;

            if (_netManager.IsServer &&
                comp.ReflectDamage > 0f &&
                args.Origin is { } attacker &&
                attacker != uid)
            {
                _damageable.TryChangeDamage(attacker, args.OriginalDamage * comp.ReflectDamage, origin: uid);

                if (comp.CounterSound != null)
                    _audio.PlayPvs(comp.CounterSound, uid);
            }
        }
    }

    private void OnKnowledgeStartup(EntityUid uid, LegendaryCQCKnowledgeComponent comp, ComponentStartup args)
    {
        EnsureComp<LegendaryCQCCooldownsComponent>(uid);
        comp.CombatMode = CompOrNull<CombatModeComponent>(uid)?.IsInCombatMode ?? false;
        comp.LastMoveTime = _timing.CurTime;
        Dirty(uid, comp);
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }

    private void OnKnowledgeShutdown(EntityUid uid, LegendaryCQCKnowledgeComponent comp, ComponentShutdown args)
    {
        if (comp.OriginalAttackRate != null &&
            TryComp<MeleeWeaponComponent>(uid, out var melee))
        {
            melee.AttackRate = comp.OriginalAttackRate.Value;
            Dirty(uid, melee);
        }

        comp.OriginalAttackRate = null;
        RemCompDeferred<LegendaryCQCCooldownsComponent>(uid);

        if (!TerminatingOrDeleted(uid))
            _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }

    private void OnCombatModeToggled(ref CombatModeToggledEvent ev)
    {
        if (!TryComp<LegendaryCQCKnowledgeComponent>(ev.User, out var knowledge))
            return;

        if (knowledge.CombatMode == ev.Activated)
            return;

        knowledge.CombatMode = ev.Activated;
        Dirty(ev.User, knowledge);
        _movementSpeed.RefreshMovementSpeedModifiers(ev.User);
    }


    private void UpdateBuffs(float frameTime)
    {
        var query = EntityQueryEnumerator<LegendaryCQCRushBuffComponent>();
        while (query.MoveNext(out var uid, out var rushBuff))
        {
            if (_timing.CurTime >= rushBuff.EndTime)
            {
                RemComp<LegendaryCQCRushBuffComponent>(uid);
                _movementSpeed.RefreshMovementSpeedModifiers(uid);
            }
        }

        var counterQuery = EntityQueryEnumerator<LegendaryCQCCounterBuffComponent>();
        while (counterQuery.MoveNext(out var uid, out var counterBuff))
        {
            if (_timing.CurTime >= counterBuff.EndTime)
            {
                RemComp<LegendaryCQCCounterBuffComponent>(uid);
            }
        }
    }

    private void UpdateCombatModes(float frameTime)
    {
        var query = EntityQueryEnumerator<LegendaryCQCKnowledgeComponent>();
        while (query.MoveNext(out var uid, out var knowledge))
        {
            UpdateCombatState(uid, frameTime);

            // Haste decay logic
            if (knowledge.HasteMeter > 0)
            {
                knowledge.HasteMeter = Math.Max(0, knowledge.HasteMeter - knowledge.HasteDecayRate * frameTime);
                UpdateAttackSpeed(uid, knowledge);
            }

            if (!TryComp<PhysicsComponent>(uid, out var physics))
                continue;

            if (physics.LinearVelocity.LengthSquared() > knowledge.MinCounterVelocitySquared)
            {
                knowledge.LastMoveTime = _timing.CurTime;
            }
            else if (_netManager.IsServer &&
                     !HasComp<LegendaryCQCCounterBuffComponent>(uid) &&
                     !IsDown(uid) &&
                     _blocker.CanInteract(uid, null) &&
                     _timing.CurTime >= knowledge.LastMoveTime + knowledge.CounterStanceDelay)
            {
                var counterBuff = EnsureComp<LegendaryCQCCounterBuffComponent>(uid);
                counterBuff.EndTime = _timing.CurTime + knowledge.CounterStanceDuration;
                knowledge.LastMoveTime = _timing.CurTime;
            }

            if (physics.LinearVelocity.Length() > 0.1f)
                AddHaste(uid, knowledge, 0.05f * frameTime);
        }
    }

    private void UpdateAttackSpeed(EntityUid uid, LegendaryCQCKnowledgeComponent knowledge)
    {
        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        if (knowledge.OriginalAttackRate == null)
            knowledge.OriginalAttackRate = melee.AttackRate;

        var bonus = knowledge.HasteMeter * knowledge.MaxHasteBonus;
        melee.AttackRate = knowledge.OriginalAttackRate.Value * (1.0f + bonus);
        Dirty(uid, melee);
    }

    private void AddHaste(EntityUid uid, LegendaryCQCKnowledgeComponent knowledge, float amount)
    {
        knowledge.HasteMeter = Math.Min(1.0f, knowledge.HasteMeter + amount);
        UpdateAttackSpeed(uid, knowledge);
    }

    private void UpdateChokeholds(float frameTime)
    {
    }


    private bool CheckCooldown(EntityUid uid, string ability)
    {
        if (!TryComp<LegendaryCQCCooldownsComponent>(uid, out var cooldowns))
            return true;

        if (!cooldowns.CooldownTimers.TryGetValue(ability, out var cooldownEnd))
            return true;

        return _timing.CurTime >= cooldownEnd;
    }

    private void SetCooldown(EntityUid uid, string ability, TimeSpan duration)
    {
        var cooldowns = EnsureComp<LegendaryCQCCooldownsComponent>(uid);
        var cooldownEnd = _timing.CurTime + duration;
        cooldowns.CooldownTimers[ability] = cooldownEnd;
    }

    private void UpdateCombatState(EntityUid uid, float frameTime)
    {
        // Combo state reset is handled in SharedLegendaryCQCSystem.Update
    }

    private void DropAllItems(EntityUid uid)
    {
        foreach (var handName in _hands.EnumerateHands(uid))
        {
            _hands.TryDrop(uid, handName);
        }
    }

    private bool StealAllItems(EntityUid user, EntityUid target)
    {
        var stoleAnyItems = false;

        foreach (var handName in _hands.EnumerateHands(target))
        {
            var item = _hands.GetHeldItem(target, handName);
            if (item == null)
                continue;

            if (_hands.TryDrop(target, handName))
            {
                stoleAnyItems = true;
                _hands.TryPickupAnyHand(user, item.Value);
            }
        }

        return stoleAnyItems;
    }

    private void ApplyLegendaryPacification(EntityUid target)
    {
        if (HasComp<Content.Shared.CombatMode.Pacification.PacifiedComponent>(target))
            return;

        EnsureComp<Content.Shared.CombatMode.Pacification.PacifiedComponent>(target);
        EnsureComp<LegendaryCQCPacifiedComponent>(target);
    }

    private void ClearLegendaryPacification(EntityUid target)
    {
        if (!HasComp<LegendaryCQCPacifiedComponent>(target))
            return;

        RemComp<LegendaryCQCPacifiedComponent>(target);
        RemComp<Content.Shared.CombatMode.Pacification.PacifiedComponent>(target);
    }

    private bool TryGetTarget(Entity<CanPerformComboComponent> ent, out EntityUid target, [NotNullWhen(true)] out ComboPrototype? proto)
    {
        target = EntityUid.Invalid;
        proto = null;

        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out proto) ||
            ent.Comp.CurrentTarget == null)
            return false;

        target = ent.Comp.CurrentTarget.Value;
        return true;
    }

    private void DoDamage(EntityUid user, EntityUid target, string damageType, float amount)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.Add(damageType, amount);
        _damageable.TryChangeDamage(target, damage, origin: user);
    }

    private void OnMeleeHit(EntityUid uid, LegendaryCQCKnowledgeComponent knowledge, MeleeHitEvent args)
    {
        if (args.IsHit)
        {
            AddHaste(uid, knowledge, 0.25f);
        }

        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        if (physics.LinearVelocity.Length() < 4.0f)
            return;

        if (!CheckCooldown(uid, "RunningTackle"))
            return;

        var target = args.HitEntities[0];

        if (IsDown(target))
            return;

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(3), true);

        if (args.Direction != null)
        {
            var direction = args.Direction.Value.Normalized();
            _throwing.TryThrow(target, direction * 4, 15f); // 4 units distance
        }

        args.BonusDamage.DamageDict["Blunt"] = args.BonusDamage.DamageDict.GetValueOrDefault("Blunt") + 10f;

        _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-running-tackle-user"), uid, uid);
        _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-running-tackle-target"), target, target);

        SetCooldown(uid, "RunningTackle", TimeSpan.FromSeconds(5));
    }

    private void TrySay(EntityUid speaker, string message)
    {
        if (!_netManager.IsServer || string.IsNullOrWhiteSpace(message))
            return;

        _chat.TrySendInGameICMessage(speaker, message, InGameICChatType.Speak,
            hideChat: false, hideLog: true, checkRadioPrefix: false);
    }

    private void ComboPopup(EntityUid user, EntityUid target, string comboLocId)
    {
        if (!_netManager.IsServer)
            return;

        var comboName = Loc.GetString(comboLocId);
        _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-user", ("combo", comboName)), user, user);
        _popup.PopupEntity(Loc.GetString("legendary-cqc-popup-target", ("combo", comboName)), target, target);
    }

    private void ClearLastAttacks(Entity<CanPerformComboComponent> ent)
    {
        ent.Comp.LastAttacks.Clear();
    }

    private bool IsDown(EntityUid uid)
    {
        return _standingState.IsDown(uid);
    }
}
