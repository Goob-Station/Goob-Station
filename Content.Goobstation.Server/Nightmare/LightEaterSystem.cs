// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Nightmare;
using Content.Goobstation.Shared.Nightmare.Components;
using Content.Server.Light.Components;
using Content.Server.PowerCell;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Light.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Nightmare;

/// <summary>
/// This handles the Light Eater system.
/// Light Eater is an armblade that ashes any light that it attacks.
/// </summary>
public sealed class LightEaterSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightEaterUserComponent, ToggleLightEaterEvent>(OnToggleLightEater);
        SubscribeLocalEvent<LightEaterComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<LightEaterUserComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<LightEaterUserComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<LightEaterUserComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<LightEaterUserComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnToggleLightEater(EntityUid uid, LightEaterUserComponent component, ToggleLightEaterEvent args)
    {
        if (args.Handled)
            return;

        component.Activated = !component.Activated;
        if (!component.Activated)
        {
            var lightEater = Spawn(component.LightEaterProto, Transform(uid).Coordinates);
            component.LightEaterEntity = lightEater;
            if (!_handsSystem.TryPickupAnyHand(uid, lightEater))
            {
                QueueDel(component.LightEaterEntity);
            }
        }
        else if (component.LightEaterEntity != null)
            QueueDel(component.LightEaterEntity);

        args.Handled = true;
    }

    private void OnMeleeHit(EntityUid uid, LightEaterComponent component, MeleeHitEvent args)
    {
        if (args.Handled
            || !args.IsHit
            || !args.HitEntities.Any())
            return;

        foreach (var target in args.HitEntities)
        {
            if (HasComp<PoweredLightComponent>(target))
            {
                Spawn("Ash", Transform(target).Coordinates);
                QueueDel(target);
                continue;
            }

            if (TryComp<InventoryComponent>(target, out var inv))
            {
                foreach (var container in inv.Containers)
                {
                    foreach (var containerItem in container.ContainedEntities)
                    {
                        if (!HasComp<HandheldLightComponent>(containerItem))
                            continue;

                        // not checking for point lights cuz of pda lights
                        Spawn("Ash", Transform(target).Coordinates);
                        QueueDel(containerItem);
                    }
                }
            }

            if (!HasComp<BorgChassisComponent>(target)
                || !_powerCellSystem.TryGetBatteryFromSlot(target, out _))
                continue;

            _powerCellSystem.SetDrawEnabled(target, false);
            args.Handled = true;
            // could add more interactions in the future here
        }
    }
}
