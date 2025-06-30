// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Administration;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Commands;

[AdminCommand(AdminFlags.Mapping)]
public sealed class LavalandMappingCommand : IConsoleCommand
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "mappinglavaland";

    public string Description => "Loads lavaland world on a new map. Be careful, this can cause freezes on runtime!";

    public string Help => "mappinglavaland <prototype id> <seed (optional)>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        LavalandMapPrototype? lavalandProto;
        int? lavalandSeed = null;

        switch (args.Length)
        {
            case 0:
                shell.WriteLine(Loc.GetString("Enter Lavaland prototype ID as a first argument"));
                shell.WriteLine(Help);
                return;
            case 1:
                if (!_proto.TryIndex(args[0], out lavalandProto))
                {
                    shell.WriteLine(Loc.GetString("Wrong lavaland prototype!"));
                    return;
                }
                break;
            case 2:
                if (!_proto.TryIndex(args[0], out lavalandProto))
                {
                    shell.WriteLine(Loc.GetString("Wrong lavaland prototype!"));
                    return;
                }

                if (!ushort.TryParse(args[1], out var targetId))
                {
                    shell.WriteLine(Loc.GetString("shell-argument-must-be-number"));
                    return;
                }
                lavalandSeed = targetId;
                break;
            default:
                shell.WriteLine(Loc.GetString("cmd-playerpanel-invalid-arguments"));
                shell.WriteLine(Help);
                return;
        }

        shell.WriteLine("Starting lavaland map...");
        var lavalandSys = _entityManager.System<LavalandPlanetSystem>();

        if (lavalandSys.GetPreloaderEntity() == null)
            lavalandSys.EnsurePreloaderMap();

        if (!lavalandSys.SetupLavalandPlanet(out var lavaland, lavalandProto, lavalandSeed))
            shell.WriteLine("Failed to load lavaland!");

        shell.WriteLine($"Successfully created new lavaland map: {_entityManager.ToPrettyString(lavaland)}");
    }
}