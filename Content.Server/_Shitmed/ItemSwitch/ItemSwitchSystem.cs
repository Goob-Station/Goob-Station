using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Shitmed.ItemSwitch;
using Content.Shared._Shitmed.ItemSwitch.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Shitmed.ItemSwitch;

public sealed class ItemSwitchSystem : SharedItemSwitchSystem
{
    [Dependency] private readonly SharedItemSwitchSystem _itemSwitch = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSwitchComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<ItemSwitchComponent, MeleeAttackEvent>(OnMeleeAttackRef);
    }

    public BatteryComponent? GetBatteryComponent(Entity<ItemSwitchComponent> ent)
    {
        return TryComp<BatteryComponent>(ent.Owner, out var battery) ? battery : null;
    }

    public ItemSwitchState? GetState(Entity<ItemSwitchComponent> ent)
    {
        return ent.Comp.States.TryGetValue(ent.Comp.State, out var state) ? state : null;
    }

    private void OnChargeChanged(Entity<ItemSwitchComponent> ent, ref ChargeChangedEvent args)
    {
        var batteryComponent = GetBatteryComponent(ent);
        var state = GetState(ent);

        if (state == null)
            return;

        var energyPerUse = state.EnergyPerUse;

        if (!ent.Comp.NeedsPower)
            return;
        if (batteryComponent != null && batteryComponent.CurrentCharge < energyPerUse)
            _itemSwitch.TryTurnOff(ent);
    }

    private void OnMeleeAttackRef(Entity<ItemSwitchComponent> ent, ref MeleeAttackEvent args)
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
