using Content.Server.DoAfter;
using Content.Server.Forensics;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Server.Zombies;
using Content.Shared.Alert;
using Content.Shared.Changeling;
using Content.Shared.Changeling.Components;
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
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.Serialization.Manager;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.StatusEffect;
using Content.Server.Flash.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Cuffs;
using Content.Shared.Fluids;
using Content.Shared.Stealth.Components;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared.Camera;
using Robust.Shared.Timing;
using Content.Shared.Damage.Components;
using Content.Server.Gravity;
using Content.Server.Stunnable;

namespace Content.Server.Changeling.EntitySystems;

public sealed partial class ChangelingSystem : EntitySystem
{
    // this is one hell of a star wars intro text
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;

    [Dependency] private readonly BlindableSystem _blindable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;

    [Dependency] private readonly PullingSystem _pull = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffs = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChangelingComponent, MobStateChangedEvent>(OnMobStateChange);

        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAEvent>(OnAbsorb);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);
        SubscribeLocalEvent<ChangelingComponent, StingExtractDNAEvent>(OnStingExtractDNA);
        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformCycleEvent>(OnTransformCycle);
        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformEvent>(OnTransform);
        SubscribeLocalEvent<ChangelingComponent, EnterStasisEvent>(OnEnterStasis);
        SubscribeLocalEvent<ChangelingComponent, ExitStasisEvent>(OnExitStasis);

        SubscribeLocalEvent<ChangelingComponent, ToggleArmbladeEvent>(OnToggleArmblade);
        SubscribeLocalEvent<ChangelingComponent, CreateBoneShardEvent>(OnCreateBoneShard);
        SubscribeLocalEvent<ChangelingComponent, ToggleChitinousArmorEvent>(OnToggleArmor);
        SubscribeLocalEvent<ChangelingComponent, ToggleOrganicShieldEvent>(OnToggleShield);
        SubscribeLocalEvent<ChangelingComponent, ShriekDissonantEvent>(OnShriekDissonant);
        SubscribeLocalEvent<ChangelingComponent, ShriekResonantEvent>(OnShriekResonant);
        SubscribeLocalEvent<ChangelingComponent, ToggleStrainedMusclesEvent>(OnToggleStrainedMuscles);

        SubscribeLocalEvent<ChangelingComponent, StingBlindEvent>(OnStingBlind);
        SubscribeLocalEvent<ChangelingComponent, StingCryoEvent>(OnStingCryo);
        SubscribeLocalEvent<ChangelingComponent, StingLethargicEvent>(OnStingLethargic);
        SubscribeLocalEvent<ChangelingComponent, StingMuteEvent>(OnStingMute);
        SubscribeLocalEvent<ChangelingComponent, StingTransformEvent>(OnStingTransform);
        SubscribeLocalEvent<ChangelingComponent, StingFakeArmbladeEvent>(OnStingFakeArmblade);

        SubscribeLocalEvent<ChangelingComponent, ActionAnatomicPanaceaEvent>(OnAnatomicPanacea);
        SubscribeLocalEvent<ChangelingComponent, ActionAugmentedEyesightEvent>(OnAugmentedEyesight);
        SubscribeLocalEvent<ChangelingComponent, ActionBiodegradeEvent>(OnBiodegrade);
        SubscribeLocalEvent<ChangelingComponent, ActionChameleonSkinEvent>(OnChameleonSkin);
        SubscribeLocalEvent<ChangelingComponent, ActionEphedrineOverdoseEvent>(OnEphedrineOverdose);
        SubscribeLocalEvent<ChangelingComponent, ActionFleshmendEvent>(OnHealUltraSwag);
        SubscribeLocalEvent<ChangelingComponent, ActionLastResortEvent>(OnLastResort);
        SubscribeLocalEvent<ChangelingComponent, ActionLesserFormEvent>(OnLesserForm);
        SubscribeLocalEvent<ChangelingComponent, ActionSpacesuitEvent>(OnSpacesuit);
        SubscribeLocalEvent<ChangelingComponent, ActionHivemindAccessEvent>(OnHivemindAccess);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityManager.EntityQuery<ChangelingComponent>())
        {
            var uid = comp.Owner;

            if (_timing.CurTime < comp.RegenTime)
                continue;

            comp.RegenTime = _timing.CurTime + TimeSpan.FromSeconds(comp.RegenCooldown);

            Cycle(uid, comp);
        }
    }
    public void Cycle(EntityUid uid, ChangelingComponent comp)
    {
        UpdateChemicals(uid, comp);

        if (comp.StrainedMusclesActive)
        {
            var stamina = EnsureComp<StaminaComponent>(uid);
            _stamina.TakeStaminaDamage(uid, 7.5f, visual: false);
            if (stamina.StaminaDamage >= stamina.CritThreshold || _gravity.IsWeightless(uid))
                ToggleStrainedMuscles(uid, comp);
        }
    }

    #region Helper Methods

    public void DoScreech(EntityUid uid, ChangelingComponent comp)
    {
        _audio.PlayPvs(comp.ShriekSound, uid);

        var center = Transform(uid).MapPosition;
        var gamers = Filter.Empty();
        gamers.AddInRange(center, comp.ShriekPower, _player, EntityManager);

        foreach (var gamer in gamers.Recipients)
        {
            if (gamer.AttachedEntity == null)
                continue;

            var pos = Transform(gamer.AttachedEntity!.Value).WorldPosition;
            var delta = center.Position - pos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(.01f, 0);

            _recoil.KickCamera(uid, -delta.Normalized());
        }
    }

    /// <summary>
    ///     Check if a target is crit/dead or cuffed. For absorbing.
    /// </summary>
    public bool IsIncapacitated(EntityUid uid)
    {
        if (_mobState.IsIncapacitated(uid)
        || TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.CuffedHandCount > 0)
            return true;

        return false;
    }

    private void UpdateChemicals(EntityUid uid, ChangelingComponent comp, float? amount = null)
    {
        var chemicals = comp.Chemicals;

        chemicals += amount ?? 1 /*regen*/;

        comp.Chemicals = Math.Clamp(chemicals, 0, comp.MaxChemicals);

        Dirty(uid, comp);

        _alerts.ShowAlert(uid, "Chemicals");
    }

    public void AddDNA(EntityUid uid, ChangelingComponent comp, TransformData data, bool countObjective = false)
    {
        if (comp.AbsorbedDNA.Count >= comp.MaxAbsorbedDNA)
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-max"), uid, uid);
            return;
        }
        comp.AbsorbedDNA.Add(data);

        if (countObjective)
        {
            if (_mind.TryGetMind(uid, out var mindId, out var mind))
                if (_mind.TryGetObjectiveComp<StealDNAConditionComponent>(mindId, out var objective, mind))
                    objective.DNAStolen += 1;
        }
    }
    public bool TryStealDNA(EntityUid uid, EntityUid target, ChangelingComponent comp, bool countObjective = false)
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
            Appearance = appearance
        };

        if (fingerprint.Fingerprint != null)
            data.Fingerprint = fingerprint.Fingerprint;

        AddDNA(uid, comp, data, countObjective);

        return true;
    }

    private ChangelingComponent? CopyChangelingComponent(EntityUid target, ChangelingComponent comp)
    {
        var newComp = EnsureComp<ChangelingComponent>(target);
        newComp.AbsorbedDNA = comp.AbsorbedDNA;
        newComp.AbsorbedDNAIndex = comp.AbsorbedDNAIndex;
        newComp.Chemicals = comp.Chemicals;

        newComp.IsInLesserForm = comp.IsInLesserForm;
        newComp.CurrentForm = comp.CurrentForm;

        newComp.TotalAbsorbedEntities = comp.TotalAbsorbedEntities;
        newComp.TotalStolenDNA = comp.TotalStolenDNA;

        return comp;
    }
    private EntityUid? TransformEntity(EntityUid uid, TransformData? data = null, EntProtoId? protoId = null, ChangelingComponent? comp = null, bool persistentDna = false)
    {
        EntProtoId? pid = null;

        if (data != null)
        {
            if (!_proto.TryIndex(data.Appearance.Species, out var species))
                return null;
            pid = species.Prototype;
        }
        else if (protoId != null)
            pid = protoId;
        else return null;

        var config = new PolymorphConfiguration()
        {
            Entity = (EntProtoId) pid,
            TransferDamage = true,
            Forced = true,
            Inventory = PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false
        };
        var newUid = _polymorph.PolymorphEntity(uid, config);

        if (newUid == null)
            return null;

        var newEnt = newUid.Value;

        if (data != null)
        {
            Comp<FingerprintComponent>(newEnt).Fingerprint = data.Fingerprint;
            Comp<DnaComponent>(newEnt).DNA = data.DNA;
            _humanoid.CloneAppearance(data.Appearance.Owner, newEnt);
            _metaData.SetEntityName(newEnt, data.Name);
            var message = Loc.GetString("changeling-transform-finish", ("target", data.Name));
            _popup.PopupEntity(message, newEnt, newEnt);
        }

        RemCompDeferred<PolymorphedEntityComponent>(newEnt);

        if (comp != null)
        {
            // copy our stuff
            var newLingComp = CopyChangelingComponent(newEnt, comp);
            if (!persistentDna && data != null)
                newLingComp?.AbsorbedDNA.Remove(data);
            RemCompDeferred<ChangelingComponent>(uid);

            if (TryComp<StoreComponent>(uid, out var storeComp))
            {
                var storeCompCopy = _serialization.CreateCopy(storeComp, notNullableOverride: true);
                RemComp<StoreComponent>(newUid.Value);
                EntityManager.AddComponent(newUid.Value, storeCompCopy);
            }
        }

        // exceptional comps check
        // there's no foreach for types i believe so i gotta thug it out yandev style.
        if (HasComp<HeadRevolutionaryComponent>(uid))
            EnsureComp<HeadRevolutionaryComponent>(newEnt);
        if (HasComp<RevolutionaryComponent>(uid))
            EnsureComp<RevolutionaryComponent>(newEnt);

        QueueDel(uid);

        return newUid;
    }
    public bool TryTransform(EntityUid target, ChangelingComponent comp, bool sting = false, bool persistentDna = false)
    {
        var data = comp.SelectedForm;

        if (data == null)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-self"), target, target);
            return false;
        }
        if (data == comp.CurrentForm)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-choose"), target, target);
            return false;
        }

        var locName = Identity.Entity(target, EntityManager);
        EntityUid? newUid = null;
        if (sting)
            newUid = TransformEntity(target, data: data, persistentDna: persistentDna);
        else newUid = TransformEntity(target, data: data, comp: comp, persistentDna: persistentDna);

        if (newUid != null)
        {
            PlayMeatySound((EntityUid) newUid, comp);
            var loc = Loc.GetString("changeling-transform-others", ("user", locName));
            _popup.PopupEntity(loc, (EntityUid) newUid, PopupType.LargeCaution);
        }

        return true;
    }

    public void RemoveAllChangelingEquipment(EntityUid target, ChangelingComponent comp)
    {
        if (comp.Equipment.Values.Count == 0)
            return;

        foreach (var equip in comp.Equipment.Values)
        {
            if (equip == null || equip.Entity == null)
                continue;

            QueueDel(equip.Entity);
            if (equip.Prototype != null)
                comp.Equipment.Remove((EntProtoId) equip.Prototype);
        }

        PlayMeatySound(target, comp);
    }

    #endregion

    #region Event Handlers

    private void OnStartup(Entity<ChangelingComponent> ent, ref ComponentStartup args)
    {
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        EnsureComp<ZombieImmuneComponent>(ent);

        // add actions
        foreach (var actionId in ent.Comp.BaseChangelingActions)
            _actions.AddAction(ent, actionId);

        // check for equipment
        if (ent.Comp.Equipment.Count > 0)
        {
            foreach (var equip in ent.Comp.Equipment.Values)
            {
                if (equip != null)
                    TryToggleItem(ent, ent.Comp, equip);
            }
        }
    }

    /// <summary>
    ///     Removes all equipment in case of death. This is done to prevent equipment from dropping on getting gibbed.
    /// </summary>
    private void OnMobStateChange(Entity<ChangelingComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemoveAllChangelingEquipment(ent, ent.Comp);
    }

    #endregion

    #region Basic Abilities

    private void OnOpenEvolutionMenu(Entity<ChangelingComponent> ent, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
    }

    private void OnAbsorb(Entity<ChangelingComponent> ent, ref AbsorbDNAEvent args)
    {
        var target = args.Target;

        if (!IsIncapacitated(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-incapacitated"), ent, ent);
            return;
        }
        if (HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-absorbed"), ent, ent);
            return;
        }
        if (!HasComp<AbsorbableComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), ent, ent);
            return;
        }

        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        var popupOthers = Loc.GetString("changeling-absorb-start", ("user", Identity.Entity(ent, EntityManager)), ("target", Identity.Entity(target, EntityManager)));
        _popup.PopupEntity(popupOthers, ent, PopupType.LargeCaution);
        PlayMeatySound(ent, ent.Comp);
        var dargs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(15), new AbsorbDNADoAfterEvent(), ent, target)
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
    public ProtoId<DamageGroupPrototype> AbsorbedDamageGroup = "Genetic";
    private void OnAbsorbDoAfter(Entity<ChangelingComponent> ent, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        PlayMeatySound(args.User, ent.Comp);

        if (args.Cancelled || !IsIncapacitated(target) || HasComp<AbsorbedComponent>(target))
            return;

        var dmg = new DamageSpecifier(_proto.Index(AbsorbedDamageGroup), 200);
        _damage.TryChangeDamage(target, dmg, false, false);
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        EnsureComp<AbsorbedComponent>(target);

        var popup = Loc.GetString("changeling-absorb-end-self-ling");
        var bonusChemicals = 0;
        var bonusEvolutionPoints = 0;
        if (HasComp<ChangelingComponent>(target))
        {
            bonusChemicals += 60;
            bonusEvolutionPoints += 10;
        }
        else
        {
            popup = Loc.GetString("changeling-absorb-end-self", ("target", Identity.Entity(target, EntityManager)));
            bonusChemicals += 10;
            bonusEvolutionPoints += 2;
        }
        TryStealDNA(ent, target, ent.Comp, true);
        ent.Comp.TotalAbsorbedEntities++;
        ent.Comp.TotalStolenDNA++;

        _popup.PopupEntity(popup, args.User, args.User);
        ent.Comp.MaxChemicals += bonusChemicals;

        if (TryComp<StoreComponent>(args.User, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "EvolutionPoint", bonusEvolutionPoints } }, args.User, store);
            _store.UpdateUserInterface(args.User, args.User, store);
        }

        if (_mind.TryGetMind(ent, out var mindId, out var mind))
            if (_mind.TryGetObjectiveComp<AbsorbConditionComponent>(mindId, out var objective, mind))
                objective.Absorbed += 1;
    }

    private void OnStingExtractDNA(Entity<ChangelingComponent> ent, ref StingExtractDNAEvent args)
    {
        if (!TrySting(ent, ent.Comp, args, true))
            return;

        var target = args.Target;
        if (!TryStealDNA(ent, target, ent.Comp, true))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-fail"), ent, ent);
            // royal cashback
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
        }
        else _popup.PopupEntity(Loc.GetString("changeling-sting", ("target", Identity.Entity(target, EntityManager))), ent, ent);
    }

    private void OnTransformCycle(Entity<ChangelingComponent> ent, ref ChangelingTransformCycleEvent args)
    {
        ent.Comp.AbsorbedDNAIndex += 1;
        if (ent.Comp.AbsorbedDNAIndex >= ent.Comp.MaxAbsorbedDNA || ent.Comp.AbsorbedDNAIndex >= ent.Comp.AbsorbedDNA.Count)
            ent.Comp.AbsorbedDNAIndex = 0;

        if (ent.Comp.AbsorbedDNA.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-cycle-empty"), ent, ent);
            return;
        }

        var selected = ent.Comp.AbsorbedDNA.ToArray()[ent.Comp.AbsorbedDNAIndex];
        ent.Comp.SelectedForm = selected;
        _popup.PopupEntity(Loc.GetString("changeling-transform-cycle", ("target", selected.Name)), ent, ent);
    }
    private void OnTransform(Entity<ChangelingComponent> ent, ref ChangelingTransformEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (!TryTransform(ent, ent.Comp))
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
    }

    private void OnEnterStasis(Entity<ChangelingComponent> ent, ref EnterStasisEvent args)
    {
        if (ent.Comp.IsInStasis || HasComp<AbsorbedComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter-fail"), ent, ent);
            return;
        }

        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        ent.Comp.Chemicals = 0f;

        if (_mobState.IsAlive(ent))
        {
            // fake our death
            var othersMessage = Loc.GetString("suicide-command-default-text-others", ("name", ent));
            _popup.PopupEntity(othersMessage, ent, Robust.Shared.Player.Filter.PvsExcept(ent), true);

            var selfMessage = Loc.GetString("changeling-stasis-enter");
            _popup.PopupEntity(selfMessage, ent, ent);
        }

        if (!_mobState.IsDead(ent))
            _mobState.ChangeMobState(ent, MobState.Dead);

        ent.Comp.IsInStasis = true;
    }
    private void OnExitStasis(Entity<ChangelingComponent> ent, ref ExitStasisEvent args)
    {
        if (!ent.Comp.IsInStasis)
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-fail"), ent, ent);
            return;
        }
        if (HasComp<AbsorbedComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-fail-dead"), ent, ent);
            return;
        }

        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return;

        // heal of everything
        _damage.SetAllDamage(ent, damageable, 0);
        _mobState.ChangeMobState(ent, MobState.Alive);
        _blood.TryModifyBloodLevel(ent, 1000);
        _blood.TryModifyBleedAmount(ent, -1000);

        _popup.PopupEntity(Loc.GetString("changeling-stasis-exit"), ent, ent);

        ent.Comp.IsInStasis = false;
    }

    #endregion

    #region Combat Abilities

    private void OnToggleArmblade(Entity<ChangelingComponent> ent, ref ToggleArmbladeEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (!TryToggleItem(ent, ArmbladePrototype, ent.Comp))
            return;

        PlayMeatySound(ent, ent.Comp);
    }
    private void OnCreateBoneShard(Entity<ChangelingComponent> ent, ref CreateBoneShardEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        var star = Spawn(BoneShardPrototype, Transform(ent).Coordinates);
        _hands.TryPickupAnyHand(ent, star);

        PlayMeatySound(ent, ent.Comp);
    }
    private void OnToggleArmor(Entity<ChangelingComponent> ent, ref ToggleChitinousArmorEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (!TryToggleItem(ent, ArmorPrototype, ent.Comp, "outerClothing")
        || !TryToggleItem(ent, ArmorHelmetPrototype, ent.Comp, "head"))
        {
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        PlayMeatySound(ent, ent.Comp);
    }
    private void OnToggleShield(Entity<ChangelingComponent> ent, ref ToggleOrganicShieldEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (!TryToggleItem(ent, ShieldPrototype, ent.Comp))
            return;

        PlayMeatySound(ent, ent.Comp);
    }
    private void OnShriekDissonant(Entity<ChangelingComponent> ent, ref ShriekDissonantEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        DoScreech(ent, ent.Comp);

        var pos = _transform.GetMapCoordinates(ent);
        var power = ent.Comp.ShriekPower;
        _emp.EmpPulse(pos, power, 5000f, power * 2);
    }
    private void OnShriekResonant(Entity<ChangelingComponent> ent, ref ShriekResonantEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        DoScreech(ent, ent.Comp);

        var power = ent.Comp.ShriekPower;

        // kill lights and stun people
        var lookup = _lookup.GetEntitiesInRange(ent, power);
        var lights = GetEntityQuery<PoweredLightComponent>();
        var people = GetEntityQuery<StatusEffectsComponent>();

        foreach (var target in lookup)
        {
            if (people.HasComp(target))
            {
                _stun.TryParalyze(target, TimeSpan.FromSeconds(power / 1.5f), true);
                _stun.TrySlowdown(target, TimeSpan.FromSeconds(power * 2f), true, 0.8f, 0.8f);
            }

            if (lights.HasComponent(target))
                _light.TryDestroyBulb(target);
        }
    }
    private void OnToggleStrainedMuscles(Entity<ChangelingComponent> ent, ref ToggleStrainedMusclesEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        ToggleStrainedMuscles(ent, ent.Comp);
    }
    private void ToggleStrainedMuscles(EntityUid uid, ChangelingComponent comp)
    {
        if (!comp.StrainedMusclesActive)
        {
            _speed.ChangeBaseSpeed(uid, 125f, 150f, 1f);
            _popup.PopupEntity(Loc.GetString("changeling-muscles-start"), uid, uid);
            comp.StrainedMusclesActive = true;
        }
        else
        {
            _speed.ChangeBaseSpeed(uid, 100f, 100f, 1f);
            _popup.PopupEntity(Loc.GetString("changeling-muscles-end"), uid, uid);
            comp.StrainedMusclesActive = false;
        }

        PlayMeatySound(uid, comp);
    }

    #endregion

    #region Stings

    private void OnStingBlind(Entity<ChangelingComponent> ent, ref StingBlindEvent args)
    {
        if (!TrySting(ent, ent.Comp, args))
            return;

        var target = args.Target;
        if (!TryComp<BlindableComponent>(target, out var blindable) || blindable.IsBlind)
            return;

        _blindable.AdjustEyeDamage((target, blindable), 2);
        var timeSpan = TimeSpan.FromSeconds(5f);
        _statusEffect.TryAddStatusEffect(target, TemporaryBlindnessSystem.BlindingStatusEffect, timeSpan, false, TemporaryBlindnessSystem.BlindingStatusEffect);
    }
    private void OnStingCryo(Entity<ChangelingComponent> ent, ref StingCryoEvent args)
    {
        var reagents = new List<(string, FixedPoint2)>()
        {
            ("Fresium", 20f),
            ("ChloralHydrate", 10f)
        };

        if (!TryReagentSting(ent, ent.Comp, args, reagents))
            return;
    }
    private void OnStingLethargic(Entity<ChangelingComponent> ent, ref StingLethargicEvent args)
    {
        var reagents = new List<(string, FixedPoint2)>()
        {
            ("Impedrezene", 10f),
            ("MuteToxin", 5f)
        };

        if (!TryReagentSting(ent, ent.Comp, args, reagents))
            return;
    }
    private void OnStingMute(Entity<ChangelingComponent> ent, ref StingMuteEvent args)
    {
        var reagents = new List<(string, FixedPoint2)>()
        {
            ("MuteToxin", 15f)
        };

        if (!TryReagentSting(ent, ent.Comp, args, reagents))
            return;
    }
    private void OnStingTransform(Entity<ChangelingComponent> ent, ref StingTransformEvent args)
    {
        if (!TrySting(ent, ent.Comp, args, true))
            return;

        var target = args.Target;
        if (!TryTransform(target, ent.Comp, true, true))
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
    }
    private void OnStingFakeArmblade(Entity<ChangelingComponent> ent, ref StingFakeArmbladeEvent args)
    {
        if (!TrySting(ent, ent.Comp, args))
            return;

        var target = args.Target;
        var fakeArmblade = EntityManager.SpawnEntity(FakeArmbladePrototype, Transform(target).Coordinates);
        if (!_hands.TryPickupAnyHand(target, fakeArmblade))
        {
            QueueDel(fakeArmblade);
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-simplemob"), ent, ent);
            return;
        }

        PlayMeatySound(target, ent.Comp);
    }

    #endregion

    #region Utilities

    public void OnAnatomicPanacea(Entity<ChangelingComponent> ent, ref ActionAnatomicPanaceaEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        var reagents = new List<(string, FixedPoint2)>()
        {
            ("Diphenhydramine", 5f),
            ("Arithrazine", 5f),
            ("Ethylredoxrazine", 5f)
        };
        if (TryInjectReagents(ent, reagents))
            _popup.PopupEntity(Loc.GetString("changeling-panacea"), ent, ent);
        else return;
        PlayMeatySound(ent, ent.Comp);
    }
    public void OnAugmentedEyesight(Entity<ChangelingComponent> ent, ref ActionAugmentedEyesightEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (HasComp<FlashImmunityComponent>(ent))
        {
            RemComp<FlashImmunityComponent>(ent);
            _popup.PopupEntity(Loc.GetString("changeling-passive-deactivate"), ent, ent);
            return;
        }

        EnsureComp<FlashImmunityComponent>(ent);
        _popup.PopupEntity(Loc.GetString("changeling-passive-activate"), ent, ent);
    }
    public void OnBiodegrade(Entity<ChangelingComponent> ent, ref ActionBiodegradeEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (TryComp<CuffableComponent>(ent, out var cuffs) && cuffs.Container.ContainedEntities.Count > 0)
        {
            var cuff = cuffs.LastAddedCuffs;

            _cuffs.Uncuff(ent, cuffs.LastAddedCuffs, cuff);
            QueueDel(cuff);
        }

        var soln = new Solution();
        soln.AddReagent("PolytrinicAcid", 10f);

        if (_pull.IsPulled(ent))
        {
            var puller = Comp<PullableComponent>(ent).Puller;
            if (puller != null)
            {
                _puddle.TrySplashSpillAt((EntityUid) puller, Transform((EntityUid) puller).Coordinates, soln, out _);
                return;
            }
        }
        _puddle.TrySplashSpillAt(ent, Transform(ent).Coordinates, soln, out _);
    }
    public void OnChameleonSkin(Entity<ChangelingComponent> ent, ref ActionChameleonSkinEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (HasComp<StealthComponent>(ent) && HasComp<StealthOnMoveComponent>(ent))
        {
            RemComp<StealthComponent>(ent);
            RemComp<StealthOnMoveComponent>(ent);
            _popup.PopupEntity(Loc.GetString("changeling-chameleon-end"), ent, ent);
            return;
        }

        EnsureComp<StealthComponent>(ent);
        EnsureComp<StealthOnMoveComponent>(ent);
        _popup.PopupEntity(Loc.GetString("changeling-chameleon-start"), ent, ent);
    }
    public void OnEphedrineOverdose(Entity<ChangelingComponent> ent, ref ActionEphedrineOverdoseEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        var stam = EnsureComp<StaminaComponent>(ent);
        stam.StaminaDamage = 0;

        var reagents = new List<(string, FixedPoint2)>()
        {
            ("Synaptizine", 5f)
        };
        if (TryInjectReagents(ent, reagents))
            _popup.PopupEntity(Loc.GetString("changeling-inject"), ent, ent);
        else
        {
            _popup.PopupEntity(Loc.GetString("changeling-inject-fail"), ent, ent);
            return;
        }
    }
    // john space made me do this
    public void OnHealUltraSwag(Entity<ChangelingComponent> ent, ref ActionFleshmendEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        var reagents = new List<(string, FixedPoint2)>()
        {
            ("Ichor", 10f),
            ("TranexamicAcid", 5f)
        };
        if (TryInjectReagents(ent, reagents))
            _popup.PopupEntity(Loc.GetString("changeling-fleshmend"), ent, ent);
        else return;
        PlayMeatySound(ent, ent.Comp);
    }
    public void OnLastResort(Entity<ChangelingComponent> ent, ref ActionLastResortEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        // todo: implement
    }
    public void OnLesserForm(Entity<ChangelingComponent> ent, ref ActionLesserFormEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        var newUid = TransformEntity(ent, protoId: "MobMonkey", comp: ent.Comp);
        if (newUid == null)
        {
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        PlayMeatySound((EntityUid) newUid, ent.Comp);
        var loc = Loc.GetString("changeling-transform-others", ("user", Identity.Entity((EntityUid) newUid, EntityManager)));
        _popup.PopupEntity(loc, (EntityUid) newUid, PopupType.LargeCaution);

        ent.Comp.IsInLesserForm = true;
    }
    public void OnSpacesuit(Entity<ChangelingComponent> ent, ref ActionSpacesuitEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (!TryToggleItem(ent, SpacesuitPrototype, ent.Comp, "outerClothing")
        || !TryToggleItem(ent, SpacesuitHelmetPrototype, ent.Comp, "head"))
        {
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        PlayMeatySound(ent, ent.Comp);
    }
    public void OnHivemindAccess(Entity<ChangelingComponent> ent, ref ActionHivemindAccessEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (HasComp<HivemindComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("changeling-passive-active"), ent, ent);
            return;
        }

        EnsureComp<HivemindComponent>(ent);
        _popup.PopupEntity(Loc.GetString("changeling-hivemind-start"), ent, ent);
    }

    #endregion
}
