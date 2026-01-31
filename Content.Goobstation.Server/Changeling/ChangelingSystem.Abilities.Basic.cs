using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.Changeling.Objectives.Components;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Devour.Events;
using Content.Goobstation.Shared.InternalResources.Components;
using Content.Server.Nutrition.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Store.Components;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingSystem
{
    private void SubscribeBasicAbilities()
    {
        SubscribeLocalEvent<ChangelingIdentityComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbDNAEvent>(OnAbsorb);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingExtractDNAEvent>(OnStingExtractDNA);
        SubscribeLocalEvent<ChangelingIdentityComponent, ChangelingTransformCycleEvent>(OnTransformCycle);
        SubscribeLocalEvent<ChangelingIdentityComponent, ChangelingTransformEvent>(OnTransform);
        SubscribeLocalEvent<ChangelingIdentityComponent, EnterStasisEvent>(OnEnterStasis);
        SubscribeLocalEvent<ChangelingIdentityComponent, ExitStasisEvent>(OnExitStasis);
    }

    private void OnOpenEvolutionMenu(EntityUid uid, ChangelingIdentityComponent comp, ref OpenEvolutionMenuEvent args)
    {
        if (args.Handled || !TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);

        args.Handled = true;
    }

    private void OnAbsorb(Entity<ChangelingIdentityComponent> ent, ref AbsorbDNAEvent args)
    {
        var target = args.Target;

        if (args.Handled || CanAbsorb(ent, target))
            return;

        var popupOthers = Loc.GetString("changeling-absorb-start",
            ("user", Identity.Entity(ent, EntityManager)),
            ("target", Identity.Entity(target, EntityManager)));
        _popup.PopupEntity(popupOthers, ent, PopupType.LargeCaution);
        PlayMeatySound(ent);

        var dargs = new DoAfterArgs(EntityManager,
            ent,
            TimeSpan.FromSeconds(15),
            new AbsorbDNADoAfterEvent(),
            ent,
            target)
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

        args.Handled = true;
    }

    private bool CanAbsorb(Entity<ChangelingIdentityComponent> ent, EntityUid target)
    {
        if (HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-absorbed"), ent, ent);
            return false;
        }
        if (!HasComp<AbsorbableComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), ent, ent);
            return false;
        }
        if (!IsIncapacitated(target) && !IsHardGrabbed(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-nograb"), ent, ent);
            return false;
        }
        if (CheckFireStatus(target)) // checks if the target is on fire
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-onfire"), ent, ent);
            return false;
        }
        return true;
    }

    // holy shit method
    private void OnAbsorbDoAfter(Entity<ChangelingIdentityComponent> ent, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Args.Target == null
            || args.Cancelled
            || HasComp<AbsorbedComponent>(args.Args.Target)
            || !IsIncapacitated(args.Args.Target.Value)
            || !TryComp<ChangelingChemicalComponent>(ent, out var chemComp)
            || TryComp<ChangelingBiomassComponent>(ent, out var bioComp)
            && !IsHardGrabbed(args.Args.Target.Value))
            return;
        var target = args.Args.Target.Value;

        AbsorbApplyDamageAndPlayAudio(ent, target);
        AbsorbPayoutAndStealDNA(ent, args, target, bioComp, out var bonusChemicals, out var biomassMaxIncrease, out var biomassValid);
        AbsorbUpdateObjectiveStatus(ent, target);
        AbsorbUpdateChemicalsAndBiomass(ent, chemComp, bonusChemicals, bioComp, biomassValid, biomassMaxIncrease);
    }

    private void AbsorbUpdateChemicalsAndBiomass(Entity<ChangelingIdentityComponent> ent,
        ChangelingChemicalComponent chemComp,
        float bonusChemicals,
        ChangelingBiomassComponent? bioComp,
        bool biomassValid,
        float biomassMaxIncrease)
    {
        if (chemComp is { ResourceData: not null })
        {
            _resources.TryUpdateResourcesCapacity(ent, chemComp.ResourceData, bonusChemicals);
            UpdateChemicals((ent, ent.Comp), chemComp.ResourceData.MaxAmount, chemComp); // refill chems to max
        }

        // modify biomass if the changeling uses it
        if (bioComp is not { ResourceData: not null }
            || !biomassValid)
            return;
        _resources.TryUpdateResourcesCapacity(ent, bioComp.ResourceData, biomassMaxIncrease);
        UpdateBiomass((ent, ent.Comp), bioComp.ResourceData.MaxAmount, bioComp);
    }

    private void AbsorbUpdateObjectiveStatus(Entity<ChangelingIdentityComponent> ent, EntityUid target)
    {
        if (!_mind.TryGetMind(ent, out var mindId, out var mind))
            return;
        if (_mind.TryGetObjectiveComp<AbsorbConditionComponent>(mindId, out var absorbObj, mind)
            && !HasComp<PartialAbsorbableComponent>(target))
            absorbObj.Absorbed += 1;

        if (_mind.TryGetObjectiveComp<AbsorbChangelingConditionComponent>(mindId, out var lingAbsorbObj, mind)
            && TryComp<ChangelingIdentityComponent>(target, out var absorbed))
            lingAbsorbObj.LingAbsorbed += absorbed.TotalChangelingsAbsorbed + 1;
    }

    private void AbsorbPayoutAndStealDNA(Entity<ChangelingIdentityComponent> ent,
        AbsorbDNADoAfterEvent args,
        EntityUid target,
        ChangelingBiomassComponent? bioComp,
        out float bonusChemicals,
        out float biomassMaxIncrease,
        out bool biomassValid)
    {
        var popup = string.Empty;
        bonusChemicals = 0f;
        var bonusEvolutionPoints = 0f;
        var bonusChangelingAbsorbs = 0;
        biomassMaxIncrease = 0f;
        biomassValid = false;
        if (TryComp<ChangelingIdentityComponent>(target, out var targetComp))
        {
            if (TryComp<ChangelingChemicalComponent>(target, out var targetChemComp)
                && targetChemComp.ResourceData != null)
            {
                popup = Loc.GetString("changeling-absorb-end-self-ling");
                bonusChemicals += targetChemComp.ResourceData.MaxAmount / 2;
                bonusEvolutionPoints += targetComp.TotalEvolutionPoints / 2;
                bonusChangelingAbsorbs += targetComp.TotalChangelingsAbsorbed + 1;
            }

            biomassValid = true;

            if (bioComp is { ResourceData: not null })
                biomassMaxIncrease = bioComp.ResourceData.MaxAmount / 2;

            if (!TryComp<HumanoidAppearanceComponent>(target, out var targetForm)
                || targetForm.Species == "Monkey") // if they are a headslug or in monkey form
                popup = Loc.GetString("changeling-absorb-end-self-ling-incompatible");
        }
        else if (!HasComp<PartialAbsorbableComponent>(target))
        {
            popup = Loc.GetString("changeling-absorb-end-self");
            bonusChemicals += 10;
            bonusEvolutionPoints += 2;

            biomassValid = true;
        }
        else
            popup = Loc.GetString("changeling-absorb-end-partial");

        ent.Comp.TotalEvolutionPoints += bonusEvolutionPoints;

        var objBool = !HasComp<PartialAbsorbableComponent>(target);
        if (objBool)
        {
            ent.Comp.TotalAbsorbedEntities++;
            ent.Comp.TotalChangelingsAbsorbed += bonusChangelingAbsorbs;
        }

        TryStealDNA(ent, target, objBool);

        _popup.PopupEntity(popup, args.User, args.User);

        if (!TryComp<StoreComponent>(args.User, out var store))
            return;
        _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "EvolutionPoint", bonusEvolutionPoints } },
            args.User,
            store);
        _store.UpdateUserInterface(args.User, args.User, store);
    }

    private void AbsorbApplyDamageAndPlayAudio(EntityUid user, EntityUid target)
    {
        var dmg = new DamageSpecifier(_proto.Index(_absorbedDamageGroup), 200);

        _damage.TryChangeDamage(target, dmg, true, false, targetPart: TargetBodyPart.All); // Shitmed Change
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);
        PlayMeatySound(user);

        EnsureComp<AbsorbedComponent>(target);
        EnsureComp<UnrevivableComponent>(target);
    }

    private void OnAbsorbBiomatter(Entity<ChangelingIdentityComponent> ent, ref AbsorbBiomatterEvent args)
    {
        var target = args.Target;

        if (args.Handled
            || !TryComp<FoodComponent>(target, out var food)
            || !TryComp<SolutionContainerManagerComponent>(target, out var solMan))
            return;
        var totalFood = FixedPoint2.New(0);

        foreach (var (_, sol) in _solution.EnumerateSolutions((target, solMan)))
            foreach (var proto in _biomassAbsorbedChemicals)
                totalFood += sol.Comp.Solution.GetTotalPrototypeQuantity(proto);

        if (food.RequiresSpecialDigestion ||
            totalFood == 0) // no eating winter coats or food that won't give you anything
        {
            var popup = Loc.GetString("changeling-absorbbiomatter-bad-food");
            _popup.PopupEntity(popup, ent, ent);
            return;
        }

        var popupOthers =
            Loc.GetString("changeling-absorbbiomatter-start", ("user", Identity.Entity(ent, EntityManager)));
        _popup.PopupEntity(popupOthers, ent, PopupType.MediumCaution);
        PlayMeatySound(ent);
        // so you can't just instantly mukbang a bag of food mid-combat, 2.7s for raw meat
        var dargs = new DoAfterArgs(EntityManager,
            ent,
            TimeSpan.FromSeconds(totalFood.Float() * 0.15f),
            new AbsorbBiomatterDoAfterEvent(),
            ent,
            target)
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

        args.Handled = true;
    }

    private void OnAbsorbBiomatterDoAfter(EntityUid uid,
        ChangelingIdentityComponent comp,
        ref AbsorbBiomatterDoAfterEvent args)
    {
        if (args.Args.Target == null
            || args.Cancelled
            || !TryComp<SolutionContainerManagerComponent>(args.Args.Target, out var solMan))
            return;

        var target = args.Args.Target.Value;
        var totalFood = FixedPoint2.New(0);

        foreach (var (_, sol) in _solution.EnumerateSolutions((target, solMan)))
        {
            var solution = sol.Comp.Solution;
            foreach (var proto in _biomassAbsorbedChemicals)
            {
                var quant = solution.GetTotalPrototypeQuantity(proto);
                totalFood += quant;
                solution.RemoveReagent(proto, quant);
            }
            _puddle.TrySpillAt(target, solution, out _);
        }

        UpdateChemicals((uid, comp), totalFood.Float() * 2); // 36 for raw meat
        QueueDel(target); // eaten
    }

    private void OnStingExtractDNA(Entity<ChangelingIdentityComponent> ent, ref StingExtractDNAEvent args)
    {
        if (args.Handled
            || !TrySting(ent, args, true))
            return;

        var target = args.Target;
        var objBool = !HasComp<PartialAbsorbableComponent>(target);
        var targetIdentity = Identity.Entity(target, EntityManager);
        var locString = Loc.GetString("changeling-sting", ("target", targetIdentity));

        if (!TryStealDNA(ent, target, objBool))
            UpdateChemicals(ent, Comp<InternalResourcesActionComponent>(args.Action).UseAmount); // royal cashback
        else
            _popup.PopupEntity(locString, ent, ent);

        args.Handled = true;
    }

    private void OnTransformCycle(EntityUid uid,
        ChangelingIdentityComponent comp,
        ref ChangelingTransformCycleEvent args)
    {
        if (args.Handled)
            return;

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

        args.Handled = true;
    }

    private void OnTransform(EntityUid uid, ChangelingIdentityComponent comp, ref ChangelingTransformEvent args)
    {
        if (args.Handled || !TryTransform(uid, comp))
            UpdateChemicals((uid, comp), Comp<InternalResourcesActionComponent>(args.Action).UseAmount);

        args.Handled = true;
    }

    private void OnEnterStasis(EntityUid uid, ChangelingIdentityComponent comp, ref EnterStasisEvent args)
    {
        if (args.Handled)
            return;

        if (comp.IsInStasis || HasComp<AbsorbedComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter-fail"), uid, uid);
            return;
        }

        if (_mobState.IsAlive(uid))
        {
            // fake our death
            var othersMessage = Loc.GetString("suicide-command-default-text-others", ("name", uid));
            _popup.PopupEntity(othersMessage, uid, Filter.PvsExcept(uid), true);
        }

        var currentTime = comp.StasisTime;
        var lowestTime = comp.DefaultStasisTime;
        var highestTime = comp.CatastrophicStasisTime;

        if (currentTime == lowestTime) // mm les than ideal
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter"), uid, uid);
        else if (currentTime > lowestTime && currentTime < highestTime)
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter-damaged"), uid, uid);
        else
            _popup.PopupEntity(Loc.GetString("changeling-stasis-enter-catastrophic"), uid, uid);

        if (!_mobState.IsDead(uid))
            _mobState.ChangeMobState(uid, MobState.Dead);

        comp.IsInStasis = true;

        args.Handled = true;
    }

    private void OnExitStasis(EntityUid uid, ChangelingIdentityComponent comp, ref ExitStasisEvent args)
    {
        if (args.Handled)
            return;

        // check if we're allowed to revive
        var reviveEv = new BeforeSelfRevivalEvent(uid, "self-revive-fail");
        RaiseLocalEvent(uid, ref reviveEv);

        if (reviveEv.Cancelled)
            return;

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

        if (comp.StasisTime > 0)
        {
            _popup.PopupEntity(Loc.GetString("changeling-stasis-exit-fail-time"), uid, uid);
            return;
        }

        if (!TryComp<DamageableComponent>(uid, out _))
            return;
        // heal of everything
        var stasisEv = new RejuvenateEvent(false);
        RaiseLocalEvent(uid, stasisEv);

        _popup.PopupEntity(Loc.GetString("changeling-stasis-exit"), uid, uid);

        // stuns or knocks down anybody grabbing you
        if (_pull.IsPulled(uid))
        {
            var puller = Comp<PullableComponent>(uid).Puller;
            if (puller != null)
            {
                _stun.KnockdownOrStun(puller.Value, TimeSpan.FromSeconds(1), true);
            }
        }

        args.Handled = true;
    }

}
