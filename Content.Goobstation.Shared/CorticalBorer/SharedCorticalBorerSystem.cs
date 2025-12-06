using Content.Goobstation.Common.CorticalBorer;
using Content.Goobstation.Shared.CorticalBorer.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.MedicalScanner;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.CorticalBorer;

public abstract class SharedCorticalBorerSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UI = default!;
    [Dependency] protected readonly SharedActionsSystem Actions = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly AlertsSystem Alerts = default!;

    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorticalBorerComponent, AttemptMeleeEvent>(OnAttempt);

        SubscribeLocalEvent<CorticalBorerInfestedComponent, CheckCorticalBorerEvent>(OnCheck);
        SubscribeLocalEvent<CorticalBorerInfestedComponent, EjectCorticalBorerEvent>(OnEject);
    }

    private void OnEject(Entity<CorticalBorerInfestedComponent> ent, ref EjectCorticalBorerEvent args)
    {
        if (ent.Comp.InfestationContainer.ContainedEntities.Count != 0)
            TryEjectBorer(ent.Comp.Borer);
    }

    private void OnCheck(Entity<CorticalBorerInfestedComponent> ent, ref CheckCorticalBorerEvent args)
    {
        args.Found = true;
    }

    private void OnAttempt(Entity<CorticalBorerComponent> ent, ref AttemptMeleeEvent args)
    {
        if (ent.Comp.Host != null)
            args.Cancelled = true;
    }

    public bool CanUseAbility(Entity<CorticalBorerComponent> ent, EntityUid target)
    {
        if (!_statusEffects.HasStatusEffect(target, ent.Comp.StatusEffectProto))
            return true;

        Popup.PopupEntity(Loc.GetString("cortical-borer-sugar-block"), ent.Owner, ent.Owner, PopupType.Medium);
        return false;
    }

    public bool TryUseAbility(Entity<CorticalBorerComponent> borer, EntityUid user, EntityUid ability)
    {
        if (!TryComp(ability, out CorticalBorerAbilityRequiresChemicalsComponent? requiresChems))
            return true;

        if (requiresChems.Chemicals <= borer.Comp.ChemicalPoints)
        {
            UpdateChems(borer, -requiresChems.Chemicals);
            return true;
        }

        Popup.PopupEntity(Loc.GetString("cortical-borer-not-enough-chem"), user, user, PopupType.Medium);
        return false;
    }

    public void InfestTarget(Entity<CorticalBorerComponent> ent, EntityUid target)
    {
        var (uid, comp) = ent;

        // Make sure the infected person is infected right
        var infestedComp = EnsureComp<CorticalBorerInfestedComponent>(target);

        // Make sure they get into the target
        if (!Container.Insert(uid, infestedComp.InfestationContainer))
        {
            // oh no it didn't work somehow so remove the comp you just added...
            RemCompDeferred<CorticalBorerInfestedComponent>(target);
            return;
        }

        // Set up the Borer
        infestedComp.Borer = ent;
        comp.Host = target;

        if (comp.AddOnInfest is not null)
        {
            foreach (var (_, compReg) in comp.AddOnInfest)
            {
                var compType = compReg.Component.GetType();
                if (HasComp(ent, compType))
                    continue;

                var newComp = (Component) _serManager.CreateCopy(compReg.Component, notNullableOverride: true);
                EntityManager.AddComponent(ent, newComp, true);
            }
        }

        if (comp.RemoveOnInfest is not null)
        {
            foreach (var (_, compReg) in comp.RemoveOnInfest)
            {
                RemCompDeferred(ent, compReg.Component.GetType());
            }
        }

        if (TryComp<DamageableComponent>(ent, out var damComp))
            _damage.SetAllDamage(ent, damComp, 0);
    }

    public bool TryEjectBorer(Entity<CorticalBorerComponent> ent)
    {
        if (!ent.Comp.Host.HasValue)
            return false;

        if (TerminatingOrDeleted(ent.Owner))
            return false;

        // Make sure they get out of the host
        if (!Container.TryRemoveFromContainer(ent.Owner))
            return false;

        // close all the UIs that relate to host
        if (TryComp<UserInterfaceComponent>(ent, out var uic))
        {
            UI.CloseUi((ent.Owner, uic), HealthAnalyzerUiKey.Key);
            UI.CloseUi((ent.Owner, uic), CorticalBorerDispenserUiKey.Key);
        }

        RemCompDeferred<CorticalBorerInfestedComponent>(ent.Comp.Host.Value);
        ent.Comp.Host = null;

        if (ent.Comp.RemoveOnInfest is not null)
        {
            foreach (var (_, compReg) in ent.Comp.RemoveOnInfest)
            {
                var compType = compReg.Component.GetType();
                if (HasComp(ent, compType))
                    continue;

                var newComp = (Component) _serManager.CreateCopy(compReg.Component, notNullableOverride: true);
                EntityManager.AddComponent(ent, newComp, true);
            }
        }

        if (ent.Comp.AddOnInfest is not null)
        {
            foreach (var (_, compReg) in ent.Comp.AddOnInfest)
            {
                RemCompDeferred(ent, compReg.Component.GetType());
            }
        }

        return true;
    }

    public void LayEgg(Entity<CorticalBorerComponent> ent)
    {
        if (ent.Comp.Host is not { } host)
            return;

        if (ent.Comp.EggProto is not { } egg)
            return;

        PredictedSpawnAtPosition(egg, host.ToCoordinates());

        // TODO: Brain damage
        _damage.TryChangeDamage(host,
            new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), 15),
            true,
            origin: ent,
            targetPart: TargetBodyPart.Head);
    }

    private List<CorticalBorerDispenserItem> GetAllBorerChemicals(Entity<CorticalBorerComponent> ent)
    {
        var clones = new List<CorticalBorerDispenserItem>();
        foreach (var prototype in _proto.EnumeratePrototypes<CorticalBorerChemicalPrototype>())
        {
            if (!_proto.TryIndex(prototype.Reagent, out ReagentPrototype? proto))
                continue;

            var reagentName = proto.LocalizedName;
            var reagentId = proto.ID;
            var cost = prototype.Cost;
            var amount = ent.Comp.InjectAmount;
            var chems = ent.Comp.ChemicalPoints;
            var color = proto.SubstanceColor;

            clones.Add(new CorticalBorerDispenserItem(reagentName,reagentId, cost, amount, chems, color)); // need color and name
        }

        return clones;
    }

    public void UpdateChems(Entity<CorticalBorerComponent> ent, int change)
    {
        var (_, comp) = ent;

        if (comp.ChemicalPoints + change >= comp.ChemicalPointCap)
            comp.ChemicalPoints = comp.ChemicalPointCap;
        else if (comp.ChemicalPoints + change <= 0)
            comp.ChemicalPoints = 0;
        else
            comp.ChemicalPoints += change;

        if (comp.ChemicalPoints % comp.UiUpdateInterval == 0)
            UpdateUiState(ent);

        Alerts.ShowAlert(ent, ent.Comp.ChemicalAlert);

        Dirty(ent);
    }

    protected void UpdateUiState(Entity<CorticalBorerComponent> ent)
    {
        var chems = GetAllBorerChemicals(ent);

        var state = new CorticalBorerDispenserBoundUserInterfaceState(chems, ent.Comp.InjectAmount);
        UI.SetUiState(ent.Owner, CorticalBorerDispenserUiKey.Key, state);
    }
}

