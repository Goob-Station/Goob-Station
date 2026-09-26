using Content.Goobstation.Common.CCVar;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed class VoiceChatGuideUIController : UIController
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly VoiceChatManager _voice = default!;

    private VoiceChatGuideWindow? _window;

    public bool Open(bool prompt = false)
    {
        if (!_voice.RequestLink())
            return false;

        if (_window != null)
        {
            _window.MoveToFront();
            return true;
        }

        var window = new VoiceChatGuideWindow(prompt);
        window.OnClose += () =>
        {
            if (_window == window)
                _window = null;
        };
        window.NeverAskPressed += () =>
        {
            _cfg.SetCVar(GoobCVars.VoiceChatJoinPrompt, false);
            _cfg.SaveToFile();
        };

        _window = window;
        window.OpenCentered();
        return true;
    }

    public void Close()
    {
        _window?.Close();
        _window = null;
    }
}
