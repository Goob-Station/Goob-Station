// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Shared.Placeable;

/// <summary>
/// Tracks placed entities
/// Subscribe to <see cref="ItemPlacedEvent"/> or <see cref="ItemRemovedEvent"/> to do things when items or placed or removed.
/// </summary>
public sealed class ItemPlacerSystem : EntitySystem
{
    [Dependency] private readonly CollisionWakeSystem _wake = default!;
    [Dependency] private readonly PlaceableSurfaceSystem _placeableSurface = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemPlacerComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<ItemPlacerComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnStartCollide(EntityUid uid, ItemPlacerComponent comp, ref StartCollideEvent args)
    {
        if (_whitelistSystem.IsWhitelistFail(comp.Whitelist, args.OtherEntity))
            return;

        if (TryComp<CollisionWakeComponent>(args.OtherEntity, out var wakeComp))
            _wake.SetEnabled(args.OtherEntity, false, wakeComp);

        var count = comp.PlacedEntities.Count;
        if (comp.MaxEntities == 0 || count < comp.MaxEntities)
        {
            comp.PlacedEntities.Add(args.OtherEntity);

            var ev = new ItemPlacedEvent(args.OtherEntity);
            RaiseLocalEvent(uid, ref ev);
        }

        if (comp.MaxEntities > 0 && count >= (comp.MaxEntities - 1))
        {
            // Don't let any more items be placed if it's reached its limit.
            _placeableSurface.SetPlaceable(uid, false);
        }
    }

    private void OnEndCollide(EntityUid uid, ItemPlacerComponent comp, ref EndCollideEvent args)
    {
        if (TryComp<CollisionWakeComponent>(args.OtherEntity, out var wakeComp))
            _wake.SetEnabled(args.OtherEntity, true, wakeComp);

        comp.PlacedEntities.Remove(args.OtherEntity);

        var ev = new ItemRemovedEvent(args.OtherEntity);
        RaiseLocalEvent(uid, ref ev);

        _placeableSurface.SetPlaceable(uid, true);
    }
}

/// <summary>
/// Raised on the <see cref="ItemPlacer"/> when an item is placed and it is under the item limit.
/// </summary>
[ByRefEvent]
public readonly record struct ItemPlacedEvent(EntityUid OtherEntity);

/// <summary>
/// Raised on the <see cref="ItemPlacer"/> when an item is removed from it.
/// </summary>
[ByRefEvent]
public readonly record struct ItemRemovedEvent(EntityUid OtherEntity);