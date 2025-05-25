// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Ninja.Events;
using Content.Server.Power.EntitySystems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Popups;

namespace Content.Server.Ninja.Systems;

public sealed class ItemCreatorSystem : SharedItemCreatorSystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemCreatorComponent, CreateItemEvent>(OnCreateItem);
        SubscribeLocalEvent<ItemCreatorComponent, NinjaBatteryChangedEvent>(OnBatteryChanged);
    }

    private void OnCreateItem(Entity<ItemCreatorComponent> ent, ref CreateItemEvent args)
    {
        var (uid, comp) = ent;
        if (comp.Battery is not {} battery)
            return;

        args.Handled = true;

        var user = args.Performer;
        if (!_battery.TryUseCharge(battery, comp.Charge))
        {
            _popup.PopupEntity(Loc.GetString(comp.NoPowerPopup), user, user);
            return;
        }

        var ev = new CreateItemAttemptEvent(user);
        RaiseLocalEvent(uid, ref ev);
        if (ev.Cancelled)
            return;

        // try to put throwing star in hand, otherwise it goes on the ground
        var star = Spawn(comp.SpawnedPrototype, Transform(user).Coordinates);
        _hands.TryPickupAnyHand(user, star);
    }

    private void OnBatteryChanged(Entity<ItemCreatorComponent> ent, ref NinjaBatteryChangedEvent args)
    {
        if (ent.Comp.Battery == args.Battery)
            return;

        ent.Comp.Battery = args.Battery;
        Dirty(ent, ent.Comp);
    }
}