using Content.Shared._Goobstation.Wizard.Spellblade;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Wizard.Spellblade;

[UsedImplicitly]
public sealed class SpellbladeBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private SpellbladeMenu? _menu;

    public SpellbladeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SpellbladeMenu>();
        _menu.SetEntity(Owner);
        _menu.SendSpellbladeSystemMessageAction += SendSpellbladeSystemMessage;

        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    public void SendSpellbladeSystemMessage(ProtoId<SpellbladeEnchantmentPrototype> protoId)
    {
        SendPredictedMessage(new SpellbladeEnchantMessage(protoId));
    }
}
