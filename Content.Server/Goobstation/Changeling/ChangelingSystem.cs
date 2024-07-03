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
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Camera;
using Robust.Shared.Map;
using Robust.Shared.Player;
using System.Numerics;
using Robust.Server.Player;
using Content.Server.Flash;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;

namespace Content.Server.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    // this is one hell of a star wars intro text
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
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly EmpSystem _emp = default!;

    [Dependency] private readonly SharedHandsSystem _hands = default!;

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
        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformDoAfterEvent>(OnTransformDoAfter);
        SubscribeLocalEvent<ChangelingComponent, EnterStasisEvent>(OnEnterStasis);
        SubscribeLocalEvent<ChangelingComponent, ExitStasisEvent>(OnExitStasis);

        SubscribeLocalEvent<ChangelingComponent, ToggleArmbladeEvent>(OnToggleArmblade);
        SubscribeLocalEvent<ChangelingComponent, CreateBoneShardEvent>(OnCreateBoneShard);
        SubscribeLocalEvent<ChangelingComponent, ToggleChitinousArmorEvent>(OnToggleArmor);
        SubscribeLocalEvent<ChangelingComponent, ToggleOrganicShieldEvent>(OnToggleShield);
        SubscribeLocalEvent<ChangelingComponent, ShriekDissonantEvent>(OnShriekDissonant);
        SubscribeLocalEvent<ChangelingComponent, ShriekResonantEvent>(OnShriekResonant);
        SubscribeLocalEvent<ChangelingComponent, ToggleStrainedMusclesEvent>(OnToggleStrainedMuscles);
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

    #region Helper Methods

    private void CameraShake(float range, MapCoordinates epicenter, float totalIntensity)
    {
        var players = Filter.Empty();
        players.AddInRange(epicenter, range, _playerMan, EntityManager);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid uid)
                continue;

            var playerPos = _transform.GetGridOrMapTilePosition(player.AttachedEntity!.Value);
            var delta = epicenter.Position - playerPos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(0.01f, 0);

            var distance = delta.Length();
            var effect = 5 * MathF.Pow(totalIntensity, 0.5f) * (1 - distance / range);
            if (effect > 0.01f)
                _recoil.KickCamera(uid, -delta.Normalized() * effect);
        }
    }

    public bool IsInLesserForm(EntityUid uid, ChangelingComponent comp)
    {
        // todo: check lesser form
        return false;
    }

    public void PlayMeatySound(EntityUid uid, ChangelingComponent comp)
    {
        var rand = _rand.Next(0, comp.SoundPool.Count - 1);
        var sound = comp.SoundPool.ToArray()[rand];
        _audio.PlayPvs(sound, uid, AudioParams.Default.WithVolume(-3f));
    }
    public void PlayShriekSound(EntityUid uid, ChangelingComponent comp)
    {
        var pos = Transform(uid).MapPosition;
        _audio.PlayPvs(comp.ShriekSound, uid);
        CameraShake(comp.ShriekPower, pos, 100);
    }

    /// <summary>
    ///     Check if a target is crit/dead or cuffed. For absorbing.
    /// </summary>
    public bool IsIncapacitated(EntityUid uid)
    {
        if (_mobState.IsIncapacitated(uid)
        || (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.CuffedHandCount > 0))
            return true;

        return false;
    }

    private void UpdateChemicals(EntityUid uid, ChangelingComponent comp, float? amount = 1)
    {
        var regen = Math.Abs(1 * (1 + comp.ChemicalRegenerationModifier));
        var chemicals = comp.Chemicals + amount ?? regen;

        comp.Chemicals = Math.Clamp(chemicals, 0, comp.MaxChemicals);

        Dirty(uid, comp);

        _alerts.ShowAlert(uid, "Chemicals");
    }
    public void UpdateModifier(ChangelingComponent comp)
    {
        var modifier = comp.ChemicalRegenerationMobStateModifier;
        comp.ChemicalRegenerationModifier = modifier;
    }

    public bool TryUseAbility(EntityUid uid, ChangelingComponent comp, BaseActionEvent action)
    {
        if (!TryComp<ChangelingActionComponent>(action.Action, out var lingAction))
            return false;

        if (!lingAction.UseWhileLesserForm && IsInLesserForm(uid, comp))
        {
            _popup.PopupEntity(Loc.GetString("changeling-action-fail-lesserform"), uid, uid);
            return false;
        }

        var price = lingAction.ChemicalCost;
        if (comp.Chemicals < price)
        {
            _popup.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), uid, uid);
            return false;
        }

        UpdateChemicals(uid, comp, -price);

        return true;
    }
    public bool TrySting(EntityUid uid, EntityUid target, ChangelingComponent comp, BaseActionEvent action)
    {
        if (!TryUseAbility(uid, comp, action))
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
    public bool TryReagentSting(EntityUid uid, EntityUid target, ChangelingComponent comp, BaseActionEvent action, string reagentId, FixedPoint2 reagentAmount)
    {
        if (!TrySting(uid, target, comp, action))
            return false;

        var solution = new Solution();
        solution.AddReagent(reagentId, reagentAmount);

        if (!_solution.TryGetInjectableSolution(target, out var targetSolution, out var _))
            return false;

        _solution.TryAddSolution(targetSolution.Value, solution);

        return true;
    }

    public void AddDNA(EntityUid uid, ChangelingComponent comp, TransformData data, bool countObjective = false)
    {
        if (comp.AbsorbedDNA.Count >= comp.MaxAbsorbedDNA)
        {
            comp.AbsorbedDNA.RemoveAt(0);
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-max"), uid, uid);
        }
        comp.AbsorbedDNA.Add(data);

        if (countObjective)
            comp.TotalStolenDNA += 1;
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
            Appearance = appearance
        };

        if (fingerprint.Fingerprint != null)
            data.Fingerprint = fingerprint.Fingerprint;

        AddDNA(uid, comp, data);

        return true;
    }

    private void Transform(EntityUid uid, ChangelingComponent comp, TransformData data)
    {
        if (!_proto.TryIndex<SpeciesPrototype>(data.Appearance.Species, out var species))
            return;

        // right now this is awful
        // todo: find a better way to transform
        var config = new PolymorphConfiguration()
        {
            Entity = species.Prototype,
            TransferDamage = true,
            Forced = true,
            Inventory = PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false
        };
        var newUid = _polymorph.PolymorphEntity(uid, config);

        if (newUid == null)
            return;

        // copy our stuff
        var lingCompCopy = _serialization.CreateCopy(comp, notNullableOverride: true);
        AddComp(newUid.Value, lingCompCopy, true);
        var newLingComp = Comp<ChangelingComponent>(newUid.Value);

        newLingComp.AbsorbedDNA = comp.AbsorbedDNA;
        newLingComp.AbsorbedDNA.Remove(data); // a one timer opportunity.

        if (TryComp<StoreComponent>(uid, out var storeComp))
        {
            var storeCompCopy = _serialization.CreateCopy(storeComp, notNullableOverride: true);
            RemComp<StoreComponent>(newUid.Value);
            EntityManager.AddComponent(newUid.Value, storeCompCopy);
        }

        Comp<FingerprintComponent>(newUid.Value).Fingerprint = data.Fingerprint;
        Comp<DnaComponent>(newUid.Value).DNA = data.DNA;
        _humanoid.CloneAppearance(data.Appearance.Owner, newUid.Value);
        _metaData.SetEntityName(newUid.Value, data.Name);

        var message = Loc.GetString("changeling-transform-finish", ("target", data.Name));
        _popup.PopupEntity(message, newUid.Value, newUid.Value);

        RemCompDeferred<ChangelingComponent>(uid);
        RemCompDeferred<PolymorphedEntityComponent>(newUid.Value);
        QueueDel(uid);
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

        if (comp.CurrentForm == null)
        {
            // steal DNA from ourselves. real.
            TryStealDNA(uid, uid, comp);
            var form = comp.AbsorbedDNA.ToArray()[0];
            comp.CurrentForm = form;
            comp.AbsorbedDNA.RemoveAt(0);
        }
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

    #region Basic Abilities

    private void OnOpenEvolutionMenu(EntityUid uid, ChangelingComponent comp, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnAbsorb(EntityUid uid, ChangelingComponent comp, ref AbsorbDNAEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
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
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    public ProtoId<DamageGroupPrototype> AbsorbedDamageGroup = "Genetic";
    private void OnAbsorbDoAfter(EntityUid uid, ChangelingComponent comp, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        PlayMeatySound(args.User, comp);

        if (args.Cancelled || !IsIncapacitated(target) || HasComp<AbsorbedComponent>(target))
            return;

        var dmg = new DamageSpecifier(_proto.Index(AbsorbedDamageGroup), 200);
        _damage.TryChangeDamage(target, dmg, false, false);
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        EnsureComp<AbsorbedComponent>(target);

        var popup = Loc.GetString("changeling-absorb-end-self-ling");
        var bonusChemicals = 10;
        var bonusEvolutionPoints = 0;
        if (HasComp<ChangelingComponent>(target))
        {
            bonusChemicals += 20;
            bonusEvolutionPoints += 5;
        }
        else
        {
            popup = Loc.GetString("changeling-absorb-end-self", ("target", Identity.Entity(target, EntityManager)));
            bonusChemicals += 10;
            TryStealDNA(args.User, target, comp);
            comp.TotalAbsorbedEntities++;
            comp.TotalStolenDNA++;
        }

        _popup.PopupEntity(popup, args.User, args.User);
        comp.MaxChemicals += bonusChemicals;

        if (TryComp<StoreComponent>(args.User, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "EvolutionPoint", bonusEvolutionPoints } }, args.User, store);
            _store.UpdateUserInterface(args.User, args.User, store);
        }
    }

    private void OnStingExtractDNA(EntityUid uid, ChangelingComponent comp, ref StingExtractDNAEvent args)
    {
        if (!TrySting(uid, args.Target, comp, args))
            return;

        if (TryStealDNA(uid, args.Target, comp))
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract", ("target", Identity.Entity(args.Target, EntityManager))), uid, uid);
        else
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-fail"), uid, uid);
    }

    private void OnTransformCycle(EntityUid uid, ChangelingComponent comp, ref ChangelingTransformCycleEvent args)
    {
        comp.AbsorbedDNAIndex += 1;
        if (comp.AbsorbedDNAIndex >= comp.MaxAbsorbedDNA || comp.AbsorbedDNAIndex >= comp.AbsorbedDNA.Count - 1)
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
    private void OnTransform(EntityUid uid, ChangelingComponent comp, ref ChangelingTransformEvent args)
    {
        if (comp.SelectedForm == null)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-self"), uid, uid);
            return;
        }
        if (comp.SelectedForm == comp.CurrentForm)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-choose"), uid, uid);
            return;
        }

        if (!TryUseAbility(uid, comp, args))
            return;

        PlayMeatySound(uid, comp);

        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(2.5), new ChangelingTransformDoAfterEvent(), uid, uid)
        {
            BreakOnDamage = false,
            BreakOnMove = false,
            BreakOnHandChange = false,
            BreakOnWeightlessMove = false,
        };
        var loc = Loc.GetString("changeling-transform-others", ("user", Identity.Entity(uid, EntityManager)));
        _popup.PopupEntity(loc, uid, PopupType.LargeCaution);
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnTransformDoAfter(EntityUid uid, ChangelingComponent comp, ref ChangelingTransformDoAfterEvent args)
    {
        PlayMeatySound(args.User, comp);
        if (comp.SelectedForm != null)
            Transform(uid, comp, (TransformData) comp.SelectedForm);
    }

    private void OnEnterStasis(EntityUid uid, ChangelingComponent comp, ref EnterStasisEvent args)
    {
        if (comp.IsInStasis)
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter-fail"), uid, uid);
            return;
        }

        if (!TryUseAbility(uid, comp, args))
            return;

        if (_mobState.IsAlive(uid))
        {
            // fake our death
            var othersMessage = Loc.GetString("suicide-command-default-text-others", ("name", uid));
            _popup.PopupEntity(othersMessage, uid, Robust.Shared.Player.Filter.PvsExcept(uid), true);

            var selfMessage = Loc.GetString("changeling-stasis-enter");
            _popup.PopupEntity(selfMessage, uid, uid);
        }

        if (!_mobState.IsDead(uid))
            _mobState.ChangeMobState(uid, MobState.Dead);

        // faster regen
        comp.ChemicalRegenerationModifier += 2;

        comp.IsInStasis = true;
    }
    private void OnExitStasis(EntityUid uid, ChangelingComponent comp, ref ExitStasisEvent args)
    {
        if (!comp.IsInStasis)
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-fail"), uid, uid);
            return;
        }

        if (!TryUseAbility(uid, comp, args))
            return;

        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return;

        // heal of everything
        _damage.SetAllDamage(uid, damageable, 0);
        _mobState.ChangeMobState(uid, MobState.Alive);
        _blood.TryModifyBloodLevel(uid, 1000);
        _blood.TryModifyBleedAmount(uid, -1000);

        _popup.PopupEntity(Loc.GetString("changeling-stasis-exit"), uid, uid);

        // slower regen
        comp.ChemicalRegenerationModifier -= 2;

        comp.IsInStasis = false;
    }

    #endregion



    #region Combat Abilities

    private void OnToggleArmblade(EntityUid uid, ChangelingComponent comp, ref ToggleArmbladeEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        if (comp.ArmbladeEntity == null)
        {
            comp.ChemicalRegenerationModifier -= .25f;

            var armblade = EntityManager.SpawnEntity(comp.ArmbladePrototype, Transform(uid).Coordinates);
            if (!_hands.TryForcePickupAnyHand(uid, armblade))
            {
                EntityManager.DeleteEntity(comp.ArmbladeEntity);
                _popup.PopupEntity(Loc.GetString("changeling-armblade-fail-hands"), uid, uid);
            }
            else
            {
                PlayMeatySound(uid, comp);
                _popup.PopupEntity(Loc.GetString("changeling-armblade-start"), uid, uid);
                comp.ArmbladeEntity = armblade;
            }
        }
        else
        {
            PlayMeatySound(uid, comp);
            _popup.PopupEntity(Loc.GetString("changeling-armblade-end"), uid, uid);
            comp.ChemicalRegenerationModifier += .25f;
            EntityManager.DeleteEntity(comp.ArmbladeEntity);
            comp.ArmbladeEntity = null;
        }
    }
    private void OnCreateBoneShard(EntityUid uid, ChangelingComponent comp, ref CreateBoneShardEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        PlayMeatySound(uid, comp);

        var star = EntityManager.SpawnEntity(comp.BoneShardPrototype, Transform(uid).Coordinates);
        _hands.TryPickupAnyHand(uid, star);
    }
    private void OnToggleArmor(EntityUid uid, ChangelingComponent comp, ref ToggleChitinousArmorEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;


    }
    private void OnToggleShield(EntityUid uid, ChangelingComponent comp, ref ToggleOrganicShieldEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;


    }
    private void OnShriekDissonant(EntityUid uid, ChangelingComponent comp, ref ShriekDissonantEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        PlayShriekSound(uid, comp);

        var pos = _transform.GetMapCoordinates(uid);
        var power = comp.ShriekPower;
        _emp.EmpPulse(pos, power, 5000f, power * 2);
    }
    private void OnShriekResonant(EntityUid uid, ChangelingComponent comp, ref ShriekResonantEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;

        PlayShriekSound(uid, comp);

        var pos = Transform(uid).MapPosition;
        var players = Filter.Empty();
        players.AddInRange(pos, comp.ShriekPower, _playerMan, EntityManager);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity == null)
                continue;

            var pid = player.AttachedEntity.Value;
            var power = comp.ShriekPower;

            _flash.Flash(pid, uid, null, power, power / 2, stunDuration: TimeSpan.FromSeconds(power / 2));
        }
    }
    private void OnToggleStrainedMuscles(EntityUid uid, ChangelingComponent comp, ref ToggleStrainedMusclesEvent args)
    {
        if (!TryUseAbility(uid, comp, args))
            return;


    }

    #endregion



    #region Stings



    #endregion



    #region Utilities



    #endregion
}
