// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Bokser815 <70928915+Bokser815@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.Sprinting;
using Content.Goobstation.Shared.Stealth;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.BackStab;
using Content.Shared._White.Grab;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts;

/// <summary>
/// Handles most of Martial Arts Systems.
/// </summary>
public abstract partial class SharedMartialArtsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly Content.Shared.StatusEffect.StatusEffectsSystem _status = default!;
    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _newStatus = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrowing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifier = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly BackStabSystem _backstab = default!;
    [Dependency] private readonly SharedGoobStealthSystem _stealth = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedSprintingSystem _sprinting = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeKravMaga();
        InitializeSleepingCarp();
        InitializeCqc();
        InitializeCorporateJudo();
        InitializeCapoeira();
        InitializeDragon();
        InitializeNinjutsu();
        InitializeHellRip();
        InitializeCanPerformCombo();

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CheckGrabOverridesEvent>(CheckGrabStageOverride);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ShotAttemptedEvent>(OnShotAttempt);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComboAttackPerformedEvent>(OnComboAttackPerformed);

        SubscribeLocalEvent<KravMagaSilencedComponent, SpeakAttemptEvent>(OnSilencedSpeakAttempt);

        SubscribeLocalEvent<MartialArtModifiersComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
        SubscribeLocalEvent<MartialArtModifiersComponent, RefreshMovementSpeedModifiersEvent>(OnGetMovespeed);

        SubscribeLocalEvent<StatusEffectContainerComponent, BeforeStaminaDamageEvent>(OnBeforeStatusStamina);

        SubscribeLocalEvent<MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<InteractHandEvent>(OnInteract);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<CanPerformComboComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (comp.CurrentTarget != null && TerminatingOrDeleted(comp.CurrentTarget.Value))
                comp.CurrentTarget = null;

            if (_timing.CurTime < comp.ResetTime
                || comp.LastAttacks.Count == 0
                && comp.ConsecutiveGnashes == 0)
                continue;

            comp.LastAttacks.Clear();
            comp.ConsecutiveGnashes = 0;
            Dirty(ent, comp);
        }

        var kravSilencedQuery = EntityQueryEnumerator<KravMagaSilencedComponent>();
        while (kravSilencedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.SilencedTime)
                continue;
            RemCompDeferred(ent, comp);
        }

        var kravBlockedQuery = EntityQueryEnumerator<KravMagaBlockedBreathingComponent>();
        while (kravBlockedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.BlockedTime)
                continue;
            RemCompDeferred(ent, comp);
        }

        var meleeAttackRateMultiplierQuery = EntityQueryEnumerator<MartialArtModifiersComponent>();
        while (meleeAttackRateMultiplierQuery.MoveNext(out var ent, out var multiplier))
        {
            if (_timing.CurTime < multiplier.NextUpdate)
                continue;

            double? nextUpdate = null;
            var refreshSpeed = false;
            for (var i = multiplier.Data.Count - 1; i >= 0; i--)
            {
                var data = multiplier.Data[i];

                if (_timing.CurTime < data.EndTime)
                {
                    nextUpdate = nextUpdate == null
                        ? data.EndTime.TotalSeconds
                        : Math.Min(nextUpdate.Value, data.EndTime.TotalSeconds);
                    continue;
                }

                if ((data.Type & MartialArtModifierType.MoveSpeed) != 0)
                    refreshSpeed = true;

                multiplier.Data.RemoveAt(i);
            }

            if (refreshSpeed)
                _modifier.RefreshMovementSpeedModifiers(ent);

            if (multiplier.Data.Count == 0)
                RemCompDeferred(ent, multiplier);
            else
            {
                if (nextUpdate != null)
                    multiplier.NextUpdate = TimeSpan.FromSeconds(nextUpdate.Value);
                Dirty(ent, multiplier);
            }
        }

        if (_netManager.IsClient)
            return;

        var dragonQuery =
            EntityQueryEnumerator<DragonKungFuTimerComponent, StatusEffectsComponent, MobStateComponent, PhysicsComponent>();
        while (dragonQuery.MoveNext(out var uid, out var timer, out var status, out var mobState, out var physics))
        {
            if (mobState.CurrentState != MobState.Alive)
                continue;

            if (physics.LinearVelocity.LengthSquared() > timer.MinVelocitySquared)
            {
                timer.LastMoveTime = _timing.CurTime;
                continue;
            }

            if (!_blocker.CanInteract(uid, null)
                || _timing.CurTime < timer.LastMoveTime + timer.PauseDuration)
                continue;

            _status.TryAddStatusEffect<DragonPowerBuffComponent>(uid,
                "DragonPower",
                timer.BuffLength,
                true,
                status);

            // So that it doesn't update constantly
            timer.LastMoveTime = _timing.CurTime;
        }
    }

    #region Event Methods

    private void OnBeforeStatusStamina(Entity<StatusEffectContainerComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (!_newStatus.TryEffectsWithComp<StaminaResistanceModifierStatusEffectComponent>(ent, out var effects))
            return;

        foreach (var effect in effects)
        {
            args.Value *= effect.Comp1.Modifier;
        }
    }

    private void OnInteract(InteractHandEvent args)
    {
        if (_netManager.IsClient
            || args.User == args.Target
            || !HasComp<MobStateComponent>(args.Target)
            || !TryComp(args.User, out MartialArtsKnowledgeComponent? knowledge))
            return;

        if (knowledge.MartialArtsForm == MartialArtsForms.Ninjutsu)
            OnNinjutsuHug(args.User, args.Target);

        // Including this in combos clutters combo counter
        // RaiseLocalEvent(args.User, new ComboAttackPerformedEvent(args.User, args.Target, args.User, ComboAttackType.Hug));
    }

    private void OnComboAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (ent.Comp.Blocked)
        {
            var ev = new CanDoCQCEvent();
            RaiseLocalEvent(ent, ev);
            if (!ev.Handled)
                return;
        }

        switch (ent.Comp.MartialArtsForm)
        {
            case MartialArtsForms.CloseQuartersCombat:
                OnCQCAttackPerformed(ent, ref args);
                break;
            case MartialArtsForms.Capoeira:
                OnCapoeiraAttackPerformed(ent, ref args);
                break;
        }
    }

    private void OnGetMovespeed(Entity<MartialArtModifiersComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var (mult, _) = GetMultiplierModifier(ent, MartialArtModifierType.MoveSpeed, null);
        args.ModifySpeed(mult, mult);
    }

    private DamageModifierSet GetDamageModifierSet(DamageSpecifier specifier, float multiplier, float modifier)
    {
        return new()
        {
            Coefficients = specifier.DamageDict
                .Select(x => KeyValuePair.Create(x.Key, multiplier))
                .ToDictionary(),
            FlatReduction = specifier.DamageDict
                .Select(x => KeyValuePair.Create(x.Key, -modifier)) // Minus mod because it subtracts values from damage
                .ToDictionary(),
        };
    }

    private void OnGetMeleeAttackRate(Entity<MartialArtModifiersComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        var (mult, mod) = GetMultiplierModifier(ent, MartialArtModifierType.AttackRate, args.Weapon != args.User);
        args.Multipliers *= mult;
        args.Rate += mod;
    }

    private (float mult, float mod) GetMultiplierModifier(Entity<MartialArtModifiersComponent> ent,
        MartialArtModifierType type,
        bool? armed)
    {
        var mult = 1f;
        var mod = 0f;
        foreach (var data in ent.Comp.Data.Where(x => (x.Type & type) != 0))
        {
            if (armed is true)
            {
                if ((data.Type & MartialArtModifierType.Armed) == 0
                    && (data.Type & MartialArtModifierType.Unarmed) != 0)
                    continue;
            }
            else if (armed is false)
            {
                if ((data.Type & MartialArtModifierType.Unarmed) == 0
                    && (data.Type & MartialArtModifierType.Armed) != 0)
                    continue;
            }
            mult *= data.Multiplier;
            mod += data.Modifier;
        }

        foreach (var (_, limit) in ent.Comp.MinMaxModifiersMultipliers.Where(x => (x.Key & type) != 0))
        {
            mult = Math.Clamp(mult, limit.X, limit.Y);
            mod = Math.Clamp(mod, limit.Z, limit.W);
        }

        return (mult, mod);
    }

    private void OnMeleeHit(MeleeHitEvent args)
    {
        if (args.Handled)
            return;

        var ent = args.User;

        if (TryComp<MartialArtModifiersComponent>(ent, out var modifiers))
        {
            var (mult, mod) =
                GetMultiplierModifier((ent, modifiers), MartialArtModifierType.Damage, args.Weapon != ent);
            args.ModifiersList.Add(GetDamageModifierSet(args.BaseDamage, mult, mod));
        }

        if (!TryComp(ent, out MartialArtsKnowledgeComponent? comp))
            return;

        switch (comp.MartialArtsForm)
        {
            case MartialArtsForms.Ninjutsu:
                OnNinjutsuMeleeHit(ent, ref args);
                break;
            case MartialArtsForms.Capoeira:
                OnCapoeiraMeleeHit(ent, ref args);
                break;
        }


        if (args.Weapon != ent
            || !_proto.TryIndex<MartialArtPrototype>(comp.MartialArtsForm.ToString(), out var martialArtsPrototype))
            return;

        if (!martialArtsPrototype.RandomDamageModifier)
            return;

        var randomDamage = _random.Next(martialArtsPrototype.MinRandomDamageModifier, martialArtsPrototype.MaxRandomDamageModifier + 1);
        var bonusDamageSpec = new DamageSpecifier();
        bonusDamageSpec.DamageDict.Add(martialArtsPrototype.DamageModifierType, randomDamage);
        args.BonusDamage += bonusDamageSpec;
    }

    private void OnShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if(TryComp<CanPerformComboComponent>(ent, out var comboComponent))
            comboComponent.AllowedCombos.Clear();

        RemCompDeferred<DragonKungFuTimerComponent>(ent);
    }

    private void CheckGrabStageOverride<T>(EntityUid uid, T component, CheckGrabOverridesEvent args)
        where T : GrabStagesOverrideComponent
    {
        if (args.Stage == GrabStage.Soft)
            args.Stage = component.StartingStage;
    }

    private void OnSilencedSpeakAttempt(Entity<KravMagaSilencedComponent> ent, ref SpeakAttemptEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"),
            ent,
            ent); // You cant speak while someone is choking you
        args.Cancel();
    }

    private void OnShotAttempt(Entity<MartialArtsKnowledgeComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.MartialArtsForm != MartialArtsForms.SleepingCarp)
            return;
        _popupSystem.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }

    private void ComboPopup(EntityUid user, EntityUid target, string comboName)
    {
        if (!_netManager.IsServer)
            return;
        var userName = Identity.Entity(user, EntityManager);
        var targetName = Identity.Entity(target, EntityManager);
        _popupSystem.PopupEntity(Loc.GetString("martial-arts-action-sender",
            ("name", targetName),
            ("move", comboName)),
            user,
            user);
        _popupSystem.PopupEntity(Loc.GetString("martial-arts-action-receiver",
            ("name", userName),
            ("move", comboName)),
            target,
            target);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Tries to grant a martial art to a user. Use this method.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="comp"></param>
    /// <returns></returns>
    private bool TryGrantMartialArt(EntityUid user, GrantMartialArtKnowledgeComponent comp)
    {
        if (!_netManager.IsServer || MetaData(user).EntityLifeStage >= EntityLifeStage.Terminating)
            return false;

        if (HasComp<ChangelingIdentityComponent>(user))
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-changeling"), user, user);
            return false;
        }

        if (HasComp<KravMagaComponent>(user))
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-knowanother"), user, user);
            return false;
        }

        if (!HasComp<CanPerformComboComponent>(user))
        {
            return GrantMartialArt(comp, user);
        }

        if (!TryComp<MartialArtsKnowledgeComponent>(user, out var cqc))
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-knowanother"), user, user);
            return false;
        }

        if (cqc.Blocked && comp.MartialArtsForm == MartialArtsForms.CloseQuartersCombat)
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-success-unblocked"), user, user);
            cqc.Blocked = false;
            return true;
        }

        _popupSystem.PopupEntity(Loc.GetString("cqc-fail-already"), user, user);
        return false;
    }

    private bool GrantMartialArt(GrantMartialArtKnowledgeComponent comp, EntityUid user)
    {
        var canPerformComboComponent = EnsureComp<CanPerformComboComponent>(user);
        var martialArtsKnowledgeComponent = EnsureComp<MartialArtsKnowledgeComponent>(user);
        var pullerComponent = EnsureComp<PullerComponent>(user);

        if (!_proto.TryIndex<MartialArtPrototype>(comp.MartialArtsForm.ToString(), out var martialArtsPrototype)
            || !TryComp<MeleeWeaponComponent>(user, out var meleeWeaponComponent))
            return false;

        if (comp.LearnMessage != null)
            _popupSystem.PopupEntity(Loc.GetString(comp.LearnMessage), user, user);

        switch (martialArtsPrototype.MartialArtsForm)
        {
            case MartialArtsForms.KungFuDragon:
                EnsureComp<DragonKungFuTimerComponent>(user);
                break;
            case MartialArtsForms.Ninjutsu:
                EnsureComp<NinjutsuSneakAttackComponent>(user);
                break;
            case MartialArtsForms.CloseQuartersCombat:
                var riposte = EnsureComp<RiposteeComponent>(user);
                riposte.Data.TryAdd("CQC",
                    new(0.1f,
                    false,
                    null,
                    true,
                    new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"),
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(4),
                    false,
                    0.75f,
                    null,
                    null,
                    new CanDoCQCEvent()));
                break;
        }

        martialArtsKnowledgeComponent.MartialArtsForm = martialArtsPrototype.MartialArtsForm;
        //martialArtsKnowledgeComponent.StartingStage = martialArtsPrototype.StartingStage;
        LoadCombos(martialArtsPrototype.RoundstartCombos, canPerformComboComponent);
        martialArtsKnowledgeComponent.Blocked = false;

        if (meleeWeaponComponent.Damage.DamageDict.Count != 0)
        {
            martialArtsKnowledgeComponent.OriginalFistDamage =
                meleeWeaponComponent.Damage.DamageDict.Values.ElementAt(0).Float();
            martialArtsKnowledgeComponent.OriginalFistDamageType =
                meleeWeaponComponent.Damage.DamageDict.Keys.ElementAt(0);
        }

        var newDamage = new DamageSpecifier();
        newDamage.DamageDict.Add(martialArtsPrototype.DamageModifierType, martialArtsPrototype.BaseDamageModifier);
        meleeWeaponComponent.Damage += newDamage;

        Dirty(user, canPerformComboComponent);
        Dirty(user, pullerComponent);
        return true;
    }

    private void LoadCombos(ProtoId<ComboListPrototype> list, CanPerformComboComponent combo)
    {
        combo.AllowedCombos.Clear();
        if (!_proto.TryIndex(list, out var comboListPrototype))
            return;
        foreach (var item in comboListPrototype.Combos)
        {
            combo.AllowedCombos.Add(_proto.Index(item));
        }
    }

    private bool TryUseMartialArt(Entity<CanPerformComboComponent> ent,
        ComboPrototype proto,
        out EntityUid target,
        out bool downed)
    {
        target = EntityUid.Invalid;
        downed = false;

        if (ent.Comp.CurrentTarget == null)
            return false;

        if (!TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledgeComponent))
            return false;

        if (knowledgeComponent.MartialArtsForm != proto.MartialArtsForm)
            return false;

        if (!proto.CanDoWhileProne && IsDown(ent))
        {
            _popupSystem.PopupEntity(Loc.GetString("martial-arts-fail-prone"), ent, ent);
            return false;
        }

        downed = IsDown(ent.Comp.CurrentTarget.Value);
        target = ent.Comp.CurrentTarget.Value;

        if (!knowledgeComponent.Blocked)
            return true;

        // TODO: fix blocked martial art supercode
        var ev = new CanDoCQCEvent();
        RaiseLocalEvent(ent, ev);
        return ev.Handled;

        bool IsDown(EntityUid uid)
        {
            if (!TryComp<StandingStateComponent>(uid, out var standingState))
                return false;

            return standingState.CurrentState != StandingState.Standing;
        }
    }

    private void DoDamage(EntityUid ent,
        EntityUid target,
        string damageType,
        float damageAmount,
        out DamageSpecifier damage,
        TargetBodyPart? targetBodyPart = null)
    {
        damage = new DamageSpecifier();
        if(!TryComp<TargetingComponent>(ent, out var targetingComponent))
            return;
        damage.DamageDict.Add(damageType, damageAmount);
        if (TryComp(ent, out MartialArtModifiersComponent? modifiers))
        {
            var (mult, mod) = GetMultiplierModifier((ent, modifiers), MartialArtModifierType.Damage, false);
            var modifierSet = GetDamageModifierSet(damage, mult, mod);
            damage = DamageSpecifier.ApplyModifierSet(damage, modifierSet);
        }
        _damageable.TryChangeDamage(target,
            damage,
            origin: ent,
            targetPart: targetBodyPart ?? targetingComponent.Target);
    }

    #endregion
}
