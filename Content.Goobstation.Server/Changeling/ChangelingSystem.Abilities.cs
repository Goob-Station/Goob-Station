// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Server.Changeling.Objectives.Components;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Server.Light.Components;
using Content.Server.Nutrition.Components;
using Content.Shared._Goobstation.Weapons.AmmoSelector;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Stealth.Components;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingSystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<ChangelingIdentityComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbDNAEvent>(OnAbsorb);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingExtractDNAEvent>(OnStingExtractDNA);
        SubscribeLocalEvent<ChangelingIdentityComponent, ChangelingTransformCycleEvent>(OnTransformCycle);
        SubscribeLocalEvent<ChangelingIdentityComponent, ChangelingTransformEvent>(OnTransform);
        SubscribeLocalEvent<ChangelingIdentityComponent, EnterStasisEvent>(OnEnterStasis);
        SubscribeLocalEvent<ChangelingIdentityComponent, ExitStasisEvent>(OnExitStasis);

        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleArmbladeEvent>(OnToggleArmblade);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleArmHammerEvent>(OnToggleHammer);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleArmClawEvent>(OnToggleClaw);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleDartGunEvent>(OnToggleDartGun);
        SubscribeLocalEvent<ChangelingIdentityComponent, CreateBoneShardEvent>(OnCreateBoneShard);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleChitinousArmorEvent>(OnToggleArmor);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleOrganicShieldEvent>(OnToggleShield);
        SubscribeLocalEvent<ChangelingIdentityComponent, ShriekDissonantEvent>(OnShriekDissonant);
        SubscribeLocalEvent<ChangelingIdentityComponent, ShriekResonantEvent>(OnShriekResonant);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleStrainedMusclesEvent>(OnToggleStrainedMuscles);

        SubscribeLocalEvent<ChangelingIdentityComponent, StingReagentEvent>(OnStingReagent);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingTransformEvent>(OnStingTransform);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingFakeArmbladeEvent>(OnStingFakeArmblade);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingLayEggsEvent>(OnLayEgg);

        SubscribeLocalEvent<ChangelingIdentityComponent, ActionAnatomicPanaceaEvent>(OnAnatomicPanacea);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionBiodegradeEvent>(OnBiodegrade);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionChameleonSkinEvent>(OnChameleonSkin);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionEphedrineOverdoseEvent>(OnEphedrineOverdose);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionFleshmendEvent>(OnHealUltraSwag);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionLastResortEvent>(OnLastResort);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionLesserFormEvent>(OnLesserForm);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionSpacesuitEvent>(OnSpacesuit);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionHivemindAccessEvent>(OnHivemindAccess);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbBiomatterEvent>(OnAbsorbBiomatter);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbBiomatterDoAfterEvent>(OnAbsorbBiomatterDoAfter);
    }

    #region Basic Abilities

    private void OnOpenEvolutionMenu(EntityUid uid, ChangelingIdentityComponent comp, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnAbsorb(EntityUid uid, ChangelingIdentityComponent comp, ref AbsorbDNAEvent args)
    {
        var target = args.Target;

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
        if (!IsIncapacitated(target) && !IsHardGrabbed(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-nograb"), uid, uid);
            return;
        }

        if (!TryUseAbility(uid, comp, args))
            return;

        var popupOthers = Loc.GetString("changeling-absorb-start", ("user", Identity.Entity(uid, EntityManager)), ("target", Identity.Entity(target, EntityManager)));
        _popup.PopupEntity(popupOthers, uid, PopupType.LargeCaution);
        PlayMeatySound(uid, comp);
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(15), new AbsorbDNADoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    public ProtoId<DamageGroupPrototype> AbsorbedDamageGroup = "Genetic";
    private void OnAbsorbDoAfter(EntityUid uid, ChangelingIdentityComponent comp, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        if (args.Cancelled || HasComp<AbsorbedComponent>(target) || (!IsIncapacitated(target) && !IsHardGrabbed(target)))
            return;

        PlayMeatySound(args.User, comp);

        UpdateBiomass(uid, comp, comp.MaxBiomass - comp.TotalAbsorbedEntities);

        var dmg = new DamageSpecifier(_proto.Index(AbsorbedDamageGroup), 200);
        _damage.TryChangeDamage(target, dmg, false, false);
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        EnsureComp<AbsorbedComponent>(target);
        EnsureComp<UnrevivableComponent>(target);

        var popup = Loc.GetString("changeling-absorb-end-self-ling");
        var bonusChemicals = 0f;
        var bonusEvolutionPoints = 0f;
        if (TryComp<ChangelingIdentityComponent>(target, out var targetComp))
        {
            bonusChemicals += targetComp.MaxChemicals / 2;
            bonusEvolutionPoints += 10;
            comp.MaxBiomass += targetComp.MaxBiomass / 2;
        }
        else
        {
            popup = Loc.GetString("changeling-absorb-end-self");
            bonusChemicals += 10;
            bonusEvolutionPoints += 2;
        }
        TryStealDNA(uid, target, comp, true);
        comp.TotalAbsorbedEntities++;

        _popup.PopupEntity(popup, args.User, args.User);
        comp.MaxChemicals += bonusChemicals;

        if (TryComp<StoreComponent>(args.User, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "EvolutionPoint", bonusEvolutionPoints } }, args.User, store);
            _store.UpdateUserInterface(args.User, args.User, store);
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            if (_mind.TryGetObjectiveComp<AbsorbConditionComponent>(mindId, out var objective, mind))
                objective.Absorbed += 1;
    }

    public List<ProtoId<ReagentPrototype>> BiomassAbsorbedChemicals = new() { "Nutriment", "Protein", "UncookedAnimalProteins", "Fat" }; // fat so absorbing raw meat good
    private void OnAbsorbBiomatter(EntityUid uid, ChangelingIdentityComponent comp, ref AbsorbBiomatterEvent args)
    {
        var target = args.Target;

        if (!TryUseAbility(uid, comp, args))
            return;

        if (!TryComp<FoodComponent>(target, out var food))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(target, out var solMan))
            return;

        var totalFood = FixedPoint2.New(0);
        foreach (var (_, sol) in _solution.EnumerateSolutions((target, solMan)))
            foreach (var proto in BiomassAbsorbedChemicals)
                totalFood += sol.Comp.Solution.GetTotalPrototypeQuantity(proto);

        if (food.RequiresSpecialDigestion || totalFood == 0) // no eating winter coats or food that won't give you anything
        {
            var popup = Loc.GetString("changeling-absorbbiomatter-bad-food");
            _popup.PopupEntity(popup, uid, uid);
            return;
        }

        var popupOthers = Loc.GetString("changeling-absorbbiomatter-start", ("user", Identity.Entity(uid, EntityManager)));
        _popup.PopupEntity(popupOthers, uid, PopupType.MediumCaution);
        PlayMeatySound(uid, comp);
        // so you can't just instantly mukbang a bag of food mid-combat, 2.7s for raw meat
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(totalFood.Float() * 0.15f), new AbsorbBiomatterDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnAbsorbBiomatterDoAfter(EntityUid uid, ChangelingIdentityComponent comp, ref AbsorbBiomatterDoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        if (args.Cancelled)
            return;

        if (!TryComp<SolutionContainerManagerComponent>(target, out var solMan))
            return;

        var totalFood = FixedPoint2.New(0);
        foreach (var (name, sol) in _solution.EnumerateSolutions((target, solMan)))
        {
            var solution = sol.Comp.Solution;
            foreach (var proto in BiomassAbsorbedChemicals)
            {
                var quant = solution.GetTotalPrototypeQuantity(proto);
                totalFood += quant;
                solution.RemoveReagent(proto, quant);
            }
            _puddle.TrySpillAt(target, solution, out var _);
        }

        UpdateChemicals(uid, comp, totalFood.Float() * 2); // 36 for raw meat

        QueueDel(target); // eaten
    }

    private void OnStingExtractDNA(EntityUid uid, ChangelingIdentityComponent comp, ref StingExtractDNAEvent args)
    {
        if (!TrySting(uid, comp, args, true))
            return;

        var target = args.Target;
        if (!TryStealDNA(uid, target, comp, true))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-fail"), uid, uid);
            // royal cashback
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
        }
        else _popup.PopupEntity(Loc.GetString("changeling-sting", ("target", Identity.Entity(target, EntityManager))), uid, uid);
    }

    private void OnTransformCycle(EntityUid uid, ChangelingIdentityComponent comp, ref ChangelingTransformCycleEvent args)
    {
        comp.AbsorbedDNAIndex += 1;
        if (comp.AbsorbedDNAIndex >= comp.MaxAbsorbedDNA || comp.AbsorbedDNAIndex >= comp.AbsorbedDNA.Count)
            comp.AbsorbedDNAIndex = 0;

        if (comp.AbsorbedDNA.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-cycle-empty"), uid, uid);
            return;
        }

        var selected = comp.AbsorbedDNA.ToArray()[comp.AbsorbedDNAIndex];
        comp.SelectedForm = selected;
        _popup.PopupEntity(Loc.GetString("changeling-transform-cycle", ("target", selected.Name)), uid, uid);
    }
    private void OnTransform(EntityUid uid, ChangelingIdentityComponent comp, ref ChangelingTransformEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        if (!TryTransform(uid, comp))
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
    }

    private void OnEnterStasis(EntityUid uid, ChangelingIdentityComponent comp, ref EnterStasisEvent args)
    {
        if (comp.IsInStasis || HasComp<AbsorbedComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter-fail"), uid, uid);
            return;
        }

        if (!TryUseAbility(uid, comp, args))
            return;

        comp.Chemicals = 0f;

        if (_mobState.IsAlive(uid))
        {
            // fake our death
            var othersMessage = Loc.GetString("suicide-command-default-text-others", ("name", uid));
            _popup.PopupEntity(othersMessage, uid, Filter.PvsExcept(uid), true);

            var selfMessage = Loc.GetString("changeling-stasis-enter");
            _popup.PopupEntity(selfMessage, uid, uid);
        }

        if (!_mobState.IsDead(uid))
            _mobState.ChangeMobState(uid, MobState.Dead);

        comp.IsInStasis = true;
    }
    private void OnExitStasis(EntityUid uid, ChangelingIdentityComponent comp, ref ExitStasisEvent args)
    {
        if (!comp.IsInStasis)
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-fail"), uid, uid);
            return;
        }
        if (HasComp<AbsorbedComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-fail-dead"), uid, uid);
            return;
        }

        if (!TryUseAbility(uid, comp, args))
            return;

        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return;

        // heal of everything
        _rejuv.PerformRejuvenate(uid);

        _popup.PopupEntity(Loc.GetString("changeling-stasis-exit"), uid, uid);

        comp.IsInStasis = false;
    }

    #endregion

    #region Combat Abilities

    private void OnToggleArmblade(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleArmbladeEvent args)
    {
        if (!TryUseAbility(uid, comp, args, GetEquipmentChemCostOverride(comp, ArmbladePrototype)))
            return;

        if (!TryToggleItem(uid, ArmbladePrototype, comp, out _))
            return;

        PlayMeatySound(uid, comp);
    }
    private void OnToggleHammer(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleArmHammerEvent args)
    {
        if (!TryUseAbility(uid, comp, args, GetEquipmentChemCostOverride(comp, HammerPrototype)))
            return;

        if (!TryToggleItem(uid, HammerPrototype, comp, out _))
            return;

        PlayMeatySound(uid, comp);
    }
    private void OnToggleClaw(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleArmClawEvent args)
    {
        if (!TryUseAbility(uid, comp, args, GetEquipmentChemCostOverride(comp, ClawPrototype)))
            return;

        if (!TryToggleItem(uid, ClawPrototype, comp, out _))
            return;

        PlayMeatySound(uid, comp);
    }
    private void OnToggleDartGun(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleDartGunEvent args)
    {
        var chemCostOverride = GetEquipmentChemCostOverride(comp, DartGunPrototype);

        if (!TryUseAbility(uid, comp, args, chemCostOverride))
            return;

        if (!TryToggleItem(uid, DartGunPrototype, comp, out var dartgun))
            return;

        if (!TryComp(dartgun, out AmmoSelectorComponent? ammoSelector))
        {
            PlayMeatySound(uid, comp);
            return;
        }

        if (!_mind.TryGetMind(uid, out var mindId, out _) || !TryComp(mindId, out ActionsContainerComponent? container))
            return;

        var setProto = false;
        foreach (var ability in container.Container.ContainedEntities)
        {
            if (!TryComp(ability, out ChangelingReagentStingComponent? sting) || sting.DartGunAmmo == null)
                continue;

            ammoSelector.Prototypes.Add(sting.DartGunAmmo.Value);

            if (setProto)
                continue;

            _selectableAmmo.TrySetProto((dartgun.Value, ammoSelector), sting.DartGunAmmo.Value);
            setProto = true;
        }

        if (ammoSelector.Prototypes.Count == 0)
        {
            comp.Chemicals += chemCostOverride ?? Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            _popup.PopupEntity(Loc.GetString("changeling-dartgun-no-stings"), uid, uid);
            comp.Equipment.Remove(DartGunPrototype);
            QueueDel(dartgun.Value);
            return;
        }

        Dirty(dartgun.Value, ammoSelector);

        PlayMeatySound(uid, comp);
    }
    private void OnCreateBoneShard(EntityUid uid, ChangelingIdentityComponent comp, ref CreateBoneShardEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        var star = Spawn(BoneShardPrototype, Transform(uid).Coordinates);
        _hands.TryPickupAnyHand(uid, star);

        PlayMeatySound(uid, comp);
    }
    private void OnToggleArmor(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleChitinousArmorEvent args)
    {
        float? chemCostOverride = comp.ActiveArmor == null ? null : 0f;

        if (!TryUseAbility(uid, comp, args, chemCostOverride))
            return;

        if (!TryToggleArmor(uid, comp, [(ArmorHelmetPrototype, "head"), (ArmorPrototype, "outerClothing")]))
        {
            _popup.PopupEntity(Loc.GetString("changeling-equip-armor-fail"), uid, uid);
            comp.Chemicals += chemCostOverride ?? Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        PlayMeatySound(uid, comp);
    }
    private void OnToggleShield(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleOrganicShieldEvent args)
    {
        if (!TryUseAbility(uid, comp, args, GetEquipmentChemCostOverride(comp, ShieldPrototype)))
            return;

        if (!TryToggleItem(uid, ShieldPrototype, comp, out _))
            return;

        PlayMeatySound(uid, comp);
    }
    private void OnShriekDissonant(EntityUid uid, ChangelingIdentityComponent comp, ref ShriekDissonantEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        DoScreech(uid, comp);

        var pos = _transform.GetMapCoordinates(uid);
        var power = comp.ShriekPower;
        _emp.EmpPulse(pos, power, 5000f, power * 2);
    }
    private void OnShriekResonant(EntityUid uid, ChangelingIdentityComponent comp, ref ShriekResonantEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        DoScreech(uid, comp);

        var power = comp.ShriekPower;
        _flash.FlashArea(uid, uid, power, power * 2f * 1000f);

        var lookup = _lookup.GetEntitiesInRange(uid, power);
        var lights = GetEntityQuery<PoweredLightComponent>();

        foreach (var ent in lookup)
            if (lights.HasComponent(ent))
                _light.TryDestroyBulb(ent);
    }
    private void OnToggleStrainedMuscles(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleStrainedMusclesEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        ToggleStrainedMuscles(uid, comp);
    }
    private void ToggleStrainedMuscles(EntityUid uid, ChangelingIdentityComponent comp)
    {
        if (!comp.StrainedMusclesActive)
        {
            _popup.PopupEntity(Loc.GetString("changeling-muscles-start"), uid, uid);
            comp.StrainedMusclesActive = true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("changeling-muscles-end"), uid, uid);
            comp.StrainedMusclesActive = false;
        }

        PlayMeatySound(uid, comp);
        _speed.RefreshMovementSpeedModifiers(uid);
    }

    #endregion

    #region Stings

    private void OnStingReagent(EntityUid uid, ChangelingIdentityComponent comp, StingReagentEvent args)
    {
        TryReagentSting(uid, comp, args);
    }
    private void OnStingTransform(EntityUid uid, ChangelingIdentityComponent comp, ref StingTransformEvent args)
    {
        if (!TrySting(uid, comp, args, true))
            return;

        var target = args.Target;
        if (!TryTransform(target, comp, true, true))
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
    }
    private void OnStingFakeArmblade(EntityUid uid, ChangelingIdentityComponent comp, ref StingFakeArmbladeEvent args)
    {
        if (!TrySting(uid, comp, args))
            return;

        var target = args.Target;
        var fakeArmblade = EntityManager.SpawnEntity(FakeArmbladePrototype, Transform(target).Coordinates);
        if (!_hands.TryPickupAnyHand(target, fakeArmblade))
        {
            QueueDel(fakeArmblade);
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-simplemob"), uid, uid);
            return;
        }

        PlayMeatySound(target, comp);
    }
    public void OnLayEgg(EntityUid uid, ChangelingIdentityComponent comp, ref StingLayEggsEvent args)
    {
        var target = args.Target;

        if (!_mobState.IsDead(target))
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

        var mind = _mind.GetMind(uid);
        if (mind == null)
            return;
        if (!TryComp<StoreComponent>(uid, out var storeComp))
            return;

        comp.IsInLastResort = false;
        comp.IsInLesserForm = true;

        var eggComp = EnsureComp<ChangelingEggComponent>(target);
        eggComp.lingComp = comp;
        eggComp.lingMind = (EntityUid) mind;
        eggComp.lingStore = _serialization.CreateCopy(storeComp, notNullableOverride: true);
        eggComp.AugmentedEyesightPurchased = HasComp<Shared.Overlays.ThermalVisionComponent>(uid);

        EnsureComp<AbsorbedComponent>(target);
        var dmg = new DamageSpecifier(_proto.Index(AbsorbedDamageGroup), 200);
        _damage.TryChangeDamage(target, dmg, false, false);
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        PlayMeatySound(uid, comp);

        _bodySystem.GibBody(uid);
    }

    #endregion

    #region Utilities

    public void OnAnatomicPanacea(EntityUid uid, ChangelingIdentityComponent comp, ref ActionAnatomicPanaceaEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        var reagents = new Dictionary<string, FixedPoint2>
        {
            { "LingPanacea", 10f },
        };
        if (TryInjectReagents(uid, reagents))
            _popup.PopupEntity(Loc.GetString("changeling-panacea"), uid, uid);
        else
            return;
        PlayMeatySound(uid, comp);
    }
    public void OnBiodegrade(EntityUid uid, ChangelingIdentityComponent comp, ref ActionBiodegradeEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        if (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.Container.ContainedEntities.Count > 0)
        {
            var cuff = cuffs.LastAddedCuffs;

            _cuffs.Uncuff(uid, cuffs.LastAddedCuffs, cuff);
            QueueDel(cuff);
        }

        var soln = new Solution();
        soln.AddReagent("PolytrinicAcid", 10f);

        if (_pull.IsPulled(uid))
        {
            var puller = Comp<PullableComponent>(uid).Puller;
            if (puller != null)
            {
                _puddle.TrySplashSpillAt((EntityUid) puller, Transform((EntityUid) puller).Coordinates, soln, out _);
                return;
            }
        }
        _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, soln, out _);
    }
    public void OnChameleonSkin(EntityUid uid, ChangelingIdentityComponent comp, ref ActionChameleonSkinEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        if (HasComp<StealthComponent>(uid) && HasComp<StealthOnMoveComponent>(uid))
        {
            RemComp<StealthComponent>(uid);
            RemComp<StealthOnMoveComponent>(uid);
            _popup.PopupEntity(Loc.GetString("changeling-chameleon-end"), uid, uid);
            return;
        }

        EnsureComp<StealthComponent>(uid);
        EnsureComp<StealthOnMoveComponent>(uid);
        _popup.PopupEntity(Loc.GetString("changeling-chameleon-start"), uid, uid);
    }
    public void OnEphedrineOverdose(EntityUid uid, ChangelingIdentityComponent comp, ref ActionEphedrineOverdoseEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        var stam = EnsureComp<StaminaComponent>(uid);
        stam.StaminaDamage = 0;

        var reagents = new Dictionary<string, FixedPoint2>
        {
            { "Desoxyephedrine", 5f }
        };
        if (TryInjectReagents(uid, reagents))
            _popup.PopupEntity(Loc.GetString("changeling-inject"), uid, uid);
        else
        {
            _popup.PopupEntity(Loc.GetString("changeling-inject-fail"), uid, uid);
        }
    }
    // john space made me do this
    public void OnHealUltraSwag(EntityUid uid, ChangelingIdentityComponent comp, ref ActionFleshmendEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        var reagents = new Dictionary<string, FixedPoint2>
        {
            { "LingFleshmend", 10f },
        };
        if (TryInjectReagents(uid, reagents))
            _popup.PopupEntity(Loc.GetString("changeling-fleshmend"), uid, uid);
        else return;
        PlayMeatySound(uid, comp);
    }
    public void OnLastResort(EntityUid uid, ChangelingIdentityComponent comp, ref ActionLastResortEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        comp.IsInLastResort = true;

        var newUid = TransformEntity(
            uid,
            protoId: "MobHeadcrab",
            comp: comp,
            dropInventory: true,
            transferDamage: false);

        if (newUid == null)
        {
            comp.IsInLastResort = false;
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        _explosionSystem.QueueExplosion(
            (EntityUid) newUid,
            typeId: "Default",
            totalIntensity: 1,
            slope: 4,
            maxTileIntensity: 2);

        _actions.AddAction((EntityUid) newUid, "ActionLayEgg");

        PlayMeatySound((EntityUid) newUid, comp);
    }
    public void OnLesserForm(EntityUid uid, ChangelingIdentityComponent comp, ref ActionLesserFormEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        comp.IsInLesserForm = true;
        var newUid = TransformEntity(uid, protoId: "MobMonkey", comp: comp);
        if (newUid == null)
        {
            comp.IsInLesserForm = false;
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        PlayMeatySound((EntityUid) newUid, comp);
        var loc = Loc.GetString("changeling-transform-others", ("user", Identity.Entity((EntityUid) newUid, EntityManager)));
        _popup.PopupEntity(loc, (EntityUid) newUid, PopupType.LargeCaution);
    }
    public void OnSpacesuit(EntityUid uid, ChangelingIdentityComponent comp, ref ActionSpacesuitEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        if (!TryToggleArmor(uid, comp, [(SpacesuitHelmetPrototype, "head"), (SpacesuitPrototype, "outerClothing")]))
        {
            _popup.PopupEntity(Loc.GetString("changeling-equip-armor-fail"), uid, uid);
            comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            return;
        }

        PlayMeatySound(uid, comp);
    }
    public ProtoId<TagPrototype> HivemindTag = "LingMind";
    public void OnHivemindAccess(EntityUid uid, ChangelingIdentityComponent comp, ref ActionHivemindAccessEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        if (HasComp<HivemindComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("changeling-passive-active"), uid, uid);
            return;
        }

        EnsureComp<HivemindComponent>(uid);
        _tag.AddTag(uid, HivemindTag);

        _popup.PopupEntity(Loc.GetString("changeling-hivemind-start"), uid, uid);
    }

    #endregion
}