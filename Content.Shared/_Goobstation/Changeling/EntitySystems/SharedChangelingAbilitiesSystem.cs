using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Changeling.EntitySystems;

public abstract partial class SharedChangelingAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly SharedChangelingSystem _changelingSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    [Dependency] protected readonly IPrototypeManager ProtoManager = default!;
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingActionComponent, ActionAttemptEvent>(OnTryUseAbility);

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAEvent>(OnAbsorb);
    }

    /// <summary>
    ///     Check if user can interact with something.
    /// </summary>
    public bool CanInteract(EntityUid target)
    {
        return _actionBlockerSystem.CanInteract(target, null);
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
        {
            args.Cancelled = true;
            return;
        }

        if (changelingComp.Biomass < comp.BiomassCost)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-biomass-deficit"), user, user);
            args.Cancelled = true;
            return;
        }

        if (changelingComp.FormType < comp.RequiredFormType)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-action-fail-lesserform"), user, user);
            args.Cancelled = true;
            return;
        }

        if (changelingComp.Chemicals < comp.ChemicalCost)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), user, user);
            args.Cancelled = true;
            return;
        }

        if (changelingComp.TotalAbsorbedEntities < comp.RequireAbsorbed)
        {
            var delta = comp.RequireAbsorbed - changelingComp.TotalAbsorbedEntities;
            PopupSystem.PopupEntity(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), user, user);
            args.Cancelled = true;
            return;
        }

        _changelingSystem.UpdateChemicals((user, changelingComp), -comp.ChemicalCost);
        _changelingSystem.UpdateBiomass((user, changelingComp), -comp.BiomassCost);
    }

    private void OnAbsorb(Entity<ChangelingComponent> changeling, ref AbsorbDNAEvent args)
    {
        var target = args.Target;

        if (CanInteract(target))
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-absorb-fail-incapacitated"), changeling, changeling);
            return;
        }

        if (!TryComp<AbsorbableComponent>(target, out var absorbable))
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), changeling, changeling);
            return;
        }

        if (absorbable.Absorbed)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-absorb-fail-absorbed"), changeling, changeling);
            return;
        }

        var popupOthers = Loc.GetString("changeling-absorb-start", ("user", Identity.Entity(changeling, EntityManager)), ("target", Identity.Entity(target, EntityManager)));
        PopupSystem.PopupEntity(popupOthers, changeling, PopupType.LargeCaution);
        _changelingSystem.PlayMeatySound(changeling);

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
}

[Serializable, NetSerializable]
public sealed partial class AbsorbDNADoAfterEvent : SimpleDoAfterEvent { }
