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
    ///     Trying to use changeling action ability.
    /// </summary>
    public bool TryUseLingAbility(Entity<ChangelingComponent> changeling, IChangelingAction action)
    {
        var comp = changeling.Comp;

        if (comp.Biomass < action.BiomassCost)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-biomass-deficit"), changeling, changeling);
            return false;
        }

        if (comp.FormType < action.RequiredForm)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-action-fail-lesserform"), changeling, changeling);
            return false;
        }

        if (comp.Chemicals < action.ChemicalCost)
        {
            PopupSystem.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), changeling, changeling);
            return false;
        }

        if (comp.TotalAbsorbedEntities < action.RequiredAbsorbed)
        {
            var delta = action.RequiredAbsorbed - comp.TotalAbsorbedEntities;
            PopupSystem.PopupEntity(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), changeling, changeling);
            return false;
        }

        _changelingSystem.UpdateChemicals(changeling, -action.ChemicalCost);
        _changelingSystem.UpdateBiomass(changeling, -action.BiomassCost);

        return true;
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

        TryUseLingAbility(changeling, args);

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
