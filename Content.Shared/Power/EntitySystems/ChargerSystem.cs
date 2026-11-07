using System.Diagnostics.CodeAnalysis;
using Content.Shared.Emp;
using Content.Shared.Examine;
using Content.Shared.Power.Components;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Storage.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared.Power.EntitySystems;

public sealed class ChargerSystem : EntitySystem
{
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _receiver = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChargerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ChargerComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<ChargerComponent, EntRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<ChargerComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<ChargerComponent, InsertIntoEntityStorageAttemptEvent>(OnEntityStorageInsertAttempt);
        SubscribeLocalEvent<ChargerComponent, ExaminedEvent>(OnChargerExamine);
        SubscribeLocalEvent<ChargerComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<ChargerComponent, EmpDisabledRemovedEvent>(OnEmpRemoved);
        SubscribeLocalEvent<InsideChargerComponent, RefreshChargeRateEvent>(OnRefreshChargeRate);
        SubscribeLocalEvent<InsideChargerComponent, BatteryStateChangedEvent>(OnStatusChanged);
    }

    private void OnStartup(Entity<ChargerComponent> ent, ref ComponentStartup args)
    {
        UpdateStatus(ent);
    }

    private void OnChargerExamine(EntityUid uid, ChargerComponent component, ExaminedEvent args)
    {
        using (args.PushGroup(nameof(ChargerComponent)))
        {
            // rate at which the charger charges
            args.PushMarkup(Loc.GetString("charger-examine", ("color", "yellow"), ("chargeRate", (int) component.ChargeRate)));
            //Goob : checks each item slot assigned to the charger and either displays their respective charge level or tells if it's empty
            //Goob : same principle as the older system with only one itemslot being able to be assigned per charger, except checks every item slot in the list, still works with only 1 itemslot
            foreach (var slot in component.SlotId)
            {
                // try to get contents of the charger
                if (!_container.TryGetContainer(uid, slot, out var container))
                    return;

                if (HasComp<PowerCellSlotComponent>(uid))
                    return;

                // if charger is empty and not a power cell type charger, add empty message
                // power cells have their own empty message by default, for things like flash lights
                if (container.ContainedEntities.Count == 0)
                {
                    args.PushMarkup(Loc.GetString("charger-empty"));
                }
                else
                {
                    // add how much each item is charged it
                    foreach (var contained in container.ContainedEntities)
                    {
                        if (!_powerCell.TryGetBatteryFromEntityOrSlot(contained, out var battery))
                            continue;

                        var chargePercent = _battery.GetChargeLevel(battery.Value.AsNullable()) * 100;
                        args.PushMarkup(Loc.GetString("charger-content", ("chargePercent", (int) chargePercent)));
                    }
                }
            }
        }
    }

    private void OnPowerChanged(Entity<ChargerComponent> ent, ref PowerChangedEvent args)
    {
        RefreshAllBatteries(ent);
        UpdateStatus(ent);
    }

    private void OnInserted(Entity<ChargerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_timing.ApplyingState)
            return; // Already networked in the same gamestate

        if (!ent.Comp.SlotId.Contains(args.Container.ID))
            return;

        AddComp<InsideChargerComponent>(args.Entity);
        if (_powerCell.TryGetBatteryFromEntityOrSlot(args.Entity, out var battery))
            _battery.RefreshChargeRate(battery.Value.AsNullable());
        UpdateStatus(ent);
    }

    private void OnRemoved(Entity<ChargerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState)
            return; // Already networked in the same gamestate

        if (!ent.Comp.SlotId.Contains(args.Container.ID))
            return;

        RemComp<InsideChargerComponent>(args.Entity);
        if (_powerCell.TryGetBatteryFromEntityOrSlot(args.Entity, out var battery))
            _battery.RefreshChargeRate(battery.Value.AsNullable());
        UpdateStatus(ent);
    }

    /// <summary>
    /// Verify that the entity being inserted is actually rechargeable.
    /// </summary>
    private void OnInsertAttempt(EntityUid uid, ChargerComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (!component.Initialized)
            return;

        if (!component.SlotId.Contains(args.Container.ID))
            return;

        if (!TryComp<PowerCellSlotComponent>(args.EntityUid, out var cellSlot))
            return;

        if (!cellSlot.FitsInCharger)
            args.Cancel();
    }

    private void OnEntityStorageInsertAttempt(EntityUid uid, ChargerComponent component, ref InsertIntoEntityStorageAttemptEvent args)
    {
        if (!component.Initialized || args.Cancelled)
            return;

        if (!component.SlotId.Contains(args.Container.ID))
            return;

        if (!TryComp<PowerCellSlotComponent>(uid, out var cellSlot))
            return;

        if (!cellSlot.FitsInCharger)
            args.Cancelled = true;
    }
    private void OnEmpPulse(Entity<ChargerComponent> ent, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
        RefreshAllBatteries(ent);
        UpdateStatus(ent);
    }

    private void OnEmpRemoved(Entity<ChargerComponent> ent, ref EmpDisabledRemovedEvent args)
    {
        RefreshAllBatteries(ent);
        UpdateStatus(ent);
    }

    private void OnRefreshChargeRate(Entity<InsideChargerComponent> ent, ref RefreshChargeRateEvent args)
    {
        var chargerUid = Transform(ent).ParentUid;

        if (HasComp<EmpDisabledComponent>(chargerUid))
            return;

        if (!TryComp<ChargerComponent>(chargerUid, out var chargerComp))
            return;

        if (!chargerComp.Portable && !_receiver.IsPowered(chargerUid))
            return;

        if (_whitelist.IsWhitelistFail(chargerComp.Whitelist, ent.Owner))
            return;

        args.NewChargeRate += chargerComp.ChargeRate;
    }
    private void OnStatusChanged(Entity<InsideChargerComponent> ent, ref BatteryStateChangedEvent args)
    {
        // If the battery is full update the visuals and power draw of the charger.

        var chargerUid = Transform(ent).ParentUid;
        if (!TryComp<ChargerComponent>(chargerUid, out var chargerComp))
            return;

        UpdateStatus((chargerUid, chargerComp));
    }

    private void RefreshAllBatteries(Entity<ChargerComponent> ent)
    {
        // try to get contents of the charger
        //Goob: does so for every itemslot of the charger
        foreach (var slot in ent.Comp.SlotId)
        {
            if (!_container.TryGetContainer(ent.Owner, slot, out var container))
                continue;

            foreach (var item in container.ContainedEntities)
            {
                if (_powerCell.TryGetBatteryFromEntityOrSlot(item, out var battery))
                    _battery.RefreshChargeRate(battery.Value.AsNullable());
            }
        }
    }

    private void UpdateStatus(Entity<ChargerComponent> ent)
    {
        TryComp<AppearanceComponent>(ent, out var appearance);
        //Goob: system checks for items in the slots of the charger, and when they find one update the power draw of the charger
        // accordingly to its charge rate multiplied by the amount of items currently being charged inside the charger, ergo, two items charging, 2*chargerate
        int drawcount = 0;
        foreach (var slot in ent.Comp.SlotId)
        {
            if (!_container.TryGetContainer(ent.Owner, slot, out var container))
                continue;
            _appearance.SetData(ent.Owner, CellVisual.Occupied, container.ContainedEntities.Count != 0, appearance);
            foreach (var item in container.ContainedEntities)
            {
                if (_powerCell.TryGetBatteryFromEntityOrSlot(item, out var battery))
                {
                    if (battery.Value.Comp.LastCharge < battery.Value.Comp.MaxCharge)
                        drawcount += 1;
                }
            }
        }
        var status = GetStatus(ent);
        switch (status)
        {
            case CellChargerStatus.Charging:
                //Goob TODO: If someone ever adds chargers that can charge multiple batteries at multiple rates, adjust drawcount depending on something that would equate
                //  to the total power draw of each recharging item
                _receiver.SetLoad(ent.Owner, ent.Comp.ChargeRate * drawcount);
                break;
            default:
                // Don't set the load to 0 or the charger will be considered as powered even if the LV connection is unpowered.
                // TODO: Fix this on an ApcPowerReceiver level.
                _receiver.SetLoad(ent.Owner, ent.Comp.PassiveDraw);
                break;
        }
        _appearance.SetData(ent.Owner, CellVisual.Light, status, appearance);
    }

    private CellChargerStatus GetStatus(Entity<ChargerComponent> ent)
    {
        if (!ent.Comp.Portable && !Transform(ent).Anchored)
            return CellChargerStatus.Off;

        if (!ent.Comp.Portable && !_receiver.IsPowered(ent.Owner))
            return CellChargerStatus.Off;

        if (HasComp<EmpDisabledComponent>(ent))
            return CellChargerStatus.Off;
        // Goob: checks each item slot assigned to charger and updates status for visuals on the charger
        foreach (var slot in ent.Comp.SlotId)
        {
            if (!_container.TryGetContainer(ent.Owner, slot, out var container))
                return CellChargerStatus.Off;

            if (container.ContainedEntities.Count == 0)
                return CellChargerStatus.Empty;

            // Use the first stored battery for visuals. If someone ever makes a multi-slot charger then this will need to be changed.
            //Goob: i didn't change it but aside from appearance status everything works
            if (!_powerCell.TryGetBatteryFromEntityOrSlot(container.ContainedEntities[0], out var battery))
                return CellChargerStatus.Off;

            if (_battery.IsFull(battery.Value.AsNullable()))
                return CellChargerStatus.Charged;
        }
        return CellChargerStatus.Charging;
    }
}
