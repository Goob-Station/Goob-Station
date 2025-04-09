// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Server.Construction.Commands;

[AdminCommand(AdminFlags.Mapping)]
public sealed class TileWallsCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;

    // ReSharper disable once StringLiteralTypo
    public string Command => "tilewalls";
    public string Description => "Puts an underplating tile below every wall on a grid.";
    public string Help => $"Usage: {Command} <gridId> | {Command}";

    [ValidatePrototypeId<ContentTileDefinition>]
    public const string TilePrototypeId = "Plating";

    [ValidatePrototypeId<TagPrototype>]
    public const string WallTag = "Wall";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        EntityUid? gridId;

        switch (args.Length)
        {
            case 0:
                if (player?.AttachedEntity is not { Valid: true } playerEntity)
                {
                    shell.WriteError("Only a player can run this command.");
                    return;
                }

                gridId = _entManager.GetComponent<TransformComponent>(playerEntity).GridUid;
                break;
            case 1:
                if (!NetEntity.TryParse(args[0], out var idNet) || !_entManager.TryGetEntity(idNet, out var id))
                {
                    shell.WriteError($"{args[0]} is not a valid entity.");
                    return;
                }

                gridId = id;
                break;
            default:
                shell.WriteLine(Help);
                return;
        }

        if (!_entManager.TryGetComponent(gridId, out MapGridComponent? grid))
        {
            shell.WriteError($"No grid exists with id {gridId}");
            return;
        }

        if (!_entManager.EntityExists(gridId))
        {
            shell.WriteError($"Grid {gridId} doesn't have an associated grid entity.");
            return;
        }

        var tagSystem = _entManager.EntitySysManager.GetEntitySystem<TagSystem>();
        var underplating = _tileDefManager[TilePrototypeId];
        var underplatingTile = new Tile(underplating.TileId);
        var changed = 0;
        var enumerator = _entManager.GetComponent<TransformComponent>(gridId.Value).ChildEnumerator;
        while (enumerator.MoveNext(out var child))
        {
            if (!_entManager.EntityExists(child))
            {
                continue;
            }

            if (!tagSystem.HasTag(child, WallTag))
            {
                continue;
            }

            var childTransform = _entManager.GetComponent<TransformComponent>(child);

            if (!childTransform.Anchored)
            {
                continue;
            }

            var mapSystem = _entManager.System<MapSystem>();
            var tile = mapSystem.GetTileRef(gridId.Value, grid, childTransform.Coordinates);
            var tileDef = (ContentTileDefinition)_tileDefManager[tile.Tile.TypeId];

            if (tileDef.ID == TilePrototypeId)
            {
                continue;
            }

            mapSystem.SetTile(gridId.Value, grid, childTransform.Coordinates, underplatingTile);
            changed++;
        }

        shell.WriteLine($"Changed {changed} tiles.");
    }
}