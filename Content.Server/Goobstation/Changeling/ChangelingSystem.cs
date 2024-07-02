using Content.Server.DoAfter;
using Content.Server.Forensics;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Server.Zombies;
using Content.Shared.Alert;
using Content.Shared.Changeling;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Store.Components;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;
using Content.Server.Body.Systems;

namespace Content.Server.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAEvent>(OnAbsorb);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);

        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformEvent>(OnTransform);

        SubscribeLocalEvent<ChangelingComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var comp in EntityManager.EntityQuery<ChangelingComponent>())
        {
            var uid = comp.Owner;

            comp.ChemicalRegenerationAccumulator += frameTime;

            if (comp.ChemicalRegenerationAccumulator < comp.ChemicalRegenerationTimer)
                return;

            comp.ChemicalRegenerationAccumulator -= comp.ChemicalRegenerationTimer;

            UpdateChemicals(uid, comp);
            UpdateModifier(comp);
        }
    }

    private void UpdateChemicals(EntityUid uid, ChangelingComponent comp, int? amount = 1)
    {
        var regen = Math.Abs(1 * (1 + comp.ChemicalRegenerationModifier));
        var chemicals = comp.Chemicals + amount ?? regen;

        if (chemicals > comp.MaxChemicals)
            comp.Chemicals = comp.MaxChemicals;

        comp.Chemicals = chemicals;

        Dirty(uid, comp);
    }
    public void UpdateModifier(ChangelingComponent comp)
    {
        var modifier = comp.ChemicalRegenerationMobStateModifier;
        comp.ChemicalRegenerationModifier = modifier;
    }

    public bool TryUseAbility(EntityUid uid, ChangelingComponent comp, int price)
    {
        if (comp.Chemicals < price)
        {
            _popup.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), uid, uid);
            return false;
        }

        UpdateChemicals(uid, comp, -price);

        return true;
    }
    public bool TrySting(EntityUid uid, EntityUid target, ChangelingComponent comp, int price)
    {
        if (!TryUseAbility(uid, comp, price))
            return false;

        if (HasComp<ChangelingComponent>(target))
        {
            var selfMessage = Loc.GetString("changeling-sting-fail-self", ("target", Identity.Entity(target, EntityManager)));
            var targetMessage = Loc.GetString("changeling-sting-fail-ling");

            _popup.PopupEntity(selfMessage, uid, uid);
            _popup.PopupEntity(targetMessage, target, target);
            return false;
        }

        return true;
    }
    public void TryReagentSting(EntityUid uid, EntityUid target, ChangelingComponent comp, int chemicalCost, string reagentId, FixedPoint2 reagentAmount)
    {
        if (!TrySting(uid, target, comp, chemicalCost))
            return;

        var solution = new Solution();
        solution.AddReagent(reagentId, reagentAmount);
    }

    public void AddDNA(ChangelingComponent comp, TransformData data)
    {
        if (comp.AbsorbedDNA.Count >= comp.MaxAbsorbedDNA)
        {
            comp.AbsorbedDNA.RemoveAt(0);
            comp.AbsorbedDNA.Add(data);
            return;
        }
        comp.AbsorbedDNA.Add(data);
    }
    public bool TryStealDNA(EntityUid uid, EntityUid target, ChangelingComponent comp)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var appearance)
        || !TryComp<MetaDataComponent>(target, out var metadata)
        || !TryComp<DnaComponent>(target, out var dna)
        || !TryComp<FingerprintComponent>(target, out var fingerprint))
            return false;

        foreach (var storedDNA in comp.AbsorbedDNA)
        {
            if (storedDNA.DNA != null && storedDNA.DNA == dna.DNA)
                return false;
        }

        var data = new TransformData
        {
            Name = metadata.EntityName,
            DNA = dna.DNA,
            HumanoidAppearanceComp = appearance
        };

        if (fingerprint.Fingerprint != null)
            data.Fingerprint = fingerprint.Fingerprint;

        AddDNA(comp, data);

        return true;
    }

    public void PlayMeatySound(EntityUid uid, ChangelingComponent comp)
    {
        var rand = _rand.Next(0, comp.SoundPool.Count - 1);
        var sound = comp.SoundPool.ToArray()[rand];
        _audio.PlayPvs(sound, uid, AudioParams.Default.WithVolume(-3f));
    }

    public bool IsIncapacitated(EntityUid uid)
    {
        if (_mobState.IsIncapacitated(uid)
        || (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.CuffedHandCount > 0))
            return true;

        return false;
    }


    #region Event Handlers

    private void OnStartup(EntityUid uid, ChangelingComponent comp, ref ComponentStartup args)
    {
        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);
        EnsureComp<ZombieImmuneComponent>(uid);
    }

    private void OnMobStateChange(EntityUid uid, ChangelingComponent comp, ref MobStateChangedEvent args)
    {
        var modifier = 0f;
        switch (args.NewMobState)
        {
            case MobState.Alive: default: modifier = 0; break;
            case MobState.Critical: modifier = -.25f; break;
            case MobState.Dead: modifier = -.5f; break;
        }
        comp.ChemicalRegenerationMobStateModifier = modifier;
    }

    #endregion

    #region Abilities

    private void OnOpenEvolutionMenu(EntityUid uid, ChangelingComponent comp, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnAbsorb(EntityUid uid, ChangelingComponent comp, ref AbsorbDNAEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!IsIncapacitated(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-incapacitated"), uid, uid);
            return;
        }
        if (HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-absorbed"), uid, uid);
            return;
        }
        if (!HasComp<AbsorbableComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), uid, uid);
            return;
        }

        var popupOthers = Loc.GetString("changeling-absorb-start-others", ("user", Identity.Entity(uid, EntityManager)), ("target", Identity.Entity(target, EntityManager)));
        _popup.PopupEntity(popupOthers, uid, PopupType.LargeCaution);
        PlayMeatySound(uid, comp);
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(15), new AbsorbDNADoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    public ProtoId<DamageGroupPrototype> GeneticDamageGroup = "Genetic";
    private void OnAbsorbDoAfter(EntityUid uid, ChangelingComponent comp, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        PlayMeatySound(uid, comp);

        if (args.Cancelled || !IsIncapacitated(target) || HasComp<AbsorbedComponent>(target))
            return;

        var dmg = new DamageSpecifier(_proto.Index(GeneticDamageGroup), 200);
        _damage.TryChangeDamage(target, dmg, true, false);
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        EnsureComp<AbsorbedComponent>(target);

        var popupSelf = string.Empty;
        var bonusChemicals = 10;
        var bonusEvolutionPoints = 0;
        if (HasComp<ChangelingComponent>(target))
        {
            popupSelf = Loc.GetString("changeling-absorb-end-self-ling");
            bonusChemicals += 20;
            bonusEvolutionPoints += 5;
        }
        else
        {
            popupSelf = Loc.GetString("changeling-absorb-end-self", ("target", Identity.Entity(target, EntityManager)));
            bonusChemicals += 10;
            TryStealDNA(uid, target, comp);
        }

        _popup.PopupEntity(popupSelf, uid, uid);
        comp.MaxChemicals += bonusChemicals;

        if (TryComp<StoreComponent>(uid, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "", bonusEvolutionPoints } }, uid, store);
            _store.UpdateUserInterface(uid, uid, store);
        }
    }

    private void OnTransform(EntityUid uid, ChangelingComponent comp, ref ChangelingTransformEvent args)
    {
        PlayMeatySound(uid, comp);
    }

    #endregion
}
