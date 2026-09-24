using Robust.Shared.Console;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceChatCommand : LocalizedCommands
{
    [Dependency] private readonly VoiceChatManager _voiceChat = default!;

    public override string Command => "voicechat";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!_voiceChat.RequestLink())
        {
            shell.WriteError(Loc.GetString("cmd-voicechat-unavailable"));
            return;
        }

        shell.WriteLine(Loc.GetString("cmd-voicechat-opening"));
    }
}
