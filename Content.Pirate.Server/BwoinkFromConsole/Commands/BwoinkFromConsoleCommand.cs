using System;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Network;

namespace Content.Pirate.Server.BwoinkFromConsole.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class BwoinkFromConsoleCommand : IConsoleCommand
{
    [Dependency] private readonly EntityManager EntityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public string Command => "ahelp-bwoink";

    public string Description => "відправити ахелп меседж гравцю";

    public string Help => "ahelp-bwoink <username> <admin_name> <message>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player != null)
        {
            shell.WriteLine("You cannot use this command from the player.");
            return;
        }

        if (args.Length != 3)
        {
            shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        var username = args[0];
        if (string.IsNullOrWhiteSpace(username))
        {
            shell.WriteError("username cannot be empty");
            return;
        }

        var adminName = args[1];
        if (string.IsNullOrWhiteSpace(adminName))
        {
            shell.WriteError("adminName cannot be empty");
            return;
        }

        var message = args[2];
        if (string.IsNullOrWhiteSpace(message))
        {
            shell.WriteError("message cannot be empty");
            return;
        }

        _playerManager.TryGetSessionByUsername(username, out var session);

        if (session is null)
        {
            if (Guid.TryParse(username, out var guid))
            {
                _playerManager.TryGetSessionById(new NetUserId(guid), out session);
            }
        }

        if (session is null)
        {
            shell.WriteError("Player not found");
            return;
        }

        EntityManager.System<BwoinkFromConsoleSystem>().SendBwoink(adminName, session, message);
    }
}
