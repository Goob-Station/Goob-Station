// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Actions;
using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Server.Changeling.Objectives.Components;
using Content.Goobstation.Shared.Flashbang;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Flash.Components;
using Content.Server.Gravity;
using Content.Server.Guardian;
using Content.Server.Humanoid;
using Content.Server.Light.EntitySystems;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Server.Stunnable;
using Content.Server.Zombies;
using Content.Shared._Goobstation.Weapons.AmmoSelector;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Camera;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Forensics.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingSystem : SharedChangelingSystem
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
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly PullingSystem _pull = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffs = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly RejuvenateSystem _rejuv = default!;
    [Dependency] private readonly SelectableAmmoSystem _selectableAmmo = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public EntProtoId ArmbladePrototype = "ArmBladeChangeling";
    public EntProtoId FakeArmbladePrototype = "FakeArmBladeChangeling";
    public EntProtoId HammerPrototype = "ArmHammerChangeling";
    public EntProtoId ClawPrototype = "ArmClawChangeling";
    public EntProtoId DartGunPrototype = "DartGunChangeling";

    public EntProtoId ShieldPrototype = "ChangelingShield";
    public EntProtoId BoneShardPrototype = "ThrowingStarChangeling";

    public EntProtoId ArmorPrototype = "ChangelingClothingOuterArmor";
    public EntProtoId ArmorHelmetPrototype = "ChangelingClothingHeadHelmet";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingIdentityComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChangelingIdentityComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<ChangelingIdentityComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<ChangelingIdentityComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<ChangelingIdentityComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);

        SubscribeLocalEvent<ChangelingDartComponent, ProjectileHitEvent>(OnDartHit);

        SubscribeLocalEvent<ChangelingIdentityComponent, AugmentedEyesightPurchasedEvent>(OnAugmentedEyesightPurchased);

        SubscribeAbilities();
    }

    private void OnDartHit(Entity<ChangelingDartComponent> ent, ref ProjectileHitEvent args)
    {
        if (HasComp<ChangelingIdentityComponent>(args.Target))
            return;

        if (ent.Comp.ReagentDivisor <= 0)
            return;

        if (!_proto.TryIndex(ent.Comp.StingConfiguration, out var configuration))
            return;

        TryInjectReagents(args.Target,
            configuration.Reagents.Select(x => (x.Key, x.Value / ent.Comp.ReagentDivisor)).ToDictionary());
    }

    protected override void UpdateFlashImmunity(EntityUid uid, bool active)
    {
        if (TryComp(uid, out FlashImmunityComponent? flashImmunity))
            flashImmunity.Enabled = active;
    }

    private void OnAugmentedEyesightPurchased(Entity<ChangelingIdentityComponent> ent, ref AugmentedEyesightPurchasedEvent args)
    {
        InitializeAugmentedEyesight(ent);
    }

    public void InitializeAugmentedEyesight(EntityUid uid)
    {
        EnsureComp<FlashImmunityComponent>(uid);
        EnsureComp<EyeProtectionComponent>(uid);

        var thermalVision = _compFactory.GetComponent<Shared.Overlays.ThermalVisionComponent>();
        thermalVision.Color = Color.FromHex("#FB9898");
        thermalVision.LightRadius = 15f;
        thermalVision.FlashDurationMultiplier = 2f;
        thermalVision.ActivateSound = null;
        thermalVision.DeactivateSound = null;
        thermalVision.ToggleAction = null;

        AddComp(uid, thermalVision);
    }

    private void OnRefreshSpeed(Entity<ChangelingIdentityComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.StrainedMusclesActive)
            args.ModifySpeed(1.25f, 1.5f);
        else
            args.ModifySpeed(1f, 1f);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityManager.EntityQuery<ChangelingIdentityComponent>())
        {
            var uid = comp.Owner;

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateCooldown);

            Cycle(uid, comp);
        }
    }
    public void Cycle(EntityUid uid, ChangelingIdentityComponent comp)
    {
        UpdateChemicals(uid, comp);
        UpdateAbilities(uid, comp);
    }

    private void UpdateChemicals(EntityUid uid, ChangelingIdentityComponent comp, float? amount = null)
    {
        var chemicals = comp.Chemicals;
        // either amount or regen

        if (!CheckFireStatus(uid)) // if on fire, reduce total chemicals restored to a 1/4 //
        {
            chemicals += (amount ?? 1 + comp.BonusChemicalRegen) * comp.ChemicalRegenMultiplier;
        }
        else
        {
            chemicals += (amount ?? 1 + comp.BonusChemicalRegen) * comp.ChemicalRegenMultiplier * 0.25f;
        }

        comp.Chemicals = Math.Clamp(chemicals, 0, comp.MaxChemicals);
        Dirty(uid, comp);
        _alerts.ShowAlert(uid, "ChangelingChemicals");
    }
    private void UpdateAbilities(EntityUid uid, ChangelingIdentityComponent comp)
    {
        _speed.RefreshMovementSpeedModifiers(uid);
        if (comp.StrainedMusclesActive)
        {
            var stamina = EnsureComp<StaminaComponent>(uid);
            _stamina.TakeStaminaDamage(uid, 7.5f, visual: false, immediate: false);
            if (stamina.StaminaDamage >= stamina.CritThreshold || _gravity.IsWeightless(uid))
                ToggleStrainedMuscles(uid, comp);
        }
    }

    #region Helper Methods

    public void PlayMeatySound(EntityUid uid, ChangelingIdentityComponent comp)
    {
        var rand = _rand.Next(0, comp.SoundPool.Count - 1);
        var sound = comp.SoundPool.ToArray()[rand];
        _audio.PlayPvs(sound, uid, AudioParams.Default.WithVolume(-3f));
    }
    public void DoScreech(EntityUid uid, ChangelingIdentityComponent comp)
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
    /// Knocks down and/or stuns entities in range if they aren't a changeling
    /// </summary>
    public void TryScreechStun(EntityUid uid, ChangelingIdentityComponent comp)
    {
        var nearbyEntities = _lookup.GetEntitiesInRange(uid, comp.ShriekPower);

        var stunTime = 2f;
        var knockdownTime = 4f;

        foreach (var player in nearbyEntities)
        {
            if (HasComp<ChangelingIdentityComponent>(player))
                continue;

            var soundEv = new GetFlashbangedEvent(float.MaxValue);
            RaiseLocalEvent(player, soundEv);

            if (soundEv.ProtectionRange < float.MaxValue)
            {
                _stun.TryStun(player, TimeSpan.FromSeconds(stunTime / 2f), true);
                _stun.TryKnockdown(player, TimeSpan.FromSeconds(knockdownTime / 2f), true);
                continue;
            }

            _stun.TryStun(player, TimeSpan.FromSeconds(stunTime), true);
            _stun.TryKnockdown(player, TimeSpan.FromSeconds(knockdownTime), true);
        }
    }

    /// <summary>
    ///     Check if the target is crit/dead or cuffed, for absorbing.
    /// </summary>
    public bool IsIncapacitated(EntityUid uid)
    {
        if (_mobState.IsIncapacitated(uid)
        || (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.CuffedHandCount > 0))
            return true;

        return false;
    }

    /// <summary>
    ///     Check if the target is hard-grabbed, for absorbing.
    /// </summary>
    public bool IsHardGrabbed(EntityUid uid)
    {
        return (TryComp<PullableComponent>(uid, out var pullable) && pullable.GrabStage > GrabStage.Soft);
    }

    public float? GetEquipmentChemCostOverride(ChangelingIdentityComponent comp, EntProtoId proto)
    {
        return comp.Equipment.ContainsKey(proto) ? 0f : null;
    }

    public bool CheckFireStatus(EntityUid uid)
    {
        return (TryComp<FlammableComponent>(uid, out var fire) && fire.OnFire);
    }

    public bool TryUseAbility(EntityUid uid,
        ChangelingIdentityComponent comp,
        BaseActionEvent action,
        float? chemCostOverride = null,
        bool fireAffected = true)
    {
        if (action.Handled)
            return false;

        if (!TryComp<ChangelingActionComponent>(action.Action, out var lingAction))
            return false;

        if (CheckFireStatus(uid) && fireAffected) // checks if the changeling is on fire, and if the ability is affected by fire
        {
            _popup.PopupEntity(Loc.GetString("changeling-onfire"), uid, uid, PopupType.LargeCaution);
            return false;
        }


        if ((!lingAction.UseInLesserForm && comp.IsInLesserForm) || (!lingAction.UseInLastResort && comp.IsInLastResort))
        {
            _popup.PopupEntity(Loc.GetString("changeling-action-fail-lesserform"), uid, uid);
            return false;
        }

        var chemCost = chemCostOverride ?? lingAction.ChemicalCost;

        if (comp.Chemicals < chemCost)
        {
            _popup.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), uid, uid);
            return false;
        }

        if (lingAction.RequireAbsorbed > comp.TotalAbsorbedEntities)
        {
            var delta = lingAction.RequireAbsorbed - comp.TotalAbsorbedEntities;
            _popup.PopupEntity(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), uid, uid);
            return false;
        }

        UpdateChemicals(uid, comp, -chemCost);

        action.Handled = true;

        return true;
    }
    public bool TrySting(EntityUid uid, ChangelingIdentityComponent comp, EntityTargetActionEvent action, bool overrideMessage = false)
    {
        var target = action.Target;

        // can't get his dna if he doesn't have it!
        if (!HasComp<AbsorbableComponent>(target) || HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail"), uid, uid);
            return false;
        }

        if (HasComp<ChangelingIdentityComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-self", ("target", Identity.Entity(target, EntityManager))), uid, uid);
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-ling"), target, target);
            return false;
        }

        if (!TryUseAbility(uid, comp, action))
            return false;

        if (!overrideMessage)
            _popup.PopupEntity(Loc.GetString("changeling-sting", ("target", Identity.Entity(target, EntityManager))), uid, uid);
        return true;
    }
    public bool TryInjectReagents(EntityUid uid, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);

        if (!_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            return false;

        if (!_solution.TryAddSolution(targetSolution.Value, solution))
            return false;

        return true;
    }
    public bool TryReagentSting(EntityUid uid, ChangelingIdentityComponent comp, EntityTargetActionEvent action)
    {
        var target = action.Target;
        if (!TrySting(uid, comp, action))
            return false;

        if (!TryComp(action.Action, out ChangelingReagentStingComponent? reagentSting))
            return false;

        if (!_proto.TryIndex(reagentSting.Configuration, out var configuration))
            return false;

        if (!TryInjectReagents(target, configuration.Reagents))
            return false;

        return true;
    }
    public bool TryToggleItem(EntityUid uid, EntProtoId proto, ChangelingIdentityComponent comp, out EntityUid? equipment)
    {
        equipment = null;
        if (!comp.Equipment.TryGetValue(proto.Id, out var item))
        {
            item = Spawn(proto, Transform(uid).Coordinates);
            if (!_hands.TryForcePickupAnyHand(uid, (EntityUid) item))
            {
                _popup.PopupEntity(Loc.GetString("changeling-fail-hands"), uid, uid);
                QueueDel(item);
                return false;
            }
            comp.Equipment.Add(proto.Id, item);
            equipment = item;
            return true;
        }

        QueueDel(item);
        // assuming that it exists
        comp.Equipment.Remove(proto.Id);

        return true;
    }

    public bool TryToggleArmor(EntityUid uid, ChangelingIdentityComponent comp, (EntProtoId, string)[] armors)
    {
        if (comp.ActiveArmor == null)
        {
            // Equip armor
            var newArmor = new List<EntityUid>();
            var coords = Transform(uid).Coordinates;
            foreach (var (proto, slot) in armors)
            {
                EntityUid armor = EntityManager.SpawnEntity(proto, coords);
                if (!_inventory.TryEquip(uid, armor, slot, force: true))
                {
                    QueueDel(armor);
                    foreach (var delArmor in newArmor)
                        QueueDel(delArmor);

                    return false;
                }
                newArmor.Add(armor);
            }

            comp.ActiveArmor = newArmor;
            comp.ChemicalRegenMultiplier -= 0.25f; // base chem regen slowed by a flat 25%
            return true;
        }
        else
        {
            // Unequip armor
            foreach (var armor in comp.ActiveArmor)
                QueueDel(armor);

            comp.ActiveArmor = null!;
            comp.ChemicalRegenMultiplier += 0.25f; // chem regen debuff removed
            return true;
        }
    }

    public bool TryStealDNA(EntityUid uid, EntityUid target, ChangelingIdentityComponent comp, bool countObjective = false)
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
            DNA = dna.DNA ?? Loc.GetString("forensics-dna-unknown"),
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

    private ChangelingIdentityComponent? CopyChangelingComponent(EntityUid target, ChangelingIdentityComponent comp)
    {
        var newComp = EnsureComp<ChangelingIdentityComponent>(target);
        newComp.AbsorbedDNA = comp.AbsorbedDNA;
        newComp.AbsorbedDNAIndex = comp.AbsorbedDNAIndex;

        newComp.Chemicals = comp.Chemicals;
        newComp.MaxChemicals = comp.MaxChemicals;

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
        ChangelingIdentityComponent? comp = null,
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

        var config = new PolymorphConfiguration
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
            RemCompDeferred<ChangelingIdentityComponent>(uid);
        }

        //    if (TryComp<StoreComponent>(uid, out var storeComp))
        //    {
        //        var storeCompCopy = _serialization.CreateCopy(storeComp, notNullableOverride: true);
        //        RemComp<StoreComponent>(newUid.Value);
        //        EntityManager.AddComponent(newUid.Value, storeCompCopy);
        //    }
        //}

        // exceptional comps check
        // there's no foreach for types i believe so i gotta thug it out yandev style.
        List<Type> types = new()
        {
            typeof(HeadRevolutionaryComponent),
            typeof(RevolutionaryComponent),
            typeof(GhoulComponent),
            typeof(HereticComponent),
            typeof(StoreComponent),
            typeof(FlashImmunityComponent),
            typeof(EyeProtectionComponent),
            typeof(Shared.Overlays.NightVisionComponent),
            typeof(Shared.Overlays.ThermalVisionComponent),
            // ADD MORE TYPES HERE
        };
        foreach (var type in types)
        {
            if (EntityManager.TryGetComponent(uid, type, out var icomp))
            {
                var newComp = (Component) _compFactory.GetComponent(_compFactory.GetComponentName(type));
                var temp = (object) newComp;
                _serialization.CopyTo(icomp, ref temp, notNullableOverride: true);
                EntityManager.AddComponent(newEnt, (Component) temp!);
            }
        }

        RaiseNetworkEvent(new LoadActionsEvent(GetNetEntity(uid)), newEnt);

        Timer.Spawn(300, () => { QueueDel(uid); });

        return newUid;
    }
    public bool TryTransform(EntityUid target, ChangelingIdentityComponent comp, bool sting = false, bool persistentDna = false)
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
        }

        return true;
    }

    public void RemoveAllChangelingEquipment(EntityUid target, ChangelingIdentityComponent comp)
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

    private void OnStartup(EntityUid uid, ChangelingIdentityComponent comp, ref ComponentStartup args)
    {
        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);
        RemComp<CanHostGuardianComponent>(uid);
        RemComp<MartialArtsKnowledgeComponent>(uid);
        RemComp<CanPerformComboComponent>(uid);
        EnsureComp<ZombieImmuneComponent>(uid);

        // add actions
        foreach (var actionId in comp.BaseChangelingActions)
            _actions.AddAction(uid, actionId);

        // making sure things are right in this world
        comp.Chemicals = comp.MaxChemicals;

        // show alerts
        UpdateChemicals(uid, comp, 0);
        // make their blood unreal
        _blood.ChangeBloodReagent(uid, "BloodChangeling");

        // Shitmed: Prevent changelings from getting their body parts severed
        foreach (var (id, part) in _bodySystem.GetBodyChildren(uid))
        {
            part.CanSever = false;
            Dirty(id, part);
        }
    }

    private void OnMobStateChange(EntityUid uid, ChangelingIdentityComponent comp, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemoveAllChangelingEquipment(uid, comp);
    }

    private void OnDamageChange(Entity<ChangelingIdentityComponent> ent, ref DamageChangedEvent args)
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

    private void OnComponentRemove(Entity<ChangelingIdentityComponent> ent, ref ComponentRemove args)
    {
        RemoveAllChangelingEquipment(ent, ent.Comp);
    }

    #endregion
}
