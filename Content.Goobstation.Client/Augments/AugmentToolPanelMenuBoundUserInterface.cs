using Content.Goobstation.Shared.Augments;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Augments;

public sealed class AugmentToolPanelMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IEntityManager _ent = default!;

    private AugmentToolPanelMenu? _menu;

    public AugmentToolPanelMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<AugmentToolPanelMenu>();
        _menu.SetEntity(Owner);
        _menu.SendSwitchMessage += SendSwitchMessage;

        // Open the menu, centered on the mouse
        var vpSize = _clyde.ScreenSize;
        _menu.OpenCenteredAt(_input.MouseScreenPosition.Position / vpSize);
    }

    public void SendSwitchMessage(EntityUid? desiredTool)
    {
        SendMessage(new AugmentToolPanelSwitchMessage(_ent.GetNetEntity(desiredTool)));
    }
}
