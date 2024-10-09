using Content.Server._Goobstation.Objectives.Components;
using Content.Server.Administration.Systems;
using Content.Server.Body.Systems;
using Content.Server.Mind;
using Content.Server.Store.Systems;
using Content.Shared._Goobstation.Changeling;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Store.Components;

namespace Content.Server._Goobstation.Changeling.EntitySystems;

public sealed partial class ChangelingAbilitiesSystem : SharedChangelingAbilitiesSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly ChangelingSystem _changelingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);
        SubscribeLocalEvent<ChangelingComponent, ToggleChangelingStasisEvent>(OnStasisEvent);
    }

    /// <summary>
    ///     Opens ling's store. Maybe move it to click on biomass/chemicals alerts?
    /// </summary>
    private void OnOpenEvolutionMenu(Entity<ChangelingComponent> changeling, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(changeling, out var store))
            return;

        _storeSystem.ToggleUi(changeling, changeling, store);
    }

    /// <summary>
    ///     Starts absorbtion doAfter. You won't be able to eat absorbed or not incapacitated
    /// </summary>
    /// <param name="changeling"></param>
    /// <param name="args"></param>
    private void OnAbsorbDoAfter(Entity<ChangelingComponent> changeling, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Target == null)
            return;

        var target = args.Target.Value;
        var comp = changeling.Comp;

        if (args.Cancelled || CanInteract(target) || !TryComp<AbsorbableComponent>(target, out var absorbable))
            return;

        _changelingSystem.PlayMeatySound(changeling);

        _changelingSystem.UpdateBiomass(changeling, comp.MaxBiomass - comp.TotalAbsorbedEntities);

        var dmg = new DamageSpecifier(ProtoManager.Index(absorbable.AbsorbedDamageGroup), 200);
        _damageableSystem.TryChangeDamage(target, dmg, false, false);

        absorbable.Absorbed = true;

        var popup = Loc.GetString("changeling-absorb-end-self-ling");
        var bonusChemicals = 0f;
        var bonusEvolutionPoints = 0f;

        if (TryComp<ChangelingComponent>(target, out var targetComp))
        {
            bonusChemicals += targetComp.MaxChemicals / 2;
            bonusEvolutionPoints += 10;
            comp.MaxBiomass += targetComp.MaxBiomass / 2;
        }
        else
        {
            popup = Loc.GetString("changeling-absorb-end-self");
            bonusChemicals += 10;
            comp.MaxBiomass += 2;
            bonusEvolutionPoints += 2;
        }

        comp.TotalAbsorbedEntities++;
        comp.MaxChemicals += bonusChemicals;
        Dirty(changeling);

        PopupSystem.PopupEntity(popup, changeling, changeling);

        _changelingSystem.TryStealHumanoidData(changeling, target);

        if (TryComp<StoreComponent>(args.User, out var store))
        {
            _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "EvolutionPoint", bonusEvolutionPoints } }, args.User, store);
            _storeSystem.UpdateUserInterface(args.User, args.User, store);
        }

        _bloodstreamSystem.ChangeBloodReagent(target, absorbable.AbsorbedBloodType);
        _bloodstreamSystem.SpillAllSolutions(target);

        if (!_mindSystem.TryGetMind(changeling, out var mindId, out var mind))
            return;

        if (_mindSystem.TryGetObjectiveComp<AbsorbConditionComponent>(mindId, out var objective, mind))
            objective.Absorbed += 1;
    }

    /// <summary>
    ///     Toggles changeling's stasis by clicking on stasis alert
    /// </summary>
    private void OnStasisEvent(Entity<ChangelingComponent> changeling, ref ToggleChangelingStasisEvent args)
    {
        var comp = changeling.Comp;

        // Can't use stasis on 0 biomass or if absorbed
        if (TryComp<AbsorbableComponent>(changeling, out var absorbable) && absorbable.Absorbed || comp.Biomass <= 0)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-stasis-toggle-fail"), changeling, changeling);
            return;
        }

        if (comp.InStasis)
            PullOutFromStasis(changeling);
        else
            PutIntoStasis(changeling);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changeling"></param>
    private void PutIntoStasis(Entity<ChangelingComponent> changeling)
    {
        var comp = changeling.Comp;

        comp.Chemicals = 0;

        if (_mobStateSystem.IsAlive(changeling))
        {
            // Fake death for other players
            var othersMessage = Loc.GetString("suicide-command-default-text-others", ("name", changeling));
            PopupSystem.PopupPredicted(othersMessage, changeling, changeling);

            var selfMessage = Loc.GetString("changeling-stasis-enter");
            PopupSystem.PopupEntity(selfMessage, changeling, changeling);
        }

        if (!_mobStateSystem.IsDead(changeling))
            _mobStateSystem.ChangeMobState(changeling, MobState.Dead);

        comp.InStasis = true;
        Dirty(changeling);

        _alertsSystem.ShowAlert(changeling, comp.StasisAlert, _changelingSystem.GetStasisAlertSeverity(comp));
    }

    /// <summary>
    ///     Removes stasis effect from changeling
    /// </summary>
    private void PullOutFromStasis(Entity<ChangelingComponent> changeling)
    {
        var comp = changeling.Comp;

        if (comp.Chemicals < comp.MaxChemicals / 2)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), changeling, changeling);
            return;
        }

        _rejuvenateSystem.PerformRejuvenate(changeling);
        PopupSystem.PopupEntity(Loc.GetString("changeling-stasis-exit"), changeling, changeling);

        comp.InStasis = false;
        _alertsSystem.ShowAlert(changeling, comp.StasisAlert, _changelingSystem.GetStasisAlertSeverity(changeling.Comp));

        Dirty(changeling);
    }
}
