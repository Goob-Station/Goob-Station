// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Chemistry;

[DataDefinition]
public sealed partial class ChangeTileReaction : ITileReaction
{
    [DataField]
    public FixedPoint2 ChangeTileCost { get; private set; } = 4.5f;

    [DataField]
    public string NewTileId = "PlatingRust";

    [DataField]
    public string? OldTileId;

    [DataField]
    public EntProtoId? Effect = "TileHereticRustRune";

    public FixedPoint2 TileReact(TileRef tile,
        ReagentPrototype reagent,
        FixedPoint2 reactVolume,
        IEntityManager entityManager,
        List<ReagentData>? data = null)
    {
        if (reactVolume < ChangeTileCost)
            return FixedPoint2.Zero;

        var gridUid = tile.GridUid;
        var gridIndices = tile.GridIndices;

        if (!entityManager.TryGetComponent(gridUid, out MapGridComponent? mapGrid))
            return FixedPoint2.Zero;

        var tileDefManager = IoCManager.Resolve<ITileDefinitionManager>();
        var tileDef = tile.Tile.GetContentTileDefinition(tileDefManager);

        if (tileDef.ID == NewTileId)
            return FixedPoint2.Zero;

        if (OldTileId != null && tileDef.ID != OldTileId)
            return FixedPoint2.Zero;

        var newTileDef = tileDefManager[NewTileId];
        entityManager.System<MapSystem>().SetTile(gridUid, mapGrid, tile.GridIndices, new Tile(newTileDef.TileId));

        if (Effect != null)
            entityManager.SpawnEntity(Effect.Value, new EntityCoordinates(gridUid, gridIndices));

        return ChangeTileCost;
    }
}
