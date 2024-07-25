using Content.Server.Flash.Components;
using Content.Server.Light.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Changeling;
using Content.Shared.Changeling.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stealth.Components;
using Content.Shared.Store.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Server.Changeling.EntitySystems;

// ability custom behavior event processing
public sealed partial class ChangelingSystem : EntitySystem
{
    private void SubscribeCustomBehavior()
    {
        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAEvent>(OnAbsorb);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);
        SubscribeLocalEvent<ChangelingComponent, StingExtractDNAEvent>(OnStingExtractDNA);
        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformCycleEvent>(OnTransformCycle);
        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformEvent>(OnTransform);
        SubscribeLocalEvent<ChangelingComponent, EnterStasisEvent>(OnEnterStasis);
        SubscribeLocalEvent<ChangelingComponent, ExitStasisEvent>(OnExitStasis);
        SubscribeLocalEvent<ChangelingComponent, ShriekDissonantEvent>(OnShriekDissonant);
        SubscribeLocalEvent<ChangelingComponent, ShriekResonantEvent>(OnShriekResonant);
        SubscribeLocalEvent<ChangelingComponent, ToggleStrainedMusclesEvent>(OnToggleStrainedMuscles);
        SubscribeLocalEvent<ChangelingComponent, StingBlindEvent>(OnStingBlind);
        SubscribeLocalEvent<ChangelingComponent, StingTransformEvent>(OnStingTransform);
        SubscribeLocalEvent<ChangelingComponent, StingFakeArmbladeEvent>(OnStingFakeArmblade);
        SubscribeLocalEvent<ChangelingComponent, ActionAugmentedEyesightEvent>(OnAugmentedEyesight);
        SubscribeLocalEvent<ChangelingComponent, ActionBiodegradeEvent>(OnBiodegrade);
        SubscribeLocalEvent<ChangelingComponent, ActionChameleonSkinEvent>(OnChameleonSkin);
        SubscribeLocalEvent<ChangelingComponent, ActionLesserFormEvent>(OnLesserForm);
        SubscribeLocalEvent<ChangelingComponent, ActionHivemindAccessEvent>(OnHivemindAccess);

        // add your behavior here
    }

    private void OnOpenEvolutionMenu(Entity<ChangelingComponent> ent, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
    }
    private void OnAbsorb(Entity<ChangelingComponent> ent, ref AbsorbDNAEvent args)
    {
        var target = args.Target;

        if (!(_mobState.IsIncapacitated(target) || TryComp<CuffableComponent>(target, out var cuffs) && cuffs.CuffedHandCount > 0))
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

        if (args.Cancelled || HasComp<AbsorbedComponent>(target))
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

    private ChangelingActionBehaviorShriekComponent? DoShriek(EntityUid actionUid, Entity<ChangelingComponent> ent)
    {
        if (!TryComp<ChangelingActionBehaviorShriekComponent>(actionUid, out var action))
            return null;

        _audio.PlayPvs(action.ShriekSound, ent);

        var center = Transform(ent).MapPosition;
        var gamers = Filter.Empty();
        gamers.AddInRange(center, action.Power, _player, EntityManager);

        foreach (var gamer in gamers.Recipients)
        {
            if (gamer.AttachedEntity == null)
                continue;

            var pos = Transform(gamer.AttachedEntity!.Value).WorldPosition;
            var delta = center.Position - pos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(.01f, 0);

            _recoil.KickCamera((EntityUid) gamer.AttachedEntity, -delta.Normalized());
        }

        return action;
    }
    private void OnShriekDissonant(Entity<ChangelingComponent> ent, ref ShriekDissonantEvent args)
    {
        var shriek = DoShriek(args.Action, ent);
        if (shriek == null)
            return;

        var pos = _transform.GetMapCoordinates(ent);
        var power = shriek.Power;
        _emp.EmpPulse(pos, power, 5000f, power * 2);
    }
    private void OnShriekResonant(Entity<ChangelingComponent> ent, ref ShriekResonantEvent args)
    {
        var shriek = DoShriek(args.Action, ent);
        if (shriek == null)
            return;

        // kill lights and stun people
        var lookup = _lookup.GetEntitiesInRange(ent, shriek.Power);
        var lights = GetEntityQuery<PoweredLightComponent>();
        var people = GetEntityQuery<StatusEffectsComponent>();

        foreach (var target in lookup)
        {
            if (target == ent.Owner)
                continue;

            if (people.HasComp(target))
            {
                _stun.TryParalyze(target, TimeSpan.FromSeconds(shriek.Power / 1.5f), true);
                _stun.TrySlowdown(target, TimeSpan.FromSeconds(shriek.Power * 2f), true, 0.8f, 0.8f);
            }

            if (lights.HasComponent(target))
                _light.TryDestroyBulb(target);
        }
    }

    private void OnToggleStrainedMuscles(Entity<ChangelingComponent> ent, ref ToggleStrainedMusclesEvent args)
    {
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

    private void OnStingBlind(Entity<ChangelingComponent> ent, ref StingBlindEvent args)
    {
        var target = args.Target;
        if (!TryComp<BlindableComponent>(target, out var blindable) || blindable.IsBlind)
            return;

        _blindable.AdjustEyeDamage((target, blindable), 2);
        var timeSpan = TimeSpan.FromSeconds(5f);
        _statusEffect.TryAddStatusEffect(target, TemporaryBlindnessSystem.BlindingStatusEffect, timeSpan, false, TemporaryBlindnessSystem.BlindingStatusEffect);
    }
    private void OnStingTransform(Entity<ChangelingComponent> ent, ref StingTransformEvent args)
    {
        var target = args.Target;
        if (!TryTransform(target, ent.Comp, true, true))
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
    }
    private void OnStingFakeArmblade(Entity<ChangelingComponent> ent, ref StingFakeArmbladeEvent args)
    {
        var target = args.Target;
        var fakeArmblade = EntityManager.SpawnEntity(ent.Comp.FakeArmbladePrototype, Transform(target).Coordinates);
        if (!_hands.TryPickupAnyHand(target, fakeArmblade))
        {
            QueueDel(fakeArmblade);
            ent.Comp.Chemicals += Comp<ChangelingActionComponent>(args.Action).ChemicalCost;
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-simplemob"), ent, ent);
            return;
        }

        PlayMeatySound(target, ent.Comp);
    }

    public void OnAugmentedEyesight(Entity<ChangelingComponent> ent, ref ActionAugmentedEyesightEvent args)
    {
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
    public void OnLesserForm(Entity<ChangelingComponent> ent, ref ActionLesserFormEvent args)
    {
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
    public void OnHivemindAccess(Entity<ChangelingComponent> ent, ref ActionHivemindAccessEvent args)
    {
        if (HasComp<HivemindComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("changeling-passive-active"), ent, ent);
            return;
        }

        EnsureComp<HivemindComponent>(ent);
        _popup.PopupEntity(Loc.GetString("changeling-hivemind-start"), ent, ent);
    }
}
