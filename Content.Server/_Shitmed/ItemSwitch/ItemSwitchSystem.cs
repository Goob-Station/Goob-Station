// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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

    /// <summary>
    /// Handles showing the current charge on examination.
    /// </summary>
    private void OnExamined(Entity<ItemSwitchComponent> ent, ref ExaminedEvent args)
    {
        var state = GetState(ent);
        var batteryComponent = GetBatteryComponent(ent);

        // If the item has no battery, or if it does not need power, or if the current state is invalid, cancel.
        if (batteryComponent == null || !ent.Comp.NeedsPower || state == null)
            return;

        // If the current state is the default state, which is also the off state, show off. Else, show on.
        var onMsg = ent.Comp.State != ent.Comp.DefaultState
            ? Loc.GetString("comp-stunbaton-examined-on")
            : Loc.GetString("comp-stunbaton-examined-off");
        args.PushMarkup(onMsg);

        // If the current state is the default state, which is also off, do not calculate the current percentage.
        // This is because any number divided by 0 gets fucked real quick.
        if (ent.Comp.State == ent.Comp.DefaultState)
            return;

        var count = (int) (batteryComponent.CurrentCharge / state.EnergyPerUse);
        args.PushMarkup(Loc.GetString("melee-battery-examine", ("color", "yellow"), ("count", count)));
    }

    /// <summary>
    /// Handles turning off and locking the item when it runs out of power.
    /// </summary>
    private void OnChargeChanged(Entity<ItemSwitchComponent> ent, ref ChargeChangedEvent args)
    {
        var batteryComponent = GetBatteryComponent(ent);
        var state = GetState(ent);

        // If the state doesn't exist, or if the item does not need power, cancel.
        if (state is null || !ent.Comp.NeedsPower )
            return;

        if (batteryComponent is not null)
            ent.Comp.IsPowered = batteryComponent.CurrentCharge >= state.EnergyPerUse;

        // If the default state exists, and the item is not powered, set to default state and lock it there. (Locking is handled in SharedItemSwitchSystem)
        if (ent.Comp.DefaultState != null && ent.Comp.IsPowered == false)
            _itemSwitch.Switch((ent.Owner, ent.Comp), ent.Comp.DefaultState);
    }

    /// <summary>
    /// Handles removing charge from the item on melee attack.
    /// </summary>
    private void OnMeleeAttack(Entity<ItemSwitchComponent> ent, ref MeleeHitEvent args)
    {
        // TODO: MeleeHitEvent is weird. Why is this even raised if we don't hit something?
        if (!args.IsHit)
            return;

        if (args.HitEntities.Count == 0)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;
        var batteryComponent = GetBatteryComponent(ent);
        var state = GetState(ent);

        // If the item does not need power, do not draw power.
        if (!comp.NeedsPower)
            return;

        // If the item has a battery, and the current state is valid, attempt to drain power by the states EnergyPerUse field.
        if (batteryComponent != null && state != null)
            _battery.TryUseCharge(uid, state.EnergyPerUse, batteryComponent);
    }

    #region Helper Methods

    /// <summary>
    /// Gets the battery component of the entity, if it exists.
    /// </summary>
    /// <returns>The battery component of the entity.</returns>
    private BatteryComponent? GetBatteryComponent(EntityUid uid)
    {
        return TryComp<BatteryComponent>(uid, out var battery) ? battery : null;
    }

    /// <summary>
    /// Gets the current state of the item.
    /// </summary>
    /// <returns>The current state of the item.</returns>
    private static ItemSwitchState? GetState(ItemSwitchComponent comp)
    {
        return comp.States.TryGetValue(comp.State, out var state) ? state : null;
    }

    #endregion
}
