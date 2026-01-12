using Robust.Client.UserInterface;
using Content.Goobstation.Shared.Keypad;
using System;

namespace Content.Goobstation.Client.Keypad.UI;

public sealed class KeypadBoundUserInterface : BoundUserInterface
{
    private KeypadMenu? _menu;

    public KeypadBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<KeypadMenu>();

        _menu.OnDigitPressed += d =>
            SendMessage(new KeypadDigitMessage(d));

        _menu.OnClearPressed += () =>
            SendMessage(new KeypadClearMessage());

        _menu.OnEnterPressed += () =>
            SendMessage(new KeypadEnterMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is KeypadUiState keypad)
            _menu?.UpdateState(keypad);
    }
}
