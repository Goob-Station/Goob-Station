// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.Grab;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
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
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
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

    public override void Initialize()
    {
        base.Initialize();
        InitializeKravMaga();
        InitializeSleepingCarp();
        InitializeCqc();
        InitializeCorporateJudo();
        InitializeCanPerformCombo();

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CheckGrabOverridesEvent>(CheckGrabStageOverride);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ShotAttemptedEvent>(OnShotAttempt);
        SubscribeLocalEvent<KravMagaSilencedComponent, SpeakAttemptEvent>(OnSilencedSpeakAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CanPerformComboComponent>();
        while (query.MoveNext(out _, out var comp))
        {
            if (_timing.CurTime < comp.ResetTime || comp.LastAttacks.Count <= 0)
                continue;
            comp.LastAttacks.Clear();
            comp.ConsecutiveGnashes = 0;
        }

        var kravSilencedQuery = EntityQueryEnumerator<KravMagaSilencedComponent>();
        while (kravSilencedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.SilencedTime)
                continue;
            RemComp<KravMagaSilencedComponent>(ent);
        }

        var kravBlockedQuery = EntityQueryEnumerator<KravMagaBlockedBreathingComponent>();
        while (kravBlockedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.BlockedTime)
                continue;
            RemComp<KravMagaBlockedBreathingComponent>(ent);
        }
    }

    #region Event Methods

    private void OnMeleeHit(Entity<MartialArtsKnowledgeComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Handled)
            return;

        if(!_proto.TryIndex<MartialArtPrototype>(ent.Comp.MartialArtsForm.ToString(), out var martialArtsPrototype))
            return;

        if (!martialArtsPrototype.RandomDamageModifier)
            return;

        var randomDamage = _random.Next(martialArtsPrototype.MinRandomDamageModifier, martialArtsPrototype.MaxRandomDamageModifier);
        var bonusDamageSpec = new DamageSpecifier();
        bonusDamageSpec.DamageDict.Add("Blunt", randomDamage);
        args.BonusDamage += bonusDamageSpec;
    }

    private void OnShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        if(TryComp<CanPerformComboComponent>(ent, out var comboComponent))
            comboComponent.AllowedCombos.Clear();
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
            comp.Used = true;
            return false;
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

        martialArtsKnowledgeComponent.MartialArtsForm = martialArtsPrototype.MartialArtsForm;
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
        newDamage.DamageDict.Add("Blunt", martialArtsPrototype.BaseDamageModifier);
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
        MartialArtsForms form,
        out EntityUid target,
        out bool downed)
    {
        target = EntityUid.Invalid;
        downed = false;

        if (ent.Comp.CurrentTarget == null)
            return false;

        if (!TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledgeComponent))
            return false;

        if (!TryComp<RequireProjectileTargetComponent>(ent.Comp.CurrentTarget, out var isDowned))
            return false;

        downed = isDowned.Active;
        target = ent.Comp.CurrentTarget.Value;

        if (knowledgeComponent.MartialArtsForm == form && !knowledgeComponent.Blocked)
        {
            return true;
        }

        foreach (var entInRange in _lookup.GetEntitiesInRange(ent, 8f))
        {
            if (!TryPrototype(entInRange, out var proto) || proto.ID != "SpawnPointChef" || !knowledgeComponent.Blocked)
                continue;
            return true;
        }

        return false;
    }

    private void DoDamage(EntityUid ent,
        EntityUid target,
        string damageType,
        int damageAmount,
        out DamageSpecifier damage,
        TargetBodyPart? targetBodyPart = null)
    {
        damage = new DamageSpecifier();
        if(!TryComp<TargetingComponent>(ent, out var targetingComponent))
            return;
        damage.DamageDict.Add(damageType, damageAmount);
        _damageable.TryChangeDamage(target, damage, origin: ent, targetPart: targetBodyPart ?? targetingComponent.Target);
    }

    #endregion
}
