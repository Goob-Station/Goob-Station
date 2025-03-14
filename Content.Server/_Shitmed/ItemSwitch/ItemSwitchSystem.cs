using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Shitmed.ItemSwitch;
using Content.Shared._Shitmed.ItemSwitch.Components;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Shitmed.ItemSwitch;

public sealed class ItemSwitchSystem : SharedItemSwitchSystem
{
    [Dependency] private readonly SharedItemSwitchSystem _itemSwitch = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSwitchComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ItemSwitchComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<ItemSwitchComponent, MeleeHitEvent>(OnMeleeAttack);
    }

    public BatteryComponent? GetBatteryComponent(Entity<ItemSwitchComponent> ent)
    {
        return TryComp<BatteryComponent>(ent.Owner, out var battery) ? battery : null;
    }

    public static ItemSwitchState? GetState(Entity<ItemSwitchComponent> ent)
    {
        return ent.Comp.States.TryGetValue(ent.Comp.State, out var state) ? state : null;
    }

    private void OnExamined(Entity<ItemSwitchComponent> ent, ref ExaminedEvent args)
    {
        var state = GetState(ent);
        var batteryComponent = GetBatteryComponent(ent);
        var comp = ent.Comp;

        if (batteryComponent == null || !comp.NeedsPower || state == null)
            return;

        var onMsg = comp.State != comp.DefaultState
            ? Loc.GetString("comp-stunbaton-examined-on")
            : Loc.GetString("comp-stunbaton-examined-off");
        args.PushMarkup(onMsg);

        if (comp.State == comp.DefaultState)
            return;

        var count = (int) (batteryComponent.CurrentCharge / state.EnergyPerUse);
        args.PushMarkup(Loc.GetString("melee-battery-examine", ("color", "yellow"), ("count", count)));
    }

    private void OnChargeChanged(Entity<ItemSwitchComponent> ent, ref ChargeChangedEvent args)
    {
        var batteryComponent = GetBatteryComponent(ent);
        var state = GetState(ent);
        var comp = ent.Comp;

        if (state == null)
            return;

        var energyPerUse = state.EnergyPerUse;

        if (!ent.Comp.NeedsPower)
            return;

        if (batteryComponent == null || !(batteryComponent.CurrentCharge < energyPerUse))
            return;

        if (batteryComponent.CurrentCharge > energyPerUse)
            comp.IsPowered = true;

        if (ent.Comp.DefaultState != null)
            _itemSwitch.Switch((ent.Owner, ent.Comp), ent.Comp.DefaultState);
        comp.IsPowered = false;

    }

    private void OnMeleeAttack(Entity<ItemSwitchComponent> ent, ref MeleeHitEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var batteryComponent = GetBatteryComponent(ent);
        var state = GetState(ent);

        if (!comp.NeedsPower)
            return;
        if (batteryComponent != null && state != null)
            _battery.TryUseCharge(uid, state.EnergyPerUse, batteryComponent);
    }

    // This might be obscene supercode, but that's how ya learn I guess.
}
