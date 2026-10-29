using Content.Server.Atmos.EntitySystems;
using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.SalemStation;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.SalemStation;

public sealed class TransferValveSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly SignalTimerSystem _signalTimer = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransferValveComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TransferValveComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<TransferValveComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<TransferValveComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<TransferValveComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<TransferValveComponent, EntInsertedIntoContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<TransferValveComponent, EntRemovedFromContainerMessage>(OnContainerChanged);
    }

    private void OnInit(EntityUid uid, TransferValveComponent component, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, TransferValveComponent.PlasmaTankSlotId, component.PlasmaTankSlot);
        _itemSlots.AddItemSlot(uid, TransferValveComponent.OxygenTankSlotId, component.OxygenTankSlot);
        _itemSlots.AddItemSlot(uid, TransferValveComponent.TimerSlotId, component.TimerSlot);
        UpdateAppearance(uid, component);
    }

    private void OnRemove(EntityUid uid, TransferValveComponent component, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, component.PlasmaTankSlot);
        _itemSlots.RemoveItemSlot(uid, component.OxygenTankSlot);
        _itemSlots.RemoveItemSlot(uid, component.TimerSlot);
    }

    private void OnInteractUsing(EntityUid uid, TransferValveComponent component, InteractUsingEvent args)
    {
        if (component.Detonated)
            return;

        var used = args.Used;
        string? slotId = null;

        if (TryComp<GasTankComponent>(used, out var tank))
        {
            if (!component.PlasmaTankSlot.HasItem && IsPlasmaTank(tank))
                slotId = TransferValveComponent.PlasmaTankSlotId;
            else if (!component.OxygenTankSlot.HasItem && IsOxygenTank(tank))
                slotId = TransferValveComponent.OxygenTankSlotId;
        }
        else if (HasComp<SignalTimerComponent>(used) && !component.TimerSlot.HasItem)
        {
            slotId = TransferValveComponent.TimerSlotId;
        }

        if (slotId == null || !_itemSlots.TryInsert(uid, slotId, used, args.User))
            return;

        UpdateAppearance(uid, component);
        args.Handled = true;
    }

    private void OnContainerChanged(EntityUid uid, TransferValveComponent component, ContainerModifiedMessage args)
    {
        UpdateAppearance(uid, component);
    }

    private void OnInteractHand(EntityUid uid, TransferValveComponent component, InteractHandEvent args)
    {
        if (TryStartTimer(component))
            args.Handled = true;
    }

    private void OnActivateInWorld(EntityUid uid, TransferValveComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        if (TryStartTimer(component))
            args.Handled = true;
    }

    private bool TryStartTimer(TransferValveComponent component)
    {
        if (component.Detonated ||
            component.PlasmaTankSlot.Item is not { } plasmaUid ||
            component.OxygenTankSlot.Item is not { } oxygenUid ||
            component.TimerSlot.Item is not { } timerUid ||
            !TryComp<GasTankComponent>(plasmaUid, out _) ||
            !TryComp<GasTankComponent>(oxygenUid, out _) ||
            !TryComp<SignalTimerComponent>(timerUid, out var timer) ||
            HasComp<ActiveSignalTimerComponent>(timerUid))
            return false;

        _signalTimer.OnStartTimer(timerUid, timer);
        return true;
    }

    private void UpdateAppearance(EntityUid uid, TransferValveComponent component)
    {
        _appearance.SetData(uid, TransferValveVisuals.Assembled,
            component.PlasmaTankSlot.HasItem &&
            component.OxygenTankSlot.HasItem &&
            component.TimerSlot.HasItem);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TransferValveComponent>();
        while (query.MoveNext(out var uid, out var valve))
        {
            if (valve.Detonated || valve.TimerSlot.Item is not { } timerUid)
                continue;

            if (TryComp<ActiveSignalTimerComponent>(timerUid, out var activeTimer))
            {
                valve.TimerWasActive = true;

                if (activeTimer.TriggerTime > _gameTiming.CurTime)
                    continue;
            }
            else if (!valve.TimerWasActive)
            {
                continue;
            }

            Detonate(uid, valve);
        }
    }

    private void Detonate(EntityUid uid, TransferValveComponent valve)
    {
        if (valve.PlasmaTankSlot.Item is not { } plasmaUid ||
            valve.OxygenTankSlot.Item is not { } oxygenUid ||
            !TryComp<GasTankComponent>(plasmaUid, out var plasmaTank) ||
            !TryComp<GasTankComponent>(oxygenUid, out var oxygenTank))
        {
            return;
        }

        valve.Detonated = true;

        _gasTank.AssumeAir((plasmaUid, plasmaTank), oxygenTank.Air);
        oxygenTank.Air.Clear();
        _gasTank.CheckStatus((plasmaUid, plasmaTank));
    }

    private static bool IsPlasmaTank(GasTankComponent tank)
    {
        return tank.Air.GetMoles(Gas.Plasma) > tank.Air.GetMoles(Gas.Oxygen);
    }

    private static bool IsOxygenTank(GasTankComponent tank)
    {
        return tank.Air.GetMoles(Gas.Oxygen) > tank.Air.GetMoles(Gas.Plasma);
    }
}
