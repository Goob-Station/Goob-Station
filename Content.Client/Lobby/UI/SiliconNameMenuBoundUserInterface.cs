using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Lobby.UI;

[UsedImplicitly]
public sealed class SiliconNameMenuBoundUserInterface : BoundUserInterface
{
    private SiliconNameMenu? _menu;
    public SiliconNameMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SiliconNameMenu>();
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
