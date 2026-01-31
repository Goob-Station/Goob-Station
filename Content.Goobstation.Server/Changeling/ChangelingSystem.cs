// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Actions;
using Content.Goobstation.Common.Body;
using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Common.Conversion;
using Content.Goobstation.Common.Magic;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Common.Medical;
using Content.Goobstation.Common.Mind;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.Changeling.GameTicking.Rules;
using Content.Goobstation.Server.Changeling.Objectives.Components;
using Content.Goobstation.Shared.Changeling;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Goobstation.Shared.Flashbang;
using Content.Goobstation.Shared.InternalResources.Data;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Server.Actions;
using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
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
using Content.Shared.Flash.Components;
using Content.Shared.Fluids;
using Content.Shared.Forensics.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Medical;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Polymorph;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Overlays;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingSystem : SharedChangelingSystem
{
    // this is one hell of a star wars intro text
    // im killing all of you bro this is so ass
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
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
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
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly PullingSystem _pull = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffs = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly SelectableAmmoSystem _selectableAmmo = default!;
    [Dependency] private readonly ChangelingRuleSystem _changelingRuleSystem = default!;
    [Dependency] private readonly SharedInternalResourcesSystem _resources = default!;

    private readonly EntProtoId _armbladePrototype = "ArmBladeChangeling";
    private readonly EntProtoId _fakeArmbladePrototype = "FakeArmBladeChangeling";
    private readonly EntProtoId _hammerPrototype = "ArmHammerChangeling";
    private readonly EntProtoId _clawPrototype = "ArmClawChangeling";
    private readonly EntProtoId _dartGunPrototype = "DartGunChangeling";
    private readonly EntProtoId _shieldPrototype = "ChangelingShield";
    private readonly EntProtoId _boneShardPrototype = "ThrowingStarChangeling";
    private readonly EntProtoId _armorPrototype = "ChangelingClothingOuterArmor";
    private readonly EntProtoId _armorHelmetPrototype = "ChangelingClothingHeadHelmet";
    private readonly ProtoId<InternalResourcesPrototype> _resourceType = "ChangelingChemicals";
    private readonly List<Type> _types =
    [
        typeof(FlashImmunityComponent),
        typeof(EyeProtectionComponent),
        typeof(NightVisionComponent),
        typeof(ThermalVisionComponent),
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingIdentityComponent, MapInitEvent>(OnIdentityMapInit);
        SubscribeLocalEvent<ChangelingComponent, MapInitEvent>(OnChangelingMapInit);

        SubscribeLocalEvent<ChangelingIdentityComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<ChangelingIdentityComponent, UpdateMobStateEvent>(OnUpdateMobState);
        SubscribeLocalEvent<ChangelingIdentityComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<ChangelingIdentityComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<ChangelingIdentityComponent, TargetBeforeDefibrillatorZapsEvent>(OnDefibZap);
        SubscribeLocalEvent<ChangelingIdentityComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<ChangelingIdentityComponent, PolymorphedEvent>(OnPolymorphed);

        SubscribeLocalEvent<ChangelingComponent, PolymorphedEvent>(OnPolymorphedTakeTwo);
        SubscribeLocalEvent<ChangelingComponent, BeforeAmputationDamageEvent>(OnLimbAmputation);
        SubscribeLocalEvent<ChangelingComponent, GetAntagSelectionBlockerEvent>(OnGetAntagBlocker);
        SubscribeLocalEvent<ChangelingComponent, BeforeMindSwappedEvent>(OnMindswapAttempt);
        SubscribeLocalEvent<ChangelingComponent, BeforeConversionEvent>(OnConversionAttempt);
        SubscribeLocalEvent<ChangelingComponent, BeforeBrainRemovedEvent>(OnBrainRemoveAttempt);
        SubscribeLocalEvent<ChangelingComponent, BeforeBrainAddedEvent>(OnBrainAddAttempt);

        SubscribeLocalEvent<ChangelingIdentityComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<ChangelingIdentityComponent, InternalResourcesRegenModifierEvent>(OnChemicalRegen);

        SubscribeLocalEvent<ChangelingDartComponent, ProjectileHitEvent>(OnDartHit);

        SubscribeLocalEvent<ChangelingIdentityComponent, AwakenedInstinctPurchasedEvent>(OnAwakenedInstinctPurchased);
        SubscribeLocalEvent<ChangelingIdentityComponent, AugmentedEyesightPurchasedEvent>(OnAugmentedEyesightPurchased);
        SubscribeLocalEvent<ChangelingIdentityComponent, VoidAdaptionPurchasedEvent>(OnVoidAdaptionPurchased);

        SubscribeAbilities();
    }
    protected override void UpdateFlashImmunity(EntityUid uid, bool active)
    {
        if (TryComp(uid, out FlashImmunityComponent? flashImmunity))
            flashImmunity.Enabled = active;
    }

    private void OnAwakenedInstinctPurchased(Entity<ChangelingIdentityComponent> ent, ref AwakenedInstinctPurchasedEvent args)
    {
        EnsureComp<ChangelingBiomassComponent>(ent);
    }

    private void OnAugmentedEyesightPurchased(Entity<ChangelingIdentityComponent> ent, ref AugmentedEyesightPurchasedEvent args)
    {
        InitializeAugmentedEyesight(ent);
    }

    private void OnVoidAdaptionPurchased(Entity<ChangelingIdentityComponent> ent, ref VoidAdaptionPurchasedEvent args)
    {
        EnsureComp<VoidAdaptionComponent>(ent);
    }

    public void InitializeAugmentedEyesight(EntityUid uid)
    {
        EnsureComp<FlashImmunityComponent>(uid);
        EnsureComp<EyeProtectionComponent>(uid);

        var thermalVision = _compFactory.GetComponent<ThermalVisionComponent>();
        thermalVision.Color = Color.FromHex("#FB9898");
        thermalVision.LightRadius = 15f;
        thermalVision.FlashDurationMultiplier = 2f;
        thermalVision.ActivateSound = null;
        thermalVision.DeactivateSound = null;
        thermalVision.ToggleAction = null;

        AddComp(uid, thermalVision);
    }

    private static void OnRefreshSpeed(Entity<ChangelingIdentityComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.StrainedMusclesActive)
            args.ModifySpeed(1.25f, 1.5f);
        else
            args.ModifySpeed(1f, 1f);
    }

    // TODO nuke this in the future and have this handled by systems for each relevant ability, like biomass does
    private void OnChemicalRegen(Entity<ChangelingIdentityComponent> ent, ref InternalResourcesRegenModifierEvent args)
    {
        if (args.Data.InternalResourcesType != _resourceType)
            return;

        if (ent.Comp.ChameleonActive)
            args.Modifier -= 0.25f;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var eqe = EntityManager.EntityQueryEnumerator<ChangelingIdentityComponent>();
        while(eqe.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;
            comp.UpdateTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateCooldown);
            HandleStrainedMuscles((uid, comp));
            HandleStasis((uid, comp));
        }
    }


    private void UpdateChemicals(Entity<ChangelingIdentityComponent> ent, float amount, ChangelingChemicalComponent? chemComp = null)
    {
        if (!Resolve(ent, ref chemComp)
            || chemComp.ResourceData == null)
            return;

        _resources.TryUpdateResourcesAmount(ent, chemComp.ResourceData, amount);
    }

    private void UpdateBiomass(Entity<ChangelingIdentityComponent> ent, float amount, ChangelingBiomassComponent? bioComp = null)
    {
        if (!Resolve(ent, ref bioComp)
            || bioComp.ResourceData == null)
            return;
        _resources.TryUpdateResourcesAmount(ent, bioComp.ResourceData, amount);
    }

    private void HandleStasis(Entity<ChangelingIdentityComponent> ent)
    {
        if (ent.Comp is not { IsInStasis: true, StasisTime: > 0f })
            return;
        ent.Comp.StasisTime -= 1f;
        if (ent.Comp.StasisTime == 0f) // If this tick finished the stasis timer
            _popup.PopupEntity(Loc.GetString("changeling-stasis-finished"), ent, ent);
    }

    private void HandleStrainedMuscles(Entity<ChangelingIdentityComponent> ent)
    {
        _speed.RefreshMovementSpeedModifiers(ent);
        if (!ent.Comp.StrainedMusclesActive)
            return;
        var stamina = EnsureComp<StaminaComponent>(ent);
        _stamina.TakeStaminaDamage(ent, 7.5f, visual: false, immediate: false);
        if (stamina.StaminaDamage >= stamina.CritThreshold || _gravity.IsWeightless(ent))
            ToggleStrainedMuscles(ent);
    }

    #region Helper Methods

    private void PlayMeatySound(EntityUid ent, EntityUid? target = null, ChangelingIdentityComponent? comp = null)
    {
        if(!Resolve(ent, ref comp))
            return;
        var rand = _rand.Next(0, comp.SoundPool.Count - 1);
        _audio.PlayPvs(comp.SoundPool.ToArray()[rand],
            target ?? ent,
            AudioParams.Default.WithVolume(-3f));
    }

    private void DoScreech(Entity<ChangelingIdentityComponent> ent)
    {
        _audio.PlayPvs(ent.Comp.ShriekSound, ent);
        var center = _transform.GetMapCoordinates(ent);
        var gamers = Filter.Empty();
        gamers.AddInRange(center, ent.Comp.ShriekPower, _player, EntityManager);
        foreach (var gamer in gamers.Recipients)
        {
            if (gamer.AttachedEntity == null)
                continue;
            var delta = center.Position - _transform.GetWorldPosition(gamer.AttachedEntity.Value);
            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(.01f, 0);
            _recoil.KickCamera(ent, -delta.Normalized());
        }
    }

    /// <summary>
    /// Knocks down and/or stuns entities in range if they aren't a changeling
    /// </summary>
    private void TryScreechStun(Entity<ChangelingIdentityComponent> ent)
    {
        var nearbyEntities = _lookup.GetEntitiesInRange(ent, ent.Comp.ShriekPower);
        const float stunTime = 2f;
        const float knockdownTime = 4f;

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
    private bool IsIncapacitated(EntityUid uid)
    {
        return _mobState.IsIncapacitated(uid)
               || (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.CuffedHandCount > 0);
    }

    /// <summary>
    ///     Check if the target is hard-grabbed, for absorbing.
    /// </summary>
    private bool IsHardGrabbed(EntityUid uid)
    {
        return TryComp<PullableComponent>(uid, out var pullable) && pullable.GrabStage > GrabStage.Soft;
    }

    private static float? GetEquipmentChemCostOverride(ChangelingIdentityComponent comp, EntProtoId proto)
    {
        return comp.Equipment.ContainsKey(proto) ? 0f : null;
    }

    private bool CheckFireStatus(EntityUid uid)
    {
        return TryComp<FlammableComponent>(uid, out var fire) && fire.OnFire;
    }

    private bool TrySting(Entity<ChangelingIdentityComponent> ent, EntityTargetActionEvent action, bool overrideMessage = false)
    {
        var target = action.Target;

        // can't sting a dried out husk
        if (HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-hollow"), ent, ent);
            return false;
        }

        if (HasComp<ChangelingIdentityComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-self", ("target", Identity.Entity(target, EntityManager))), ent, ent);
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-ling"), target, target);
            return false;
        }

        if (!overrideMessage)
            _popup.PopupEntity(Loc.GetString("changeling-sting", ("target", Identity.Entity(target, EntityManager))), ent, ent);
        return true;
    }

    private bool TryInjectReagents(EntityUid uid, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);
        return _solution.TryGetInjectableSolution(uid, out var targetSolution, out _)
               && _solution.TryAddSolution(targetSolution.Value, solution);
    }

    private bool TryReagentSting(Entity<ChangelingIdentityComponent> ent, EntityTargetActionEvent action)
    {
        var target = action.Target;
        if (!TrySting(ent, action) || !TryComp(action.Action, out ChangelingReagentStingComponent? reagentSting))
            return false;

        return _proto.TryIndex(reagentSting.Configuration, out var configuration) && TryInjectReagents(target, configuration.Reagents);
    }

    private bool TryToggleItem(EntityUid uid,
        EntProtoId proto,
        out EntityUid? equipment,
        ChangelingIdentityComponent? comp = null)
    {
        equipment = null;
        if (!Resolve(uid, ref comp))
            return false;
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

    private bool TryToggleArmor(EntityUid uid, ChangelingIdentityComponent comp, (EntProtoId, string)[] armors)
    {
        if (comp.ActiveArmor == null)
        {
            // Equip armor
            var newArmor = new List<EntityUid>();
            var coords = Transform(uid).Coordinates;
            foreach (var (proto, slot) in armors)
            {
                var armor = EntityManager.SpawnEntity(proto, coords);
                if (!_inventory.TryEquip(uid, armor, slot, force: true))
                {
                    QueueDel(armor);
                    foreach (var delArmor in newArmor)
                        QueueDel(delArmor);

                    return false;
                }
                newArmor.Add(armor);
            }

            _audio.PlayPvs(comp.ArmourSound, uid, AudioParams.Default);

            comp.ActiveArmor = newArmor;
        }
        else
        {
            // Unequip armor
            foreach (var armor in comp.ActiveArmor)
                QueueDel(armor);

            _audio.PlayPvs(comp.ArmourStripSound, uid, AudioParams.Default);
            comp.ActiveArmor = null!;
        }

        return true;
    }

    private bool TryStealDNA(Entity<ChangelingIdentityComponent> ent, EntityUid target, bool countObjective = false)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var appearance)
        || !TryComp<DnaComponent>(target, out var dna)
        || !TryComp<FingerprintComponent>(target, out var fingerprint))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-fail-lesser"), ent, ent);
            return false;
        }

        var metadata = MetaData(target);
        if (ent.Comp.AbsorbedHistory.Any(storedDNA => storedDNA.DNA == dna.DNA))
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-fail-duplicate"), ent, ent);
            return false;
        }

        var data = new TransformData
        {
            Name = metadata.EntityName,
            DNA = dna.DNA ?? Loc.GetString("forensics-dna-unknown"),
            Appearance = appearance,
        };

        if (fingerprint.Fingerprint != null)
            data.Fingerprint = fingerprint.Fingerprint;

        if (countObjective
        && _mind.TryGetMind(ent, out var mindId, out var mind)
        && _mind.TryGetObjectiveComp<StealDNAConditionComponent>(mindId, out var objective, mind)
        && ent.Comp.AbsorbedDNA.Count < ent.Comp.MaxAbsorbedDNA) // no cheesing by spamming dna extract
        {
            objective.DNAStolen += 1;
        }

        if (ent.Comp.AbsorbedDNA.Count >= ent.Comp.MaxAbsorbedDNA)
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-max"), ent, ent);
        else
        {
            ent.Comp.AbsorbedHistory.Add(data); // so we can't just come back and sting them again

            ent.Comp.AbsorbedDNA.Add(data);
            ent.Comp.TotalStolenDNA++;
        }

        return true;
    }
    private EntityUid? TransformEntity(
        EntityUid uid,
        TransformData? data = null,
        EntProtoId? protoId = null,
        ChangelingIdentityComponent? comp = null,
        bool dropInventory = false,
        bool transferDamage = true)
    {
        EntProtoId? pid;

        if (data != null)
        {
            if (!_proto.TryIndex(data.Appearance.Species, out var species))
                return null;
            pid = species.Prototype;
        }
        else if (protoId != null)
            pid = protoId;
        else
            return null;

        if (data != null
            && comp != null)
            comp.AbsorbedDNA.Remove(data);

        var config = new PolymorphConfiguration
        {
            Entity = pid,
            TransferDamage = transferDamage,
            Forced = true,
            Inventory = dropInventory ? PolymorphInventoryChange.Drop : PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false,
        };

        var newUid = _polymorph.PolymorphEntity(uid, config);

        if (newUid == null)
            return null;

        var newEnt = newUid.Value;

        if (data != null)
        {
            Comp<FingerprintComponent>(newEnt).Fingerprint = data.Fingerprint;
            Comp<DnaComponent>(newEnt).DNA = data.DNA;
            _humanoid.CloneAppearance(data.Appearance.Owner, newEnt); // im killiung john
            _metaData.SetEntityName(newEnt, data.Name);
            var message = Loc.GetString("changeling-transform-finish", ("target", data.Name));
            _popup.PopupEntity(message, newEnt, newEnt);
        }

        // otherwise we can only transform once
        RemCompDeferred<PolymorphedEntityComponent>(newEnt);

        // exceptional comps check
        // TODO make PolymorphedEvent handlers for all
        // ReSharper disable once EntityNameCapturedOnly.Local
        foreach (var type in _types)
            _polymorph.CopyPolymorphComponent(uid, newEnt, nameof(type));

        // CopyPolymorphComponent fails to copy the HumanoidAppearanceComponent in TransformData
        // outside the first list item so this has to be done manually unfortunately
        if (TryComp<ChangelingIdentityComponent>(newEnt, out var newComp)
            && comp != null)
            newComp.AbsorbedDNA = comp.AbsorbedDNA;

        RaiseNetworkEvent(new LoadActionsEvent(GetNetEntity(uid)), newEnt);

        return newUid;
    }

    private bool TryTransform(EntityUid target, ChangelingIdentityComponent comp, bool sting = false, bool persistentDna = false)
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

        EntityUid? newUid;
        if (sting)
            newUid = TransformEntity(target, data: data);
        else
        {
            comp.IsInLesserForm = false;
            newUid = TransformEntity(target, data: data, comp: comp);
            RemoveAllChangelingEquipment((target, comp));
        }

        if (newUid != null)
            PlayMeatySound(newUid.Value);

        return true;
    }

    private void RemoveAllChangelingEquipment(Entity<ChangelingIdentityComponent> ent)
    {
        // check if there's no entities or all entities are null
        if (ent.Comp.Equipment.Values.Count == 0
        || ent.Comp.Equipment.Values.All(eq => eq == null))
            return;

        foreach (var equip in ent.Comp.Equipment.Values)
            QueueDel(equip);

        PlayMeatySound(ent);
    }

    #endregion

    #region Event Handlers

    private void OnIdentityMapInit(Entity<ChangelingIdentityComponent> ent, ref MapInitEvent args)
    {
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        RemComp<CanHostGuardianComponent>(ent);
        RemComp<MartialArtsKnowledgeComponent>(ent);
        RemComp<CanPerformComboComponent>(ent);
        EnsureComp<ZombieImmuneComponent>(ent);

        // add actions
        foreach (var actionId in ent.Comp.BaseChangelingActions)
            _actions.AddAction(ent, actionId);

        // make sure its set to the default
        ent.Comp.TotalEvolutionPoints = _changelingRuleSystem.StartingCurrency;

        // don't want instant stasis
        ent.Comp.StasisTime = ent.Comp.DefaultStasisTime;

        // make their blood unreal
        _blood.ChangeBloodReagent(ent.Owner, "BloodChangeling");
    }

    // in the future ChangelingIdentity should have its own system and be ONLY used for holding stored DNA and handling transformations.
    private void OnChangelingMapInit(Entity<ChangelingComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.EvolutionsAssigned) // this is solely because polymorph will cause mega errors otherwise
            return;

        if (!_proto.TryIndex(ent.Comp.EvolutionsProto, out var evoProto))
            return;

        foreach (var startingComp in evoProto.Components)
        {
            var startCompType = startingComp.Value.Component.GetType();
            var startComp = Factory.GetComponent(startCompType);
            if (!HasComp(ent, startCompType)) // don't overwrite the starting components if you already have them (somehow)
                AddComp(ent, startComp, true);
        }

        ent.Comp.EvolutionsAssigned = true;
    }

    private void OnMobStateChange(Entity<ChangelingIdentityComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemoveAllChangelingEquipment(ent);
    }

    private static void OnUpdateMobState(Entity<ChangelingIdentityComponent> ent, ref UpdateMobStateEvent args)
    {
        if (ent.Comp.IsInStasis)
            args.State = MobState.Dead;
    }

    private void OnDamageChange(Entity<ChangelingIdentityComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.IsInStasis
            || !_mobThreshold.TryGetThresholdForState(ent, MobState.Dead, out var maxThreshold)
            || !_mobThreshold.TryGetThresholdForState(ent, MobState.Critical, out var critThreshold))
            return;

        var lowestStasisTime = ent.Comp.DefaultStasisTime; // 15 sec
        var highestStasisTime = ent.Comp.MaxStasisTime; // 45 sec
        var catastrophicStasisTime = ent.Comp.CatastrophicStasisTime; // 1 min

        var damage = args.Damageable;
        var damageTaken = damage.TotalDamage;

        var damageScaled = float.Round((float) (damageTaken / critThreshold.Value * highestStasisTime));

        var damageToTime = MathF.Min(damageScaled, highestStasisTime);
        var newStasisTime = MathF.Max(lowestStasisTime, damageToTime);

        if (damageTaken < maxThreshold)
            ent.Comp.StasisTime = newStasisTime;
        else
            ent.Comp.StasisTime = catastrophicStasisTime;
    }

    private void OnComponentRemove(Entity<ChangelingIdentityComponent> ent, ref ComponentRemove args)
    {
        RemoveAllChangelingEquipment(ent);
    }

    private void OnDefibZap(Entity<ChangelingIdentityComponent> ent, ref TargetBeforeDefibrillatorZapsEvent args)
    {
        if (!ent.Comp.IsInStasis) // so you don't get a free insta-rejuvenate after being defibbed
            return;
        ent.Comp.IsInStasis = false;
        _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-defib"), ent, ent);
    }

    // triggered by leaving stasis and by admin rejuvenate
    private void OnRejuvenate(Entity<ChangelingIdentityComponent> ent, ref RejuvenateEvent args)
    {
        ent.Comp.IsInStasis = false;
        ent.Comp.StasisTime = ent.Comp.DefaultStasisTime;

        _mobState.UpdateMobState(ent);
    }

    private void OnPolymorphed(Entity<ChangelingIdentityComponent> ent, ref PolymorphedEvent args)
        => _polymorph.CopyPolymorphComponent<ChangelingIdentityComponent>(ent, args.NewEntity);

    private void OnPolymorphedTakeTwo(Entity<ChangelingComponent> ent, ref PolymorphedEvent args)
        => _polymorph.CopyPolymorphComponent<ChangelingComponent>(ent, args.NewEntity);

    private static void OnLimbAmputation(Entity<ChangelingComponent> ent, ref BeforeAmputationDamageEvent args)
        => args.Cancelled = true;

    private static void OnGetAntagBlocker(Entity<ChangelingComponent> ent, ref GetAntagSelectionBlockerEvent args)
        => args.IsChangeling = true;

    private static void OnMindswapAttempt(Entity<ChangelingComponent> ent, ref BeforeMindSwappedEvent args)
    {
        args.Message = ent.Comp.MindswapText;
        args.Cancelled = true;
    }

    private static void OnConversionAttempt(Entity<ChangelingComponent> ent, ref BeforeConversionEvent args)
        => args.Blocked = true;

    // stop the changeling from losing control over the body
    private static void OnBrainRemoveAttempt(Entity<ChangelingComponent> ent, ref BeforeBrainRemovedEvent args)
        => args.Blocked = true;

    private static void OnBrainAddAttempt(Entity<ChangelingComponent> ent, ref BeforeBrainAddedEvent args)
        => args.Blocked = true;

    private void OnDartHit(Entity<ChangelingDartComponent> ent, ref ProjectileHitEvent args)
    {
        if (HasComp<ChangelingIdentityComponent>(args.Target)
            || ent.Comp.ReagentDivisor <= 0
            || !_proto.TryIndex(ent.Comp.StingConfiguration, out var configuration))
            return;

        TryInjectReagents(args.Target,
            configuration.Reagents.Select(x => (x.Key, x.Value / ent.Comp.ReagentDivisor)).ToDictionary());
    }
    #endregion
}
