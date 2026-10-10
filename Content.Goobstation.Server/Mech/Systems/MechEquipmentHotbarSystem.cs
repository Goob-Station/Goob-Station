using Content.Goobstation.Common.Mech;
using Content.Goobstation.Shared.Mech;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Mech;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Mech.Systems;

/// <summary>
/// Adds an action for mech users for each piece of equipment they have equipped.
/// </summary>
public sealed partial class MechEquipmentHotbarSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechPilotComponent, MechInsertedEvent>(OnPilotInserted);

        SubscribeLocalEvent<MechComponent, MechSelectEquipmentEvent>(OnSelectEquipment);
        SubscribeLocalEvent<MechComponent, MechSelectedEquipmentChangedEvent>(OnSelectedEquipmentChanged);

        SubscribeLocalEvent<MechEquipmentComponent, MechEquipmentInsertedEvent>(OnEquipmentInserted);
        SubscribeLocalEvent<MechEquipmentComponent, MechEquipmentRemovedEvent>(OnEquipmentRemoved);
    }

    private void OnPilotInserted(Entity<MechPilotComponent> pilot, ref MechInsertedEvent args)
    {
        var mech = args.mechUid;

        if (TryComp<MechComponent>(mech, out var mechComp))
        {
            foreach (var equipment in mechComp.EquipmentContainer.ContainedEntities)
                AddHotbarAction((mech, mechComp), pilot, equipment);

            RefreshHotbarToggles(mechComp);
        }
    }

    private void OnEquipmentInserted(Entity<MechEquipmentComponent> equipment, ref MechEquipmentInsertedEvent args) =>
        OnEquipmentChanged(args.Mech, equipment, inserted: true);

    private void OnEquipmentRemoved(Entity<MechEquipmentComponent> equipment, ref MechEquipmentRemovedEvent args) =>
        OnEquipmentChanged(args.Mech, equipment, inserted: false);

    private void OnEquipmentChanged(EntityUid mech, EntityUid equipment, bool inserted)
    {
        if (!TryComp<MechComponent>(mech, out var mechComp))
            return;

        if (inserted && mechComp.PilotSlot.ContainedEntity is { } pilot)
        {
            AddHotbarAction((mech, mechComp), pilot, equipment);
        }
        else if (!inserted && mechComp.EquipmentActions.Remove(equipment, out var action))
        {
            _actions.RemoveAction(action);
            QueueDel(action);
        }

        RefreshHotbarToggles(mechComp);
    }

    private void AddHotbarAction(Entity<MechComponent> mech, EntityUid pilot, EntityUid equipment)
    {
        if (mech.Comp.EquipmentActions.TryGetValue(equipment, out var existing) && Exists(existing))
        {
            EntityUid? existingId = existing;
            _actions.AddAction(pilot, ref existingId, mech.Comp.SelectEquipmentAction, mech);
            return;
        }

        EntityUid? actionId = null;
        if (!_actions.AddAction(pilot, ref actionId, out var action, mech.Comp.SelectEquipmentAction, mech))
            return;

        mech.Comp.EquipmentActions[equipment] = actionId.Value;
        _actions.SetEntityIcon((actionId.Value, action), equipment);

        if (TryComp<InstantActionComponent>(actionId, out var instant) && instant.Event is MechSelectEquipmentEvent ev)
            ev.Equipment = equipment;
    }

    private void RefreshHotbarToggles(MechComponent mechComp)
    {
        foreach (var (equipment, action) in mechComp.EquipmentActions)
            if (Exists(action))
                _actions.SetToggled(action, mechComp.CurrentSelectedEquipment == equipment);
    }

    private void OnSelectEquipment(Entity<MechComponent> ent, ref MechSelectEquipmentEvent args)
    {
        if (args.Handled || args.Equipment is not { } equipment)
            return;

        var mech = ent.Owner;
        var mechComp = ent.Comp;
        var pilot = args.Performer;
        if (!mechComp.EquipmentContainer.Contains(equipment))
            return;

        args.Handled = true;

        mechComp.CurrentSelectedEquipment = mechComp.CurrentSelectedEquipment == equipment ? null : equipment;
        Dirty(mech, mechComp);

        var popup = mechComp.CurrentSelectedEquipment != null
            ? Loc.GetString("mech-equipment-select-popup", ("item", equipment))
            : Loc.GetString("mech-equipment-select-none-popup");
        _popup.PopupEntity(popup, mech, pilot);

        RefreshHotbarToggles(mechComp);
    }

    private void OnSelectedEquipmentChanged(Entity<MechComponent> ent, ref MechSelectedEquipmentChangedEvent args) =>
        RefreshHotbarToggles(ent.Comp);
}
