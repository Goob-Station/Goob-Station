// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Content.Shared.Explosion.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Server.Explosion.EntitySystems;

// This part of the system handled send visual / overlay data to clients.
public sealed partial class ExplosionSystem
{
    public void InitVisuals()
    {
        SubscribeLocalEvent<ExplosionVisualsComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, ExplosionVisualsComponent component, ref ComponentGetState args)
    {
        Dictionary<NetEntity, Dictionary<int, List<Vector2i>>> tileLists = new();
        foreach (var (grid, data) in component.Tiles)
        {
            tileLists.Add(GetNetEntity(grid), data);
        }

        args.State = new ExplosionVisualsState(
            component.Epicenter,
            component.ExplosionType,
            component.Intensity,
            component.SpaceTiles,
            tileLists,
            component.SpaceMatrix,
            component.SpaceTileSize);
    }

    /// <summary>
    ///     Constructor for the shared <see cref="ExplosionEvent"/> using the server-exclusive explosion classes.
    /// </summary>
    private EntityUid CreateExplosionVisualEntity(MapCoordinates epicenter, string prototype, Matrix3x2 spaceMatrix, ExplosionSpaceTileFlood? spaceData, IEnumerable<ExplosionGridTileFlood> gridData, List<float> iterationIntensity)
    {
        var explosionEntity = Spawn(null, MapCoordinates.Nullspace);
        var comp = AddComp<ExplosionVisualsComponent>(explosionEntity);

        foreach (var grid in gridData)
        {
            comp.Tiles.Add(grid.Grid.Owner, grid.TileLists);
        }

        comp.SpaceTiles = spaceData?.TileLists;
        comp.Epicenter = epicenter;
        comp.ExplosionType = prototype;
        comp.Intensity = iterationIntensity;
        comp.SpaceMatrix = spaceMatrix;
        comp.SpaceTileSize = spaceData?.TileSize ?? DefaultTileSize;
        Dirty(explosionEntity, comp);

        // Light, sound & visuals may extend well beyond normal PVS range. In principle, this should probably still be
        // restricted to something like the same map, but whatever.
        _pvsSys.AddGlobalOverride(GetNetEntity(explosionEntity));

        var appearance = AddComp<AppearanceComponent>(explosionEntity);
        _appearance.SetData(explosionEntity, ExplosionAppearanceData.Progress, 1, appearance);

        return explosionEntity;
    }
}