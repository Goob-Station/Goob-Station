// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Mono.FireControl.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class VisualizeFireDirectionsCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _systemManager = default!;

    public string Command => "visualizefire";
    public string Description => "Toggles visualization of firing directions for a FireControllable entity";
    public string Help => "visualizefire <entity id>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            shell.WriteError("Expected an entity ID argument.");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var entityUid))
        {
            shell.WriteError($"Failed to parse entity ID '{args[0]}'.");
            return;
        }

        if (!_entityManager.TryGetComponent<FireControllableComponent>(entityUid, out _))
        {
            shell.WriteError($"Entity {entityUid} does not have a FireControllable component.");
            return;
        }

        // Get the fire control system
        var fireControlSystem = _systemManager.GetEntitySystem<FireControlSystem>();

        // Toggle visualization
        var enabled = fireControlSystem.ToggleVisualization(entityUid);

        shell.WriteLine(enabled
            ? $"Visualization enabled for entity {entityUid}."
            : $"Visualization disabled for entity {entityUid}.");
    }
}
