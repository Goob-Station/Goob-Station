// SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Max <SijyKijy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Systems;
using Content.Shared.Administration;
using Content.Shared.Body.Part;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AddBodyPartCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        public string Command => "addbodypart";
        public string Description => "Adds a given entity to a containing body.";
        public string Help => "Usage: addbodypart <entity uid> <body uid> <part slot> <part type>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 3)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!NetEntity.TryParse(args[0], out var childNetId))
            {
                shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
                return;
            }

            if (!NetEntity.TryParse(args[1], out var parentNetId))
            {
                shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
                return;
            }

            var childId = _entManager.GetEntity(childNetId);
            var parentId = _entManager.GetEntity(parentNetId);
            var bodySystem = _entManager.System<BodySystem>();



            if (Enum.TryParse<BodyPartType>(args[3], out var partType) &&
                bodySystem.TryCreatePartSlotAndAttach(parentId, args[2], childId, partType))
            {
                shell.WriteLine($@"Added {childId} to {parentId}.");
            }
            else
            {
                shell.WriteError($@"Could not add {childId} to {parentId}.");
            }
        }
    }
}