using Content.Client._Shitcode.Heretic.UI;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Polymorph;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic;

public sealed class HereticShapeshiftBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private HereticShapeshiftRadialMenu? _hereticRitualMenu;

    public HereticShapeshiftBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _hereticRitualMenu = this.CreateWindow<HereticShapeshiftRadialMenu>();
        _hereticRitualMenu.SetEntity(Owner);
        _hereticRitualMenu.SendHereticShapeshiftMessageAction += SendHereticRitualMessage;

        var vpSize = _displayManager.ScreenSize;
        _hereticRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    private void SendHereticRitualMessage(ProtoId<PolymorphPrototype> protoId)
    {
        SendMessage(new HereticShapeshiftMessage(protoId));
    }
}