public sealed class InfestHostAttempt : CancellableEntityEventArgs
{
    /// <summary>
    ///     The equipment that is blocking the entrance
    /// </summary>
    public EntityUid? Blocker = null;
}

[Serializable, NetSerializable]
public enum CorticalBorerDispenserUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserSetInjectAmountMessage : BoundUserInterfaceMessage
{
    public readonly int CorticalBorerDispenserDispenseAmount;

    public CorticalBorerDispenserSetInjectAmountMessage(int amount)
    {
        CorticalBorerDispenserDispenseAmount = amount;
    }
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserInjectMessage : BoundUserInterfaceMessage
{
    public readonly string ChemProtoId;

    public CorticalBorerDispenserInjectMessage(string proto)
    {
        ChemProtoId = proto;
    }
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly List<CorticalBorerDispenserItem> DisList;

    public readonly int SelectedDispenseAmount;

    public CorticalBorerDispenserBoundUserInterfaceState(List<CorticalBorerDispenserItem> disList, int dispenseAmount)
    {
        DisList = disList;
        SelectedDispenseAmount = dispenseAmount;
    }
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserItem(
    string reagentName,
    string reagentId,
    int cost,
    int amount,
    int chems,
    Color reagentColor)
{
    public string ReagentName = reagentName;
    public string ReagentId = reagentId;
    public int Cost = cost;
    public int Amount = amount;
    public int Chems = chems;
    public Color ReagentColor = reagentColor;
}
