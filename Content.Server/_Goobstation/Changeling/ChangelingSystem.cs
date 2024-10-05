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
using Content.Server.Body.Systems;
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.Serialization.Manager;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Flash;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Fluids;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared.Camera;
using Robust.Shared.Timing;
using Content.Shared.Damage.Components;
using Content.Server.Gravity;
using Content.Shared.Mobs.Components;
using Content.Server.Stunnable;
using Content.Shared.Jittering;
using Content.Server.Explosion.EntitySystems;
using System.Linq;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling;
using Content.Server.Jittering;

namespace Content.Server.Changeling;

public sealed partial class ChangelingSystem : SharedChangelingSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly JitteringSystem _jitteringSystem = default!;
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

        SubscribeAbilities();
    }

    /// <summary>
    ///     Override from shared to work with bloodstream.
    /// </summary>
    protected override void UpdateBiomass(Entity<ChangelingComponent> changeling, float? amount = null)
    {
        base.UpdateBiomass(changeling, amount);
        var comp = changeling.Comp;

        // Make ling constantly jitter if he have 10% or less of biomass
        if (comp.Biomass <= comp.MaxBiomass * 0.1)
        {
            PopupSystem.PopupEntity(Loc.GetString("popup-changeling-biomass-deficit-high"), changeling, changeling, PopupType.LargeCaution);
            _jitteringSystem.DoJitter(changeling, TimeSpan.FromSeconds(comp.BiomassUpdateCooldown), true, amplitude: 5, frequency: 10);
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

        foreach (var comp in EntityManager.EntityQuery<ChangelingComponent>())
        {
            var uid = comp.Owner;

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateCooldown);

            Cycle(uid, comp);
        }
    }
    public void Cycle(EntityUid uid, ChangelingComponent comp)
    {
        UpdateChemicals(uid, comp);

        comp.BiomassUpdateTimer += 1;
        if (comp.BiomassUpdateTimer >= comp.BiomassUpdateCooldown)
        {
            comp.BiomassUpdateTimer = 0;
            UpdateBiomass(uid, comp);
        }

        UpdateAbilities(uid, comp);
    }


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

    public bool TrySting(EntityUid uid, ChangelingComponent comp, EntityTargetActionEvent action, bool overrideMessage = false)
    {
        if (!TryUseAbility(uid, comp, action))
            return false;

        var target = action.Target;

        // can't get his dna if he doesn't have it!
        if (!HasComp<AbsorbableComponent>(target) || HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-fail"), uid, uid);
            return false;
        }

        if (HasComp<ChangelingComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-self", ("target", Identity.Entity(target, EntityManager))), uid, uid);
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-ling"), target, target);
            return false;
        }
        if (!overrideMessage)
            _popup.PopupEntity(Loc.GetString("changeling-sting", ("target", Identity.Entity(target, EntityManager))), uid, uid);
        return true;
    }
    public bool TryInjectReagents(EntityUid uid, List<(string, FixedPoint2)> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Item1, reagent.Item2);

        if (!_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            return false;

        if (!_solution.TryAddSolution(targetSolution.Value, solution))
            return false;

        return true;
    }
    public bool TryReagentSting(EntityUid uid, ChangelingComponent comp, EntityTargetActionEvent action, List<(string, FixedPoint2)> reagents)
    {
        var target = action.Target;
        if (!TrySting(uid, comp, action))
            return false;

        if (!TryInjectReagents(target, reagents))
            return false;

        return true;
    }
    public bool TryToggleItem(EntityUid uid, EntProtoId proto, ChangelingComponent comp, string? clothingSlot = null)
    {
        if (!comp.Equipment.TryGetValue(proto.Id, out var item) && item == null)
        {
            item = Spawn(proto, Transform(uid).Coordinates);
            if (clothingSlot != null && !_inventory.TryEquip(uid, (EntityUid) item, clothingSlot, force: true))
            {
                QueueDel(item);
                return false;
            }
            else if (!_hands.TryForcePickupAnyHand(uid, (EntityUid) item))
            {
                _popup.PopupEntity(Loc.GetString("changeling-fail-hands"), uid, uid);
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

        if (comp.AbsorbedDNA.Count >= comp.MaxAbsorbedDNA)
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-max"), uid, uid);
        else comp.AbsorbedDNA.Add(data);

        if (countObjective
        && _mind.TryGetMind(uid, out var mindId, out var mind)
        && _mind.TryGetObjectiveComp<StealDNAConditionComponent>(mindId, out var objective, mind))
        {
            objective.DNAStolen += 1;
        }

        comp.TotalStolenDNA++;

        return true;
    }

    private ChangelingComponent? CopyChangelingComponent(EntityUid target, ChangelingComponent comp)
    {
        var newComp = EnsureComp<ChangelingComponent>(target);
        newComp.AbsorbedDNA = comp.AbsorbedDNA;
        newComp.AbsorbedDNAIndex = comp.AbsorbedDNAIndex;

        newComp.Chemicals = comp.Chemicals;
        newComp.MaxChemicals = comp.MaxChemicals;

        newComp.Biomass = comp.Biomass;
        newComp.MaxBiomass = comp.MaxBiomass;

        newComp.IsInLesserForm = comp.IsInLesserForm;
        newComp.IsInLastResort = comp.IsInLastResort;
        newComp.CurrentForm = comp.CurrentForm;

        newComp.TotalAbsorbedEntities = comp.TotalAbsorbedEntities;
        newComp.TotalStolenDNA = comp.TotalStolenDNA;

        return comp;
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
