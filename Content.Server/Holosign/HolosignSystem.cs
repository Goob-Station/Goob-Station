// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ben <50087092+benev0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 BenOwnby <ownbyb@appstate.edu>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.Coordinates.Helpers;
using Content.Server.Power.Components;
using Content.Server.PowerCell;
using Content.Shared.Interaction;
using Content.Shared.Physics; // Goobstation
using Content.Shared.Storage;
using Content.Shared.Tag; // Goobstation
using Robust.Shared.Map; // Goobstation
using Robust.Shared.Physics.Components; // Goobstation

namespace Content.Server.Holosign;

public sealed class HolosignSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    // Goobstation start
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    // Goobstation end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HolosignProjectorComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
        SubscribeLocalEvent<HolosignProjectorComponent, ExaminedEvent>(OnExamine);

        _physicsQuery = GetEntityQuery<PhysicsComponent>(); // Goobstation
    }

    private void OnExamine(EntityUid uid, HolosignProjectorComponent component, ExaminedEvent args)
    {
        // TODO: This should probably be using an itemstatus
        // TODO: I'm too lazy to do this rn but it's literally copy-paste from emag.
        _powerCell.TryGetBatteryFromSlot(uid, out var battery);
        var charges = UsesRemaining(component, battery);
        var maxCharges = MaxUses(component, battery);

        using (args.PushGroup(nameof(HolosignProjectorComponent)))
        {
            args.PushMarkup(Loc.GetString("limited-charges-charges-remaining", ("charges", charges)));

            if (charges > 0 && charges == maxCharges)
            {
                args.PushMarkup(Loc.GetString("limited-charges-max-charges"));
            }
        }
    }

    private void OnBeforeInteract(EntityUid uid, HolosignProjectorComponent component, BeforeRangedInteractEvent args)
    {
        // Goob edit start
        if (args.Handled
            || !args.CanReach // prevent placing out of range
            || HasComp<StorageComponent>(args.Target)) // if it's a storage component like a bag, we ignore usage so it can be stored
            return;

        // places the holographic sign at the click location, snapped to grid.
        var coords = args.ClickLocation.SnapToGrid(EntityManager);
        var mapCoords = _transform.ToMapCoordinates(coords);
        var look = _mapManager.TryFindGridAt(mapCoords, out var grid, out var gridComp)
            ? _map.GetAnchoredEntities((grid, gridComp), mapCoords)
            : _lookup.GetEntitiesInRange(mapCoords, 0.1f);
        foreach (var entity in look)
        {
            if (_tag.HasTag(entity, component.HolosignTag))
                return;

            if (!_physicsQuery.TryComp(entity, out var physics))
                continue;

            if ((physics.CollisionLayer |
                 (int) (CollisionGroup.Impassable |
                        CollisionGroup.LowImpassable |
                        CollisionGroup.MidImpassable |
                        CollisionGroup.HighImpassable)) != 0)
                return;
        }
        if (!_powerCell.TryUseCharge(uid, component.ChargeUse, user: args.User)) // if no battery or no charge, doesn't work
            return;
        var holoUid = Spawn(component.SignProto, coords);
        // Goob edit end
        var xform = Transform(holoUid);
        if (!xform.Anchored)
            _transform.AnchorEntity(holoUid, xform); // anchor to prevent any tempering with (don't know what could even interact with it)

        args.Handled = true;
    }

    private int UsesRemaining(HolosignProjectorComponent component, BatteryComponent? battery = null)
    {
        if (battery == null ||
            component.ChargeUse == 0f) return 0;

        return (int) (battery.CurrentCharge / component.ChargeUse);
    }

    private int MaxUses(HolosignProjectorComponent component, BatteryComponent? battery = null)
    {
        if (battery == null ||
            component.ChargeUse == 0f) return 0;

        return (int) (battery.MaxCharge / component.ChargeUse);
    }
}
