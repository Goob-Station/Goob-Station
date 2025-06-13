using System;
using System.Text;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Pirate.Server.Administration.Commands;

[AnyCommand]
public sealed class PlayersCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    public string Command => "players";
    public string Description => "Повертає список усіх гравців на сервері";
    public string Help => "Використання: players";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var sb = new StringBuilder();

        var players = _players.Sessions;
        sb.AppendLine($"{"Нікнейм",20} {"Статус",12} {"Час Гри",14} {"Пінг",9}");
        sb.AppendLine("-----------------------------------------------------------");

        foreach (var p in players)
        {
            sb.AppendLine(string.Format("{0,20} {1,12} {2,14:hh\\:mm\\:ss} {3,9}",
                p.Name,
                p.Status.ToString(),
                DateTime.UtcNow - p.ConnectedTime,
                p.Channel.Ping + "ms"));
        }

        shell.WriteLine(sb.ToString());
    }
}
