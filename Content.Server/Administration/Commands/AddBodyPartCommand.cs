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
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Systems;
using Content.Shared.Administration;
using Content.Shared.Body.Part;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class AddBodyPartCommand : LocalizedEntityCommands
{
    [Dependency] private readonly BodySystem _bodySystem = default!;

    public override string Command => "addbodypart";
    public override string Description => "Adds a given entity to a containing body.";
    public override string Help => "Usage: addbodypart <entity uid> <body uid> <part slot> <part type> <part symmetry>"; // Shitmed Change: part symmetry

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 5) // Shitmed change
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var childNetId) || !EntityManager.TryGetEntity(childNetId, out var childId))
        {
            shell.WriteError(Loc.GetString("shell-invalid-entity-uid", ("uid", args[0])));
            return;
        }

        if (!NetEntity.TryParse(args[1], out var parentNetId) || !EntityManager.TryGetEntity(parentNetId, out var parentId))
        {
            shell.WriteError(Loc.GetString("shell-invalid-entity-uid", ("uid", args[1])));
            return;
        }

        if (Enum.TryParse<BodyPartType>(args[3], out var partType) &&
            Enum.TryParse<BodyPartSymmetry>(args[4], out var symmetry) && //Shitmed Change: part symmetry
            _bodySystem.TryCreatePartSlotAndAttach(parentId.Value, args[2], childId.Value, partType, symmetry))
        {
            shell.WriteLine($"Added {childId} to {parentId}.");
        }
        else
            shell.WriteError($"Could not add {childId} to {parentId}.");
    }
}
