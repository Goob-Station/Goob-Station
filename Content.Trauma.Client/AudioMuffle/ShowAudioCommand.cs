using Robust.Client.Graphics;
using Robust.Shared.Console;

namespace Content.Trauma.Client.AudioMuffle;

public sealed class ShowAudioMuffleCommand : LocalizedCommands
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    public override string Command => "showaudiomuffle";
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_overlayManager.HasOverlay<AudioMuffleOverlay>())
            _overlayManager.RemoveOverlay<AudioMuffleOverlay>();
        else
            _overlayManager.AddOverlay(new AudioMuffleOverlay());
    }
}
