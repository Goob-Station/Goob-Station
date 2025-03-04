using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._White.Grab;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.MartialArts;

/// <summary>
/// Provides shared Martial Arts API.
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
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly INetManager _netManager = default!;

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
        while(kravSilencedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.SilencedTime)
                continue;
            RemComp<KravMagaSilencedComponent>(ent);
        }

        var kravBlockedQuery = EntityQueryEnumerator<KravMagaBlockedBreathingComponent>();
        while(kravBlockedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.BlockedTime)
                continue;
            RemComp<KravMagaBlockedBreathingComponent>(ent);
        }
    }
    #region Event Methods

    private void OnShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        var combo = EnsureComp<CanPerformComboComponent>(ent);
        combo.AllowedCombos.Clear();
    }

    private void CheckGrabStageOverride<T>(EntityUid uid, T component, CheckGrabOverridesEvent args)
        where T : GrabStagesOverrideComponent
    {
        if (args.Stage == GrabStage.Soft)
            args.Stage = component.StartingStage;
    }
    private void OnSilencedSpeakAttempt(Entity<KravMagaSilencedComponent> ent, ref SpeakAttemptEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"), ent, ent);   // You cant speak while someone is choking you
        args.Cancel();
    }
    private void OnShotAttempt(Entity<MartialArtsKnowledgeComponent> ent, ref ShotAttemptedEvent args)
    {
        if(ent.Comp.MartialArtsForm != MartialArtsForms.SleepingCarp)
            return;
        _popupSystem.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
    #endregion

     #region Helper Methods

    private bool TryGrant(GrantMartialArtKnowledgeComponent comp, EntityUid user)
    {
        if (!HasComp<CanPerformComboComponent>(user))
        {
            EnsureComp<CanPerformComboComponent>(user);
            var martialArtsKnowledgeComponent = EnsureComp<MartialArtsKnowledgeComponent>(user);
            LoadPrototype(user, martialArtsKnowledgeComponent, comp.MartialArtsForm);
            martialArtsKnowledgeComponent.Blocked = false;
            Dirty(user, martialArtsKnowledgeComponent);
            return true;
        }

        if (!TryComp<MartialArtsKnowledgeComponent>(user, out var cqc))
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-knowanother"), user, user);
            return false;
        }

        if (cqc.Blocked)
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-success-unblocked"), user, user);
            cqc.Blocked = false;
            comp.Used = true;
            return false;
        }

        _popupSystem.PopupEntity(Loc.GetString("cqc-fail-already"), user, user);
        return false;
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

    private void LoadPrototype(EntityUid uid, MartialArtsKnowledgeComponent component, MartialArtsForms name)
    {
        var combo = EnsureComp<CanPerformComboComponent>(uid);
        if (!_proto.TryIndex<MartialArtPrototype>(name.ToString(), out var martialArtsPrototype))
            return;
        component.MartialArtsForm = martialArtsPrototype.MartialArtsForm;
        component.RoundstartCombos = martialArtsPrototype.RoundstartCombos;
        component.MinDamageModifier = martialArtsPrototype.MinDamageModifier;
        component.MaxDamageModifier = martialArtsPrototype.MaxDamageModifier;
        component.RandomDamageModifier = martialArtsPrototype.RandomDamageModifier;
        component.HarmAsStamina = martialArtsPrototype.HarmAsStamina;
        component.RandomSayings = martialArtsPrototype.RandomSayings;
        component.RandomSayingsDowned = martialArtsPrototype.RandomSayingsDowned;
        LoadCombos(martialArtsPrototype.RoundstartCombos, combo);
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

        if (!knowledgeComponent.Blocked && knowledgeComponent.MartialArtsForm == form)
        {
            return true;
        }

        foreach (var entInRange in _lookup.GetEntitiesInRange(ent, 8f))
        {
            if (TryPrototype(entInRange, out var proto) && proto.ID == "DefaultStationBeaconKitchen")
                return true;
        }

        return false;
    }

    #endregion
}
