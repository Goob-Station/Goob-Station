using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions.Events;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Changeling.EntitySystems;

public abstract partial class SharedChangelingAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    [Dependency] protected readonly IPrototypeManager ProtoManager = default!;
    [Dependency] protected readonly SharedChangelingSystem ChangelingSystem = default!;
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAEvent>(OnAbsorb);
        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnAbsorbDoAfter);

        SubscribeLocalEvent<ChangelingActionComponent, ActionAttemptEvent>(OnTryUseAbility);
    }

    /// <summary>
    ///     Trying to use changeling ability. If user don't have changeling component - it'll be cancelled
    /// </summary>
    public void OnTryUseAbility(Entity<ChangelingActionComponent> action, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var comp = action.Comp;
        var user = args.User;

        if (!TryComp<ChangelingComponent>(user, out var changelingComp))
            return;

        if (changelingComp.Biomass < comp.BiomassCost)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-biomass-deficit"), user);
            args.Cancelled = true;
        }

        if (changelingComp.FormType < comp.RequiredFormType)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-action-fail-lesserform"), user);
            args.Cancelled = true;
        }

        if (changelingComp.Chemicals < comp.ChemicalCost)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-chemicals-deficit"), user);
            args.Cancelled = true;
        }

        if (changelingComp.TotalAbsorbedEntities < comp.RequireAbsorbed)
        {
            var delta = comp.RequireAbsorbed - changelingComp.TotalAbsorbedEntities;
            PopupSystem.PopupClient(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), user);
            args.Cancelled = true;
        }

        ChangelingSystem.UpdateChemicals((user, changelingComp), -comp.ChemicalCost);
        ChangelingSystem.UpdateBiomass((user, changelingComp), -comp.BiomassCost);
    }

    private void OnAbsorb(Entity<ChangelingComponent> changeling, ref AbsorbDNAEvent args)
    {
        var target = args.Target;

        if (_actionBlockerSystem.CanInteract(target, null))
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-absorb-fail-incapacitated"), changeling);
            return;
        }
        if (!TryComp<AbsorbableComponent>(target, out var absorbable))
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-absorb-fail-unabsorbable"), changeling);
            return;
        }
        if (absorbable.Absorbed)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-absorb-fail-absorbed"), changeling);
            return;
        }

        var popupOthers = Loc.GetString("changeling-absorb-start", ("user", Identity.Entity(changeling, EntityManager)), ("target", Identity.Entity(target, EntityManager)));
        PopupSystem.PopupPredicted(popupOthers, changeling, changeling, PopupType.LargeCaution);
        ChangelingSystem.PlayMeatySound(changeling);

        var doAfterArgs = new DoAfterArgs(EntityManager, changeling, TimeSpan.FromSeconds(15), new AbsorbDNADoAfterEvent(), changeling, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnAbsorbDoAfter(Entity<ChangelingComponent> changeling, ref AbsorbDNADoAfterEvent args)
    {
        if (args.Target == null)
            return;

        var target = args.Target.Value;
        var comp = changeling.Comp;

        if (args.Cancelled || _actionBlockerSystem.CanInteract(target, null) || !TryComp<AbsorbableComponent>(target, out var absorbable))
            return;

        ChangelingSystem.PlayMeatySound(changeling);

        ChangelingSystem.UpdateBiomass(changeling, comp.MaxBiomass - comp.TotalAbsorbedEntities);

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
            bonusEvolutionPoints += 2;
        }
        if (ChangelingSystem.TryStea(uid, target, comp, true);
        comp.TotalAbsorbedEntities++;

        _popup.PopupEntity(popup, args.User, args.User);
        comp.MaxChemicals += bonusChemicals;

        if (TryComp<StoreComponent>(args.User, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "EvolutionPoint", bonusEvolutionPoints } }, args.User, store);
            _store.UpdateUserInterface(args.User, args.User, store);
        }

        // Override ths on server
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            if (_mind.TryGetObjectiveComp<AbsorbConditionComponent>(mindId, out var objective, mind))
                objective.Absorbed += 1;
    }
}

[Serializable, NetSerializable]
public sealed partial class AbsorbDNADoAfterEvent : SimpleDoAfterEvent { }
