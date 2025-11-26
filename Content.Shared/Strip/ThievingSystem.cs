// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Strip.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Strip;

public sealed partial class ThievingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThievingComponent, BeforeStripEvent>(OnBeforeStrip);
        SubscribeLocalEvent<ThievingComponent, InventoryRelayedEvent<BeforeStripEvent>>((e, c, ev) =>
            OnBeforeStrip(e, c, ev.Args));
        SubscribeLocalEvent<ThievingComponent, ToggleThievingEvent>(OnToggleStealthy);
        SubscribeLocalEvent<ThievingComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<ThievingComponent, ComponentRemove>(OnCompRemoved);
    }

    private void OnBeforeStrip(EntityUid uid, ThievingComponent component, BeforeStripEvent args)
    {
        args.Stealth |= component.Stealthy;
        if (args.Stealth)
        {
            args.Additive -= component.StripTimeReduction;
        }
    }

    private void OnCompInit(Entity<ThievingComponent> entity, ref ComponentInit args)
    {
        _alertsSystem.ShowAlert(entity, entity.Comp.StealthyAlertProtoId, 1);
    }

    private void OnCompRemoved(Entity<ThievingComponent> entity, ref ComponentRemove args)
    {
        _alertsSystem.ClearAlert(entity, entity.Comp.StealthyAlertProtoId);
    }

    private void OnToggleStealthy(Entity<ThievingComponent> ent, ref ToggleThievingEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Stealthy = !ent.Comp.Stealthy;
        _alertsSystem.ShowAlert(ent.Owner, ent.Comp.StealthyAlertProtoId, (short)(ent.Comp.Stealthy ? 1 : 0));
        DirtyField(ent.AsNullable(), nameof(ent.Comp.Stealthy), null);

        args.Handled = true;
    }
}