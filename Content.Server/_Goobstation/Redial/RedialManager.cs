using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Content.Shared._Goobstation.Redial;

namespace Content.Server._Goobstation.Redial;

public sealed class RedialManager
{
    [Dependency] private readonly IServerNetManager _netManager = default!;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgRedial>();
    }

    public void Redial(INetChannel channel, string address)
    {
        if (!channel.IsConnected)
            return;

        var msg = new MsgRedial();

        msg.Address = address;

        channel.SendMessage(msg);
    }
}

[AdminCommand(AdminFlags.Host)]
public sealed class RedialCommand : IConsoleCommand
{
    public string Command => "redial";
    public string Description => "Redials a player to another server";
    public string Help => "Usage: redial <Player> [Address]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Need at least two arguments");
            return;
        }

        var playerName = args[0];
        var reason = args[1];

        var playerMan = IoCManager.Resolve<IPlayerManager>();
        var redialMan = IoCManager.Resolve<RedialManager>();

        if (!playerMan.TryGetSessionByUsername(playerName, out var player))
        {
            shell.WriteError($"Unable to find player: '{playerName}'.");
            return;
        }

        redialMan.Redial(player.Channel, reason);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Username"),
            _ => CompletionResult.Empty
        };
    }
}
