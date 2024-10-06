using Content.Server.Forensics;
using Content.Server.Zombies;
using Content.Shared.Chemistry.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Nutrition.Components;
using Content.Shared.Store.Components;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Shared.Polymorph;
using Content.Server.Polymorph.Components;
using Content.Shared.Fluids;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Timing;
using Content.Shared.Mobs.Components;
using System.Linq;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Server.Jittering;
using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Server.Humanoid;
using Content.Shared.Mind;
using Content.Server.Objectives.Components;
using Content.Server._Goobstation.Objectives.Components;

namespace Content.Server._Goobstation.Changeling;

public sealed partial class ChangelingSystem : SharedChangelingSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly JitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedPuddleSystem _puddleSystem = default!;

    public EntProtoId ArmbladePrototype = "ArmBladeChangeling";
    public EntProtoId FakeArmbladePrototype = "FakeArmBladeChangeling";

    public EntProtoId ShieldPrototype = "ChangelingShield";
    public EntProtoId BoneShardPrototype = "ThrowingStarChangeling";

    public EntProtoId ArmorPrototype = "ChangelingClothingOuterArmor";
    public EntProtoId ArmorHelmetPrototype = "ChangelingClothingHeadHelmet";

    public EntProtoId SpacesuitPrototype = "ChangelingClothingOuterHardsuit";
    public EntProtoId SpacesuitHelmetPrototype = "ChangelingClothingHeadHelmetHardsuit";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChangelingComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<ChangelingComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<ChangelingComponent, ComponentRemove>(OnComponentRemove);
    }

    /// <summary>
    ///     Override from shared to work with bloodstream.
    /// </summary>
    public override void UpdateBiomass(Entity<ChangelingComponent> changeling, float? amount = null)
    {
        base.UpdateBiomass(changeling, amount);
        var comp = changeling.Comp;

        // Make ling constantly jitter if he have 10% or less of biomass
        if (comp.Biomass <= comp.MaxBiomass * 0.1)
        {
            PopupSystem.PopupEntity(Loc.GetString("popup-changeling-biomass-deficit-high"), changeling, changeling, PopupType.LargeCaution);
            _jitteringSystem.DoJitter(changeling, comp.BiomassUpdateCooldown, true, amplitude: 5, frequency: 10);
            return;
        }

        // Make ling vomit his blood or get little jitter on 30% or less biomass
        var lingRandomEffect = _random.Next(1, 5);
        var vomitAmount = _random.Next(1, 20);

        if (comp.Biomass <= comp.MaxBiomass * 0.3)
        {
            // Vomit and slowdown
            if (lingRandomEffect == 1 && _bloodstreamSystem.TryModifyBloodLevel(changeling, -vomitAmount))
            {
                var solution = new Solution();

                solution.AddReagent(comp.ChangelingBloodPrototype, vomitAmount);
                StunSystem.TrySlowdown(changeling, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f);
                _puddleSystem.TrySplashSpillAt(changeling, Transform(changeling).Coordinates, solution, out _);

                PopupSystem.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(changeling, EntityManager))), changeling);
            }

            // Jittering
            if (lingRandomEffect == 5)
            {
                PopupSystem.PopupEntity(Loc.GetString("popup-changeling-biomass-deficit-medium"), changeling, changeling, PopupType.MediumCaution);
                _jitteringSystem.DoJitter(changeling, TimeSpan.FromSeconds(.5f), true, amplitude: 5, frequency: 10);
            }
        }

        // Show caution on half biomass
        if (comp.Biomass == comp.MaxBiomass * 0.5)
        {
            PopupSystem.PopupEntity(Loc.GetString("popup-changeling-biomass-deficit-low"), changeling, changeling, PopupType.MediumCaution);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ChangelingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime > comp.LastChemicalsUpdate)
            {
                comp.LastChemicalsUpdate = _timing.CurTime + comp.ChemicalsUpdateCooldown;
                UpdateChemicals((uid, comp));
            }

            if (!comp.IsEmptyBiomass && _timing.CurTime > comp.LastBiomassUpdate)
            {
                comp.LastBiomassUpdate = _timing.CurTime + comp.BiomassUpdateCooldown;
                UpdateBiomass((uid, comp));
            }
        }
    }

    /// <summary>
    ///     Toggle and places item into ling's hand. Like armblade or meat shield.
    /// </summary>
    public bool TryToggleItem(Entity<ChangelingComponent> changeling, EntProtoId proto, string? clothingSlot = null)
    {
        var comp = changeling.Comp;

        // TODO: I think it's better to keep all equipment in ling's inner container instead of spawn/deleting them each time.
        if (!comp.Equipment.TryGetValue(proto.Id, out var item) && item == null)
        {
            item = Spawn(proto, Transform(changeling).Coordinates);
            if (clothingSlot != null && !_inventorySystem.TryEquip(changeling, (EntityUid) item, clothingSlot, force: true))
            {
                QueueDel(item);
                return false;
            }

            if (!_handsSystem.TryForcePickupAnyHand(changeling, item.Value))
            {
                PopupSystem.PopupEntity(Loc.GetString("changeling-fail-hands"), changeling, changeling);
                QueueDel(item);
                return false;
            }

            comp.Equipment.Add(proto.Id, item);
            return true;
        }

        QueueDel(item);
        // assuming that it exists
        comp.Equipment.Remove(proto.Id);

        return true;
    }

    /*public void Cycle(EntityUid uid, ChangelingComponent comp)
    {
        UpdateChemicals(uid, comp);

        comp.BiomassUpdateTimer += 1;
        if (comp.BiomassUpdateTimer >= comp.BiomassUpdateCooldown)
        {
            comp.BiomassUpdateTimer = 0;
            UpdateBiomass(uid, comp);
        }

        //UpdateAbilities(uid, comp);
    }*/


    /* MOVE THIS TO FAST RUNNING ABILITY
    private void UpdateAbilities(EntityUid uid, ChangelingComponent comp)
    {
        _speed.RefreshMovementSpeedModifiers(uid);
        if (comp.StrainedMusclesActive)
        {
            var stamina = EnsureComp<StaminaComponent>(uid);
            _stamina.TakeStaminaDamage(uid, 7.5f, visual: false);
            if (stamina.StaminaDamage >= stamina.CritThreshold || _gravity.IsWeightless(uid))
                ToggleStrainedMuscles(uid, comp);
        }
    }*/

    #region Helper Methods

    /// <summary>
    ///     Used to soft change humanoid appearance and dna data of entity by using HumanoidTransformData.
    /// </summary>
    public void ChangeHumanoidData(EntityUid target, HumanoidTransformData data)
    {
        var dataEntity = GetEntity(data.AppearanceEntity);

        if (TryComp<DnaComponent>(target, out var targetDna))
            targetDna.DNA = data.DNA;

        if (TryComp<FingerprintComponent>(target, out var targetFingerprints) && !string.IsNullOrEmpty(data.Fingerprint))
            targetFingerprints.Fingerprint = data.Fingerprint;

        if (TryComp<HumanoidAppearanceComponent>(target, out var targetAppearance) && TryComp<HumanoidAppearanceComponent>(dataEntity, out var dataAppearance))
            _humanoidAppearance.CloneAppearance(dataEntity, target, dataAppearance, targetAppearance);

        _metadata.SetEntityName(target, data.Name);
    }

    /// <summary>
    ///     Tries to steal humanoid data including DNA and Fingerprints
    /// </summary>
    public bool TryStealHumanoidData(Entity<ChangelingComponent> changeling, EntityUid target, bool countObjective = false)
    {
        var comp = changeling.Comp;

        if (comp.AbsorbedDNA.Count >= comp.MaxAbsorbedDNA)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-sting-extract-max"), changeling, changeling);
            return false;
        }

        // DNA and Appearance is main thing of lings
        if (!TryComp<DnaComponent>(target, out var targetDna) || !HasComp<HumanoidAppearanceComponent>(target))
            return false;

        foreach (var storedDNA in comp.AbsorbedDNA)
        {
            if (storedDNA.DNA == targetDna.DNA)
                return false;
        }

        var data = new HumanoidTransformData
        {
            Name = MetaData(target).EntityName,
            DNA = targetDna.DNA,
            AppearanceEntity = GetNetEntity(target)
        };

        if (TryComp<FingerprintComponent>(target, out var targetFingerprint) && !string.IsNullOrEmpty(targetFingerprint.Fingerprint))
            data.Fingerprint = targetFingerprint.Fingerprint;

        comp.AbsorbedDNA.Add(data);
        comp.TotalStolenDNA++;
        Dirty(changeling);

        if (countObjective
        && _mindSystem.TryGetMind(changeling, out var mindId, out var mind)
        && _mindSystem.TryGetObjectiveComp<StealDNAConditionComponent>(mindId, out var objective, mind))
        {
            objective.DNAStolen += 1;
        }

        return true;
    }

    

    private EntityUid? TransformEntity(
        EntityUid uid, 
        TransformData? data = null, 
        EntProtoId? protoId = null, 
        ChangelingComponent? comp = null, 
        bool dropInventory = false, 
        bool transferDamage = true,
        bool persistentDna = false)
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
            TransferDamage = transferDamage,
            Forced = true,
            Inventory = (dropInventory) ? PolymorphInventoryChange.Drop : PolymorphInventoryChange.Transfer,
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
        if (HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-absorbed"), target, target);
            return false;
        }

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
        else 
        {
            comp.IsInLesserForm = false;
            newUid = TransformEntity(target, data: data, comp: comp, persistentDna: persistentDna);
        }

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
        // check if there's no entities or all entities are null
        if (comp.Equipment.Values.Count == 0
        || comp.Equipment.Values.All(ent => ent == null ? true : false))
            return;

        foreach (var equip in comp.Equipment.Values)
            QueueDel(equip);

        PlayMeatySound(target, comp);
    }

    #endregion

    #region Event Handlers

    private void OnStartup(EntityUid uid, ChangelingComponent comp, ref ComponentStartup args)
    {
        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);
        EnsureComp<ZombieImmuneComponent>(uid);

        // add actions
        foreach (var actionId in comp.BaseChangelingActions)
            _actions.AddAction(uid, actionId);

        // making sure things are right in this world
        comp.Chemicals = comp.MaxChemicals;
        comp.Biomass = comp.MaxBiomass;

        // show alerts
        UpdateChemicals(uid, comp, 0);
        UpdateBiomass(uid, comp, 0);
        // make their blood unreal
        _blood.ChangeBloodReagent(uid, "BloodChangeling");
    }

    private void OnMobStateChange(EntityUid uid, ChangelingComponent comp, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemoveAllChangelingEquipment(uid, comp);
    }

    private void OnDamageChange(Entity<ChangelingComponent> ent, ref DamageChangedEvent args)
    {
        var target = args.Damageable;

        if (!TryComp<MobStateComponent>(ent, out var mobState))
            return;

        if (mobState.CurrentState != MobState.Dead)
            return;

        if (!args.DamageIncreased)
            return;
        
        target.Damage.ClampMax(200); // we never die. UNLESS??
    }

    private void OnComponentRemove(Entity<ChangelingComponent> ent, ref ComponentRemove args)
    {
        RemoveAllChangelingEquipment(ent, ent.Comp);
    }

    #endregion
}
