using Content.Client._Shitcode.Heretic.UI;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;

namespace Content.Client._Shitcode.Heretic;

public sealed class EldritchIdBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private EldritchIdRadialMenu? _hereticRitualMenu;

    public EldritchIdBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _hereticRitualMenu = this.CreateWindow<EldritchIdRadialMenu>();
        _hereticRitualMenu.SetEntity(Owner);
        _hereticRitualMenu.SendEldritchIdMessageAction += SendHereticRitualMessage;

        var vpSize = _displayManager.ScreenSize;
        _hereticRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    private void SendHereticRitualMessage(EldritchIdConfiguration config)
    {
        SendPredictedMessage(new EldritchIdMessage(config));
    }
}
