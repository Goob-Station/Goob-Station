// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Spawn)]
public sealed class AddItemStorage : LocalizedCommands
{
    private const string CommandName = "additemstorage";
    public override string Command => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var storageSystem = entityManager.System<StorageSystem>();

        if (args.Length < 2)
        {
            shell.WriteLine(Loc.GetString("cmd-additemstorage-args-error"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet)
            || !entityManager.TryGetEntity(targetNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("cmd-additemstorage-bad-target", ("target", args[0])));
            return;
        }
        var target = targetEntity.Value;

        EntityUid item;
        if (NetEntity.TryParse(args[1], out var itemNet)
            && entityManager.TryGetEntity(itemNet, out var itemEntity))
        {
            item = itemEntity.Value;
        }
        else if (prototypeManager.TryIndex(args[1], out var prototype))
        {
            item = entityManager.SpawnEntity(prototype.ID, entityManager.GetComponent<TransformComponent>(target).Coordinates);
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-additemstorage-bad-proto", ("item", args[1])));
            return;
        }


        if (storageSystem.Insert(target, item, out _, playSound: false))
        {
            shell.WriteLine(Loc.GetString("cmd-additemstorage-success",
                    ("item", entityManager.ToPrettyString(item)),
                    ("target", entityManager.ToPrettyString(target))));
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-additemstorage-failure",
                ("item", entityManager.ToPrettyString(item)),
                ("target", entityManager.ToPrettyString(target))));

            entityManager.DeleteEntity(item);
        }

    }
}

