using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic.UI;

[UsedImplicitly]
public sealed class CarvingKnifeBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    [NonSerialized] private CarvingKnifeMenu? _menu;

    public CarvingKnifeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<CarvingKnifeMenu>();
        _menu.SetEntity(Owner);
        _menu.SendCarvingKnifeSystemMessageAction += SendMessage;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
    }

    private void SendMessage(ProtoId<RuneCarvingPrototype> protoId)
    {
        SendMessage(new RuneCarvingSelectedMessage(protoId));
    }
}
