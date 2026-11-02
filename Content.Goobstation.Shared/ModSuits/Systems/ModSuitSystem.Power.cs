using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Popups;
using Content.Shared.Rounding;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem
{
    private static readonly TimeSpan PowerAlertInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextPowerAlertUpdate;

    public EntityUid? GetCore(Entity<ModSuitComponent> ent)
    {
        return _itemSlots.GetItemOrNull(ent, ent.Comp.CoreSlotId);
    }

    public Entity<BatteryComponent>? GetBattery(Entity<ModSuitComponent> ent)
    {
        if (GetCore(ent) is not { } core)
            return null;

        return _powerCell.TryGetBatteryFromEntityOrSlot(core, out var battery) ? battery : null;
    }

    public bool HasCharge(Entity<ModSuitComponent> ent, float amount)
    {
        return GetBattery(ent) is { } battery && _battery.GetCharge(battery.AsNullable()) >= amount;
    }

    public bool TryUseCharge(Entity<ModSuitComponent> ent, float amount)
    {
        if (amount <= 0f)
            return true;

        return GetBattery(ent) is { } battery && _battery.TryUseCharge(battery.AsNullable(), amount);
    }

    public float GetThemeDrain(Entity<ModSuitComponent> ent)
    {
        return _proto.Index(ent.Comp.Theme).ChargeDrain;
    }

    public float GetTotalDrain(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.State == ModSuitState.Inactive)
            return 0f;

        var drain = GetThemeDrain(ent);
        if (ent.Comp.State != ModSuitState.Active)
            return drain;

        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (!TryComp<ModModuleComponent>(module, out var moduleComp))
                continue;

            drain += moduleComp.IdlePower;
            if (moduleComp.Active)
                drain += moduleComp.ActivePower;
        }

        return drain;
    }

    public void RefreshDrain(Entity<ModSuitComponent> ent)
    {
        if (GetBattery(ent) is { } battery)
            _battery.RefreshChargeRate(battery.AsNullable());
    }

    [SubscribeLocalEvent]
    private void OnRefreshChargeRate(Entity<ModSuitComponent> ent, ref RefreshChargeRateEvent args)
    {
        args.NewChargeRate -= GetTotalDrain(ent);
    }

    [SubscribeLocalEvent]
    private void OnBatteryStateChanged(Entity<ModSuitComponent> ent, ref BatteryStateChangedEvent args)
    {
        if (args.NewState == BatteryState.Empty)
            SetPowered(ent, false);
        else if (args.OldState == BatteryState.Empty)
            SetPowered(ent, true);
    }

    [SubscribeLocalEvent]
    private void OnSuitEntInserted(Entity<ModSuitComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (ent.Comp.LifeStage < ComponentLifeStage.Running)
            return;

        RaiseUpdated(ent);

        if (args.Container.ID != ent.Comp.CoreSlotId || _timing.ApplyingState)
            return;

        RefreshDrain(ent);
        SetPowered(ent, HasCharge(ent, 0.01f));
    }

    [SubscribeLocalEvent]
    private void OnSuitEntRemoved(Entity<ModSuitComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (ent.Comp.LifeStage < ComponentLifeStage.Running)
            return;

        RaiseUpdated(ent);

        if (args.Container.ID != ent.Comp.CoreSlotId || _timing.ApplyingState)
            return;

        SetPowered(ent, false);
    }

    private void RaiseUpdated(EntityUid suit)
    {
        var ev = new ModSuitUpdatedEvent();
        RaiseLocalEvent(suit, ref ev);
    }

    private void SetPowered(Entity<ModSuitComponent> ent, bool powered)
    {
        if (ent.Comp.Powered == powered)
            return;

        ent.Comp.Powered = powered;
        Dirty(ent);

        if (ent.Comp.State == ModSuitState.Active)
        {
            if (powered)
            {
                ActivatePassiveModules(ent);
            }
            else
            {
                DeactivateAllModules(ent, true);
                if (ent.Comp.Wearer is { } wearer)
                    _popup.PopupClient(Loc.GetString("modsuit-out-of-power"), wearer, wearer, PopupType.MediumCaution);
            }
        }

        RefreshDrain(ent);
        RefreshSpeed(ent);
        UpdatePowerAlert(ent);
    }

    private void UpdatePowerAlerts()
    {
        if (_timing.CurTime < _nextPowerAlertUpdate)
            return;

        _nextPowerAlertUpdate = _timing.CurTime + PowerAlertInterval;

        var query = EntityQueryEnumerator<ModSuitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Wearer != null && comp.State != ModSuitState.Inactive && ShouldStep(comp))
                UpdatePowerAlert((uid, comp));
        }
    }

    private void UpdatePowerAlert(Entity<ModSuitComponent> ent)
    {
        if (_timing.ApplyingState || ent.Comp.Wearer is not { } wearer)
            return;

        if (ent.Comp.State == ModSuitState.Inactive || GetBattery(ent) is not { } battery)
        {
            _alerts.ClearAlert(wearer, ent.Comp.PowerAlert);
            return;
        }

        var charge = _battery.GetCharge(battery.AsNullable());
        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, charge), battery.Comp.MaxCharge, 6);
        _alerts.ShowAlert(wearer, ent.Comp.PowerAlert, (short) severity);
    }
}
