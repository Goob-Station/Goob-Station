using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.VoiceChat;

[AdminCommand(AdminFlags.Moderator)]
public sealed class VoiceMuteCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly VoiceChatSystem _voiceChat = default!;

    public override string Command => "voicemute";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!_player.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        var muted = _voiceChat.ToggleAdminMute(session.UserId);
        shell.WriteLine(Loc.GetString(muted ? "cmd-voicemute-muted" : "cmd-voicemute-unmuted", ("player", session.Name)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromHintOptions(CompletionHelper.SessionNames(players: _player), Loc.GetString("cmd-voicemute-hint"))
            : CompletionResult.Empty;
    }
}
