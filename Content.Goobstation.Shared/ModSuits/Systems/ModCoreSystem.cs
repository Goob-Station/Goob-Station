using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Emp;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModCoreSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStackSystem _stack = default!;

    [SubscribeLocalEvent]
    private void OnRefreshChargeRate(Entity<ModCoreComponent> ent, ref RefreshChargeRateEvent args)
    {
        RelayToSuit(ent, ref args);
    }

    [SubscribeLocalEvent]
    private void OnBatteryStateChanged(Entity<ModCoreComponent> ent, ref BatteryStateChangedEvent args)
    {
        RelayToSuit(ent, ref args);
    }

    [SubscribeLocalEvent]
    private void OnEmpAttempt(Entity<ModCoreComponent> ent, ref EmpAttemptEvent args)
    {
        RelayToSuit(ent, ref args);
    }

    private void RelayToSuit<T>(Entity<ModCoreComponent> ent, ref T args) where T : notnull
    {
        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container)
            && HasComp<ModSuitComponent>(container.Owner))
            RaiseLocalEvent(container.Owner, ref args);
    }

    [SubscribeLocalEvent]
    private void OnRefuel(Entity<ModCoreRefuelableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !_whitelist.IsValid(ent.Comp.Fuel, args.Used))
            return;

        args.Handled = true;

        if (!_powerCell.TryGetBatteryFromEntityOrSlot(ent.Owner, out var battery))
            return;

        var missing = battery.Value.Comp.MaxCharge - _battery.GetCharge(battery.Value.AsNullable());
        if (missing <= 0f)
        {
            _popup.PopupClient(Loc.GetString("modsuit-core-full"), args.Target, args.User);
            return;
        }

        var wanted = Math.Max(1, (int) MathF.Ceiling(missing / ent.Comp.ChargePerFuel));
        var used = 1;
        if (TryComp<StackComponent>(args.Used, out var stack))
        {
            used = Math.Min(wanted, stack.Count);
            _stack.SetCount((args.Used, stack), stack.Count - used);
        }
        else
        {
            PredictedQueueDel(args.Used);
        }

        _battery.ChangeCharge(battery.Value.AsNullable(), used * ent.Comp.ChargePerFuel);
        _audio.PlayPredicted(ent.Comp.RefuelSound, args.Target, args.User);
        _popup.PopupClient(Loc.GetString("modsuit-core-refueled"), args.Target, args.User);
    }
}
