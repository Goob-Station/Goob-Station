using Content.Server.Forensics;
using Content.Server.Zombies;
using Content.Shared.Chemistry.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Nutrition.Components;
using Content.Shared.Store.Components;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Fluids;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Timing;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Server.Jittering;
using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.Humanoid;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Server.Flash.Components;

namespace Content.Server._Goobstation.Changeling.EntitySystems;

public sealed partial class ChangelingSystem : SharedChangelingSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AlertsSystem _alertSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly JitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
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

        SubscribeLocalEvent<ChangelingComponent, ComponentInit>(OnComponentInit);
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
    public void ChangeHumanoidAppearance(EntityUid target, HumanoidTransformData data)
    {
        var dataEntity = GetEntity(data.AppearanceEntity);

        if (TryComp<DnaComponent>(target, out var targetDna))
            targetDna.DNA = data.DNA;

        if (TryComp<FingerprintComponent>(target, out var targetFingerprints) && !string.IsNullOrEmpty(data.Fingerprint))
            targetFingerprints.Fingerprint = data.Fingerprint;

        if (TryComp<HumanoidAppearanceComponent>(target, out var targetAppearance) && TryComp<HumanoidAppearanceComponent>(dataEntity, out var dataAppearance))
            _humanoidAppearance.CloneAppearance(dataEntity, target, dataAppearance, targetAppearance);

        _metadata.SetEntityName(target, data.Name);

        PopupSystem.PopupEntity(Loc.GetString("changeling-transform-finish", ("target", data.Name)), target, target);

        if (TryComp<ChangelingComponent>(target, out var changelingComp))
            changelingComp.CurrentForm = data;
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

        // We can ignore fingerprints, but we can't ignore DNA and Appearance because these are main ling things
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

        return true;
    }

    /// <summary>
    ///     Used to remove Humanoid data from ling component
    /// </summary>
    public void SpendDNA(Entity<ChangelingComponent> changeling, HumanoidTransformData data)
    {
        changeling.Comp.AbsorbedDNA.RemoveAll(lingData => lingData.DNA == data.DNA);
    }

    // Most of this should be in polymorphSystem, but i don't want to refactor whole wizden :godo:
    /// <summary>
    ///     Transform entity to new one
    /// </summary>
    public EntityUid? TransformEntityInto(EntityUid uid, EntProtoId prototype, bool dropInventory = false, bool transferDamage = true)
    {
        var config = new PolymorphConfiguration()
        {
            Entity = prototype,
            TransferDamage = transferDamage,
            Forced = true,
            Inventory = dropInventory ? PolymorphInventoryChange.Drop : PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false
        };

        var newEntity = _polymorphSystem.PolymorphEntity(uid, config);

        if (newEntity == null)
            return null;

        // TODO: This should be moved to own Prototype (ChangingData?) that will contain revertable components. Not sure how to do it now.
        // Because of this it's hardcoded.
        TryTransferComponent<HeadRevolutionaryComponent>(uid, newEntity.Value);
        TryTransferComponent<RevolutionaryComponent>(uid, newEntity.Value);
        TryTransferComponent<ZombieImmuneComponent>(uid, newEntity.Value);
        TryTransferComponent<ChangelingComponent>(uid, newEntity.Value);
        TryTransferComponent<StoreComponent>(uid, newEntity.Value);
        TryTransferComponent<FlashImmunityComponent>(uid, newEntity.Value);

        var identity = Identity.Name(newEntity.Value, EntityManager);
        PopupSystem.PopupEntity(Loc.GetString("changeling-transform-others", ("user", identity)), newEntity.Value, PopupType.LargeCaution);

        return newEntity;
    }

    /// <summary>
    ///     Reverts entity transformation
    /// </summary>
    public void RevertTransform(EntityUid uid)
    {
        var oldEntity = _polymorphSystem.Revert(uid);

        if (oldEntity == null)
            return;

        TryTransferComponent<HeadRevolutionaryComponent>(uid, oldEntity.Value);
        TryTransferComponent<RevolutionaryComponent>(uid, oldEntity.Value);
        TryTransferComponent<ZombieImmuneComponent>(uid, oldEntity.Value);
        TryTransferComponent<ChangelingComponent>(uid, oldEntity.Value);
        TryTransferComponent<StoreComponent>(uid, oldEntity.Value);
        TryTransferComponent<FlashImmunityComponent>(uid, oldEntity.Value);
    }
    #endregion

    #region Event Handlers
    private void OnComponentInit(Entity<ChangelingComponent> changeling, ref ComponentInit args)
    {
        RemComp<HungerComponent>(changeling);
        RemComp<ThirstComponent>(changeling);
        EnsureComp<ZombieImmuneComponent>(changeling);

        var comp = changeling.Comp;

        foreach (var actionId in comp.BaseChangelingActions)
            _actionsSystem.AddAction(changeling, actionId);

        comp.Chemicals = comp.MaxChemicals;
        comp.Biomass = comp.MaxBiomass;

        UpdateChemicals(changeling, 0);
        UpdateBiomass(changeling, 0);

        _alertSystem.ShowAlert(changeling, comp.StasisAlert, GetStasisAlertSeverity(comp));
        _bloodstreamSystem.ChangeBloodReagent(changeling, comp.ChangelingBloodPrototype);
    }
    #endregion
}
