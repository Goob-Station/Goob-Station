using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Pirate.Client.Wetness;

/// <summary>
/// Toggles the wetness debug overlay.
/// </summary>
public sealed class ShowWetnessCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IResourceCache _resource = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public override string Command => "showwetness";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_overlay.HasOverlay<WetnessDebugOverlay>())
        {
            _overlay.RemoveOverlay<WetnessDebugOverlay>();
            shell.WriteLine(Loc.GetString("cmd-showwetness-off"));
        }
        else
        {
            _overlay.AddOverlay(new WetnessDebugOverlay(_entMan, _eye, _resource, _ui));
            shell.WriteLine(Loc.GetString("cmd-showwetness-on"));
        }
    }
}
