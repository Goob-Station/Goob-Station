using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.MartialArts;
using Content.Shared._White.Grab;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.MartialArts;

public sealed partial class MartialArtsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrowing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeSleepingCarp();
        InitializeCqc();
        InitializeCorporateJudo();

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CheckGrabOverridesEvent>(CheckGrabStageOverride);
    }

    #region Helper Methods
    private void CheckGrabStageOverride<T>(EntityUid uid, T component, CheckGrabOverridesEvent args)
        where T : GrabStagesOverrideComponent
    {
        if (args.Stage == GrabStage.Soft)
            args.Stage = component.StartingStage;
    }

    private bool CheckGrant(GrantMartialArtKnowledgeComponent comp, EntityUid user)
    {
        if (!HasComp<CanPerformComboComponent>(user))
            return true;

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

    private void OnShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        var combo = EnsureComp<CanPerformComboComponent>(ent);
        combo.AllowedCombos.Clear();
    }

    private bool CheckCanUseMartialArt(EntityUid uid, MartialArtsForms form)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(uid, out var knowledgeComponent)
            && !knowledgeComponent.Blocked)
        {
            if (knowledgeComponent.MartialArtsForm == form)
                return true;
        }

        foreach (var ent in _lookup.GetEntitiesInRange(uid, 8f))
        {
            if (TryPrototype(ent, out var proto) && proto.ID == "DefaultStationBeaconKitchen")
                return true;
        }

        return false;
    }
    #endregion
}
