using Content.Goobstation.Client.VoiceChat.UI;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Goobstation.Client.VoiceChat;

public sealed partial class VoiceChatCommand : LocalizedCommands
{
    [Dependency] private IUserInterfaceManager _ui = default!;

    public override string Command => "voicechat";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!_ui.GetUIController<VoiceChatGuideUIController>().Open())
            shell.WriteError(Loc.GetString("cmd-voicechat-unavailable"));
    }
}
