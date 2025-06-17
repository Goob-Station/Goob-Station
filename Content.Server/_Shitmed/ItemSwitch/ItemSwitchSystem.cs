// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Shitmed.ItemSwitch;
using Content.Shared._Shitmed.ItemSwitch.Components;
using Content.Shared.Damage.Systems;
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
        SubscribeLocalEvent<ItemSwitchComponent, AttemptMeleeEvent>(OnAttemptMelee);
        SubscribeLocalEvent<ItemSwitchComponent, MeleeHitEvent>(OnMeleeAttack, after: [typeof(StaminaSystem)]);
    }

    /// <summary>
    /// Handles showing the current charge on examination.
    /// </summary>
    private void OnExamined(Entity<ItemSwitchComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.NeedsPower
        || !TryComp<BatteryComponent>(ent, out var battery)
        || !ent.Comp.States.TryGetValue(ent.Comp.State, out var state))
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

        var count = (int) (battery.CurrentCharge / state.EnergyPerUse);
        args.PushMarkup(Loc.GetString("melee-battery-examine", ("color", "yellow"), ("count", count)));
    }

    private void CheckPowerAndSwitchState(EntityUid uid, ItemSwitchComponent component)
    {
        if (!component.NeedsPower
            || !TryComp<BatteryComponent>(uid, out var battery)
            || !component.States.TryGetValue(component.State, out var state))
            return;

        component.IsPowered = battery.CurrentCharge >= state.EnergyPerUse;

        if (component is { IsPowered: false, DefaultState: { } defaultState } && component.State != defaultState)
            _itemSwitch.Switch((uid, component), defaultState);
    }

    private void OnMeleeAttack(Entity<ItemSwitchComponent> ent, ref MeleeHitEvent args)
    {
        if (!ent.Comp.NeedsPower
            || !TryComp<BatteryComponent>(ent, out var battery)
            || !ent.Comp.States.TryGetValue(ent.Comp.State, out var state))
            return;

        _battery.TryUseCharge(ent, state.EnergyPerUse, battery);
    }

    private void OnAttemptMelee(EntityUid uid, ItemSwitchComponent component, ref AttemptMeleeEvent args)
    {
        CheckPowerAndSwitchState(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ItemSwitchComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            CheckPowerAndSwitchState(uid, comp);
        }
    }

}
