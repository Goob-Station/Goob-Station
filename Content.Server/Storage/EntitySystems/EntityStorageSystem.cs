// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Bright0 <55061890+Bright0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Lucas <luc4s.rib3iro@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 zero <ribeirolucasdev@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Construction;
using Content.Server.Construction.Components;
using Content.Shared.Materials;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Storage.EntitySystems;

public sealed class EntityStorageSystem : SharedEntityStorageSystem
{
    [Dependency] private readonly ConstructionSystem _construction = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityStorageComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<InsideEntityStorageComponent, InhaleLocationEvent>(OnInsideInhale);
        SubscribeLocalEvent<InsideEntityStorageComponent, ExhaleLocationEvent>(OnInsideExhale);
        SubscribeLocalEvent<InsideEntityStorageComponent, AtmosExposedGetAirEvent>(OnInsideExposed);
        SubscribeLocalEvent<EntityStorageComponent, GotReclaimedEvent>(OnReclaimed); // Goobstation - Recycle update
    }

    private void OnMapInit(EntityUid uid, EntityStorageComponent component, MapInitEvent args)
    {
        if (!component.Open && component.Air.TotalMoles == 0)
        {
            // If we're closed on spawn and have no air already saved, we need to pull some air into our environment from where we spawned,
            // so that we have -something-. For example, if you bought an animal crate or something.
            TakeGas(uid, component);
        }
    }

    protected override void OnComponentInit(EntityUid uid, EntityStorageComponent component, ComponentInit args)
    {
        base.OnComponentInit(uid, component, args);

        if (TryComp<ConstructionComponent>(uid, out var construction))
            _construction.AddContainer(uid, ContainerName, construction);
    }

    protected override void TakeGas(EntityUid uid, EntityStorageComponent component)
    {
        if (!component.Airtight)
            return;

        var tile = GetOffsetTileRef(uid, component);

        if (tile != null && _atmos.GetTileMixture(tile.Value.GridUid, null, tile.Value.GridIndices, true) is { } environment)
        {
            _atmos.Merge(component.Air, environment.RemoveVolume(component.Air.Volume));
        }
    }

    public override void ReleaseGas(EntityUid uid, EntityStorageComponent component)
    {
        if (!component.Airtight)
            return;

        var tile = GetOffsetTileRef(uid, component);

        if (tile != null && _atmos.GetTileMixture(tile.Value.GridUid, null, tile.Value.GridIndices, true) is { } environment)
        {
            _atmos.Merge(environment, component.Air);
            component.Air.Clear();
        }
    }

    private TileRef? GetOffsetTileRef(EntityUid uid, EntityStorageComponent component)
    {
        var targetCoordinates = TransformSystem.ToMapCoordinates(new EntityCoordinates(uid, component.EnteringOffset));

        if (_map.TryFindGridAt(targetCoordinates, out var gridId, out var grid))
        {
            return _mapSystem.GetTileRef(gridId, grid, targetCoordinates);
        }

        return null;
    }

    #region Gas mix event handlers

    private void OnInsideInhale(EntityUid uid, InsideEntityStorageComponent component, InhaleLocationEvent args)
    {
        if (TryComp<EntityStorageComponent>(component.Storage, out var storage) && storage.Airtight)
        {
            args.Gas = storage.Air;
        }
    }

    private void OnInsideExhale(EntityUid uid, InsideEntityStorageComponent component, ExhaleLocationEvent args)
    {
        if (TryComp<EntityStorageComponent>(component.Storage, out var storage) && storage.Airtight)
        {
            args.Gas = storage.Air;
        }
    }

    private void OnInsideExposed(EntityUid uid, InsideEntityStorageComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<EntityStorageComponent>(component.Storage, out var storage))
        {
            if (!storage.Airtight)
                return;

            args.Gas = storage.Air;
        }

        args.Handled = true;
    }

    #endregion
}
